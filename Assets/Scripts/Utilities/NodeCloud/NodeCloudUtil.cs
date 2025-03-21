using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEditor;

public class NodeCloudUtil : MonoBehaviour
{
  private static readonly string txtAreatxt = "DONT EDIT THIS TEXT\nBEGINNING POINT X AND Z VALUE MUST BE CLOSEST TO GLOBAL 0\nWhen Creating NodeCloud, Raycast will use the Y value of BeginningPoint";
  [Header("Basic Functionality")]
  [SerializeField] private LapChecker LapCheckRef;
  [SerializeField] private NodePoint NodepointProto;

  [SerializeField] private float RaycastVisualizationInSeconds = 0.5f;
  [SerializeField] private Transform NodeCloudContainer;

  [Header("Gizmos settings")]
  [Space(15)]

  [SerializeField] private float directionLineLength = 5f;
  [SerializeField] private float BeginAndEndSize = 0.5f;
  [SerializeField] private float NodePointSphereRadius = 5f;

  [SerializeField] private bool DrawNodeCloudGizmos;
  [SerializeField] private bool DrawNodePointNextCheckpoint = true;
  [SerializeField] private bool DrawNodePointOptimalDir = true;

  [SerializeField] private bool DrawNodePointNext = true;

  [Space(15)]

  [SerializeField] private bool DrawRaycastCircleLines = false;
  [SerializeField] private bool DrawNearestCheckpointLines = false;

  [Space(15)]

  [SerializeField] private Color BegginingAndEndpointColor = Color.red;

  [SerializeField] private Color NodePointColor = Color.white;

  [SerializeField] private Color MissedRoadRacyastVisualizationColor = Color.blue;
  [SerializeField] private Color HitRoadRacyastVisualizationColor = Color.green;

  [SerializeField] private Color RaycastVisNextCheckpointColor = Color.white;
  [SerializeField] private Color RaycastVisOptimalDirColor = Color.yellow;

  [Header("Juicy Values")]
  [SerializeField] List<NodePoint> NodeCloud;

  [Header("Node Creation Settings")]

  [SerializeField] private LayerMask drivableLayers;
  [SerializeField] private LayerMask boundaryCollisionLayers;
  [TextArea]
  public string BeginAndEndpointNote = "DONT EDIT THIS TEXT\nBEGINNING POINT X AND Z VALUE MUST BE CLOSEST TO GLOBAL 0\nWhen Creating NodeCloud, Raycast will use the Y value of BeginningPoint ";
  [SerializeField] private Transform BeginningPoint;

  [SerializeField] private Transform Endpoint;

  [Tooltip("The distance Before creating another node on X axis from BeginingPoint to Endpoint")]
  [SerializeField] private float XOffset = 0.5f;

  [Tooltip("The distance Before creating another node on Z axis from BeginingPoint to Endpoint")]
  [SerializeField] private float ZOffset = 0.5f;
  [Tooltip("The Height the node will float above ground if it collides.")]
  [SerializeField] private float HeightAboveCollidedGround = 2.5f;
  [Header("Node Baking Settings")]
  [SerializeField] private float AngleBetweenRaycastsInDegrees = 25f;
  [SerializeField] private float BoundaryLayerCheckDistance = 2f;

  private bool proceedAfterRaycastingInCircleRouting = false;
  private bool proceedAfterGetttingDirectionToClosestCheckpoint = false;
  private bool proceedAfterNextCheckpoint = false;
  public void CreateNodeCloud()
  {
    EditorUtility.SetDirty(this);
    Debug.ClearDeveloperConsole();
    BeginAndEndpointNote = txtAreatxt;
    Debug.Log("Console Cleared.");
    DeleteNodeCloud();
    Debug.Log("Beginning Node Cloud Creation");
    var id = StartCoroutine(NodeCloudRaycast());
  }
  private IEnumerator NodeCloudRaycast()
  {
    Debug.Log("Starting Racyast's");

    Vector3 AccumulatedPosition = BeginningPoint.position;
    AccumulatedPosition.y = 0;

    float RaycastYPos = BeginningPoint.position.y;
    AccumulatedPosition.y = RaycastYPos;

    Vector3 setPosition = Vector3.zero;
    //EditorApplication.update;
    for (; AccumulatedPosition.x <= Endpoint.position.x; AccumulatedPosition.x += XOffset)
    {
      for (; AccumulatedPosition.z <= Endpoint.position.z; AccumulatedPosition.z += ZOffset)
      {
        bool hitDrivableRoad = Physics.Raycast(AccumulatedPosition, Vector3.down, out RaycastHit hit, 1000f, drivableLayers);
        Color c = hitDrivableRoad == true ? HitRoadRacyastVisualizationColor : MissedRoadRacyastVisualizationColor;
        Debug.DrawRay(AccumulatedPosition, Vector3.down * 1000f, c, RaycastVisualizationInSeconds);
        if (hitDrivableRoad)
        {
          var NC = GameObject.Instantiate(NodepointProto);
          setPosition = AccumulatedPosition;
          setPosition.y = hit.point.y + HeightAboveCollidedGround;
          NC.transform.position = setPosition;
          NC.transform.SetParent(NodeCloudContainer);
          NodeCloud.Add(NC);
        }
      }
      AccumulatedPosition.z = BeginningPoint.position.z;
      yield return null;
    }
    Debug.Log("Node Cloud Created! please bake directions");

  }
  public void StartBakeOfNodeCloud()
  {
    var BNCIid = StartCoroutine(BakeNodeCloudInformation());
  }
  private IEnumerator BakeNodeCloudInformation()
  {
    EditorUtility.SetDirty(this);
    Debug.ClearDeveloperConsole();
    BeginAndEndpointNote = txtAreatxt;
    Debug.Log("Console Cleared.");
    Debug.Log("Clearing Past Values...");


    for (int i = 0; i < NodeCloud.Count; i++)
    {
      NodeCloud[i].DirToNearestCheckpoint = Vector3.zero;
      NodeCloud[i].OptimalDrivingDir = Vector3.zero;
      NodeCloud[i].nextCheckpointDir = Vector3.zero;
      NodeCloud[i].wallAvoidDirection = Vector3.zero;
    }

    proceedAfterRaycastingInCircleRouting = false;
    proceedAfterGetttingDirectionToClosestCheckpoint = false;

    Debug.Log("Starting Node Cloud Baking Process...");

    var GDCCHPid = StartCoroutine(GetDirToClosestCheckpointAllNodePoints());
    // Pauses here until processis finished
    while (proceedAfterGetttingDirectionToClosestCheckpoint == false) { yield return null; }

    var rccId = StartCoroutine(RCInCircleCoroutine());
    // Pauses here as well
    while (proceedAfterRaycastingInCircleRouting == false) { yield return null; }

    Debug.Log("Baking Next Checkpoint direction");
    var ncdID = StartCoroutine(SetNextCheckpointCoroutine());
    while (proceedAfterNextCheckpoint == false) { yield return null; }

    Debug.Log("Last pass of OptimalDrivingDir");

    for (int i = 0; i < NodeCloud.Count; i++)
    {

      if (NodeCloud[i].wallAvoidDirection == NodeCloud[i].transform.forward)
      {
        NodeCloud[i].OptimalDrivingDir = (NodeCloud[i].OptimalDrivingDir + NodeCloud[i].nextCheckpointDir + NodeCloud[i].wallAvoidDirection) / 3;
        NodeCloud[i].OptimalDrivingDir.Normalize();
      }
      else
      {
        NodeCloud[i].OptimalDrivingDir = ((NodeCloud[i].wallAvoidDirection + (NodeCloud[i].nextCheckpointDir)) / 2).normalized;
      }
    }

    Debug.Log("Node Cloud Baking Process finished.");
  }


  private IEnumerator SetNextCheckpointCoroutine()
  {
    for (int i = 0; i < NodeCloud.Count; i++)
    {
      SetNextCheckpoint(NodeCloud[i]);
      yield return null;
    }
    proceedAfterNextCheckpoint = true;
  }
  private void SetNextCheckpoint(NodePoint np)
  {
    int nextIndex = (np.NearestCheckpointIndex + 1) % LapCheckRef._checkpoints.Length;
    Vector3 nextCheckDelt = LapCheckRef.checkPointLocations[nextIndex] - np.transform.position;

    float nearestDot = Vector3.Dot(np.transform.forward, np.DirToNearestCheckpoint);
    float nextCheckDot = Vector3.Dot(np.transform.forward, nextCheckDelt);

    np.nextCheckpointDir = nearestDot > nextCheckDot ? np.DirToNearestCheckpoint : nextCheckDelt;
    np.nextCheckpointDir.Normalize();
  }
  private IEnumerator RCInCircleCoroutine()
  {
    Debug.Log("Raycasting to look for Boundary collisions.");
    for (int i = 0; i < NodeCloud.Count; i++)
    {
      Vector3 retVal = RaycastInCircleFromNode(NodeCloud[i]);
      NodeCloud[i].wallAvoidDirection = retVal.normalized;
      yield return null;
    }
    proceedAfterRaycastingInCircleRouting = true;
    Debug.Log("Boundary Collison Process Finished");

  }
  private Vector3 RaycastInCircleFromNode(NodePoint np)
  {
    int NumOfIterations = Mathf.FloorToInt(360f / AngleBetweenRaycastsInDegrees);
    Vector3 SummedDirection = Vector3.zero;
    Quaternion baserot = np.transform.rotation;
    Vector3 CurrentRotInDeg = np.transform.rotation.eulerAngles;

    Color c;
    Quaternion setrot = baserot;

    int hitRaycastsCount = 0;

    for (int i = 0; i < NumOfIterations; i++)
    {
      CurrentRotInDeg.y += AngleBetweenRaycastsInDegrees;
      setrot = Quaternion.Euler(CurrentRotInDeg);
      np.transform.rotation = setrot;

      bool hitBoundary = Physics.Raycast(np.transform.position, np.transform.forward, out RaycastHit hit, BoundaryLayerCheckDistance, boundaryCollisionLayers);
      if (hitBoundary) hitRaycastsCount++;
      c = hitBoundary ? MissedRoadRacyastVisualizationColor : HitRoadRacyastVisualizationColor;
      if (DrawRaycastCircleLines) Debug.DrawRay(np.transform.position, np.transform.forward * BoundaryLayerCheckDistance, c, RaycastVisualizationInSeconds);

      SummedDirection += hitBoundary == false ? np.transform.forward : np.transform.forward * -1;
    }
    np.transform.rotation = baserot;
    SummedDirection /= NumOfIterations;
    if (hitRaycastsCount == 0) SummedDirection = np.transform.forward;
    return SummedDirection.normalized;
  }
  private IEnumerator GetDirToClosestCheckpointAllNodePoints()
  {
    Debug.Log("Calculating Direction to nearest Checkpoint from each node in NodeCloud");

    for (int i = 0; i < NodeCloud.Count; i++)
    {
      NodeCloud[i].nearestCheckpointLocation = GetClosestCheckpointFromNode(NodeCloud[i], LapCheckRef.chkPointLoc);
      yield return null;
    }
    proceedAfterGetttingDirectionToClosestCheckpoint = true;
    Debug.Log("Finished Calculating Nearest Checkpoints.");
  }
  private Vector3 GetClosestCheckpointFromNode(NodePoint np, Vector3[] checkpoints)
  {
    float closestDistance = float.MaxValue;
    int indAtClosestDistance = 0;

    for (int i = 0; i < checkpoints.Length; i++)
    {
      float dist = Vector3.Distance(np.transform.position, checkpoints[i]);
      if (dist < closestDistance)
      {
        closestDistance = dist;
        indAtClosestDistance = i;
      }
      if (DrawNearestCheckpointLines) Debug.DrawRay(np.transform.position, checkpoints[i] - np.transform.position, Color.white, RaycastVisualizationInSeconds / 2);
    }
    np.nearestCheckpointLocation = checkpoints[indAtClosestDistance];
    np.NearestCheckpointIndex = indAtClosestDistance;
    np.transform.rotation = LapCheckRef._checkpoints[indAtClosestDistance].transform.rotation;


    Debug.DrawRay(np.transform.position, checkpoints[indAtClosestDistance] - np.transform.position, Color.green, RaycastVisualizationInSeconds);
    Vector3 dirToClosestCheckpoint = (checkpoints[indAtClosestDistance] - np.transform.position).normalized;
    return dirToClosestCheckpoint;
  }



  public void DeleteNodeCloud()
  {
    Debug.Log("Deleting Current Node Cloud");
    if (NodeCloud == null) return;
    for (int i = 0; i < NodeCloud.Count; i++)
    {
      DestroyImmediate(NodeCloud[i].gameObject);
    }
    NodeCloud.Clear();
  }

  public void OnDrawGizmos()
  {
    DrawBeginAndEndPoint();
    DrawNodeCloud();
  }
  void DrawNodeCloud()
  {
    if (DrawNodeCloudGizmos == false) return;
    Gizmos.color = NodePointColor;
    for (int i = 0; i < NodeCloud.Count; i++)
    {
      if (NodeCloud[i] != null)
      {
        Gizmos.DrawSphere(NodeCloud[i].transform.position, NodePointSphereRadius);
        DrawNodePointVectors(NodeCloud[i]);
      }
    }
  }
  void DrawNodePointVectors(NodePoint np)
  {
    if (DrawNodePointOptimalDir) Debug.DrawRay(np.transform.position, np.OptimalDrivingDir * directionLineLength, RaycastVisOptimalDirColor);
    if (DrawNodePointNextCheckpoint) Debug.DrawRay(np.transform.position, np.nextCheckpointDir * directionLineLength, RaycastVisNextCheckpointColor);
  }
  void DrawBeginAndEndPoint()
  {
    Gizmos.color = BegginingAndEndpointColor;

    Vector3 beginandendvec = new(BeginAndEndSize, BeginAndEndSize, BeginAndEndSize);
    if (BeginningPoint != null) Gizmos.DrawCube(BeginningPoint.position, beginandendvec);
    if (Endpoint != null) Gizmos.DrawCube(Endpoint.position, beginandendvec);

  }
}
