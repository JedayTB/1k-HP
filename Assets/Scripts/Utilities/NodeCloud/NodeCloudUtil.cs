using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NodeCloudUtil : MonoBehaviour
{
  private static readonly string txtAreatxt = "DONT EDIT THIS TEXT\nBEGINNING POINT X AND Z VALUE MUST BE CLOSEST TO GLOBAL 0\nWhen Creating NodeCloud, Raycast will use the Y value of BeginningPoint";
  [Header("Basic Functionality")]
  [SerializeField] private Color BegginingAndEndpointColor = Color.red;
  [SerializeField] private float BeginAndEndSize = 0.5f;

  [SerializeField] private Transform NodeCloudContainer;
  [SerializeField] private float NodePointSphereRadius = 5f;
  [SerializeField] private Color NodePointColor = Color.white;
  [SerializeField] private NodePoint NodepointProto;

  [SerializeField] private float RaycastVisualizationInSeconds = 0.5f;
  [SerializeField] private Color MissedRoadRacyastVisualizationColor = Color.blue;
  [SerializeField] private Color HitRoadRacyastVisualizationColor = Color.green;

  [SerializeField] private Color RaycastVisNextCheckpointColor = Color.white;
  [SerializeField] private Color RaycastVisOptimalDirColor = Color.yellow;
  [SerializeField] private bool DrawNodePointNextCheckpoint = true;
  [SerializeField] private bool DrawNodePointOptimalDir = true;
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

  private List<Vector3> AveragedDirList;
  private List<Vector3> ClosestCheckpointDirList;
  private bool proceedAfterRaycastingInCircleRouting = false;
  private bool proceedAfterGetttingDirectionToClosestCheckpoint = false;
  public void CreateNodeCloud()
  {
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
    Debug.ClearDeveloperConsole();
    BeginAndEndpointNote = txtAreatxt;
    Debug.Log("Console Cleared.");

    Debug.Log("Clearing Past Values...");

    AveragedDirList.Clear();
    ClosestCheckpointDirList.Clear();

    for (int i = 0; i < NodeCloud.Count; i++)
    {
      NodeCloud[i].DirToNearestCheckpoint = Vector3.zero;
      NodeCloud[i].OptimalDrivingDir = Vector3.zero;
    }

    AveragedDirList = new List<Vector3>();
    ClosestCheckpointDirList = new List<Vector3>();

    proceedAfterRaycastingInCircleRouting = false;
    proceedAfterGetttingDirectionToClosestCheckpoint = false;

    Debug.Log("Starting Node Cloud Baking Process.");

    var rccId = StartCoroutine(RCInCircleCoroutine());
    while (proceedAfterRaycastingInCircleRouting == false) { yield return null; }

    var GDCCHPid = StartCoroutine(GetDirToClosestCheckpointAllNodePoints());
    while (proceedAfterGetttingDirectionToClosestCheckpoint == false) { yield return null; }
    Debug.Log("Setting Averaged Direction Vectors to Node Points");
    var sdvID = StartCoroutine(SetDirectionVectors());

    Debug.Log("Node Cloud Baking Process finished.");
  }
  private IEnumerator SetDirectionVectors()
  {
    for (int i = 0; i < NodeCloud.Count; i++)
    {
      if (AveragedDirList[i] != null) NodeCloud[i].OptimalDrivingDir = AveragedDirList[i];
      if (ClosestCheckpointDirList[i] != null) NodeCloud[i].DirToNearestCheckpoint = ClosestCheckpointDirList[i];
      yield return null;
    }
    Debug.Log("Vectors Set.");
  }
  private IEnumerator RCInCircleCoroutine()
  {
    Debug.Log("Raycasting to look for Boundary collisions.");
    Vector3 SummingVector = Vector3.zero;
    for (int i = 0; i < NodeCloud.Count; i++)
    {
      AveragedDirList.Add(RaycastInCircleFromNode(NodeCloud[i]));
      yield return null;
    }
    proceedAfterRaycastingInCircleRouting = true;
    Debug.Log("Boundary Collison Process Finished");

  }
  private Vector3 RaycastInCircleFromNode(NodePoint np)
  {
    int NumOfIterations = Mathf.FloorToInt(360f / AngleBetweenRaycastsInDegrees);
    Vector3 SummedDirection = Vector3.zero;
    Quaternion baserot = Quaternion.identity;
    Vector3 CurrentRotInDeg = np.transform.rotation.eulerAngles;

    Color c;
    Quaternion setrot = baserot;

    for (int i = 0; i < NumOfIterations; i++)
    {
      CurrentRotInDeg.y += AngleBetweenRaycastsInDegrees;
      setrot = Quaternion.Euler(CurrentRotInDeg);
      np.transform.rotation = setrot;

      bool hitBoundary = Physics.Raycast(np.transform.position, np.transform.forward, out RaycastHit hit, BoundaryLayerCheckDistance, boundaryCollisionLayers);
      c = hitBoundary ? MissedRoadRacyastVisualizationColor : HitRoadRacyastVisualizationColor;
      Debug.DrawRay(np.transform.position, np.transform.forward * BoundaryLayerCheckDistance, c, RaycastVisualizationInSeconds);

      SummedDirection += hitBoundary == true ? np.transform.forward * -1 : np.transform.forward;
    }
    np.transform.rotation = baserot;
    return SummedDirection.normalized;
  }
  private IEnumerator GetDirToClosestCheckpointAllNodePoints()
  {
    Debug.Log("Calculating Direction to nearest Checkpoint from each node in NodeCloud");
    Vector3[] LevelCheckpoints = GameStateManager.Instance.levelCheckpointLocations;
    Debug.Log(LevelCheckpoints);

    for (int i = 0; i < NodeCloud.Count; i++)
    {
      ClosestCheckpointDirList.Add(GetClosestCheckpointFromNode(NodeCloud[i], LevelCheckpoints));
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
    }
    return (checkpoints[indAtClosestDistance] - np.transform.position).normalized;
  }



  public void DeleteNodeCloud()
  {
    Debug.Log("Deleting Current Node Cloud");
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
    if (DrawNodePointOptimalDir && np.OptimalDrivingDir != Vector3.zero) Debug.DrawRay(np.transform.position, np.OptimalDrivingDir * BoundaryLayerCheckDistance, RaycastVisOptimalDirColor);
    if (DrawNodePointNextCheckpoint && np.DirToNearestCheckpoint != Vector3.zero) Debug.DrawRay(np.transform.position, np.DirToNearestCheckpoint, RaycastVisNextCheckpointColor);
  }
  void DrawBeginAndEndPoint()
  {
    Gizmos.color = BegginingAndEndpointColor;

    Vector3 beginandendvec = new(BeginAndEndSize, BeginAndEndSize, BeginAndEndSize);
    if (BeginningPoint != null) Gizmos.DrawCube(BeginningPoint.position, beginandendvec);
    if (Endpoint != null) Gizmos.DrawCube(Endpoint.position, beginandendvec);

  }
}
