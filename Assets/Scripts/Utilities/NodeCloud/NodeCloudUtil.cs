using UnityEngine;
using System.Collections.Generic;

public class NodeCloudUtil : MonoBehaviour
{
  [Header("Basic Functionality")]
  [SerializeField] private Color BegginingAndEndpointColor = Color.red;
  [SerializeField] private float BeginAndEndSize = 0.5f;

  [SerializeField] private Transform NodeCloudContainer;
  [SerializeField] private float NodePointSphereRadius = 5f;
  [SerializeField] private Color NodePointColor = Color.white;
  [SerializeField] private NodePoint NodepointProto;

  [SerializeField] private float RaycastVisualizationInSeconds = 0.5f;
  [SerializeField] private Color RacyastVisualizationColor = Color.blue;
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

  public void CreateNodeCloud()
  {
    Debug.ClearDeveloperConsole();
    Debug.Log("Console Cleared.");
    DeleteNodeCloud();
    Debug.Log("Beginning Node Cloud Creation");

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

        Debug.DrawRay(AccumulatedPosition, Vector3.down * 1000f, RacyastVisualizationColor, RaycastVisualizationInSeconds);
        if (hitDrivableRoad)
        {
          var NC = GameObject.Instantiate(NodepointProto);
          setPosition = AccumulatedPosition;
          setPosition.y = hit.point.y;
          NC.transform.position = setPosition;
          NC.transform.SetParent(NodeCloudContainer);
          NodeCloud.Add(NC);
        }
      }
      AccumulatedPosition.z = BeginningPoint.position.z;
    }
    Debug.Log("Node Cloud Created! please bake directions");
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
        // Implement DirToNextCheckpoint and OptimalDrivingDir here.
      }
    }
  }
  void DrawBeginAndEndPoint()
  {
    Gizmos.color = BegginingAndEndpointColor;

    Vector3 beginandendvec = new(BeginAndEndSize, BeginAndEndSize, BeginAndEndSize);
    if (BeginningPoint != null) Gizmos.DrawCube(BeginningPoint.position, beginandendvec);
    if (Endpoint != null) Gizmos.DrawCube(Endpoint.position, beginandendvec);

  }
}
