using UnityEngine;

public class waypointGizmos : MonoBehaviour
{
  [SerializeField] private bool DrawDebug = true;
  [Header("Waypoint Settings")]
  [SerializeField] private float waypointGizmoRadius = 0.25f;
  [SerializeField] private Color waypointColour = Color.yellow;
  [SerializeField] private bool showWaypoint = true;
  [SerializeField] public float circleRadius = 6f;


  [Space(15)]
  [SerializeField] private bool autoRefreshWaypoints = true;
  [SerializeField] private bool circuit = true;

  [SerializeField] private Transform[] Waypoints;

  public Transform[] getWaypoints()
  {
    return Waypoints;
  }

  private void Awake()
  {
    RefreshWaypoints();
  }
  private void RefreshWaypoints()
  {
    Transform[] potentialWaypoints = GetComponentsInChildren<Transform>();
    Waypoints = new Transform[potentialWaypoints.Length - 1];

    for (int i = 1; i < potentialWaypoints.Length; i++)
    {
      Waypoints[i - 1] = potentialWaypoints[i];
    }
  }

  void OnDrawGizmos()
  {
    if (!showWaypoint) return;
    if (autoRefreshWaypoints) RefreshWaypoints();
    if (Waypoints.Length == 0) return;

    Gizmos.color = waypointColour;

    if (DrawDebug) drawWayPoints();
  }
  private void drawWayPoints()
  {
    Vector3 lastWaypointposition = Waypoints[^1].position;

    for (int i = 0; i < Waypoints.Length; i++)
    {
      Transform temp = Waypoints[i];

      Gizmos.DrawSphere(temp.position, waypointGizmoRadius);
      //Gizmos.DrawWireSphere(temp.position, circleRadius);
      temp.name = $"Waypoint {i + 1}";
      if (circuit)
      {
        Gizmos.DrawLine(lastWaypointposition, temp.position);
        lastWaypointposition = temp.position;
      }
      else
      {
        if (i != Waypoints.Length - 1)
        {
          Gizmos.DrawLine(temp.position, Waypoints[i + 1].position);
        }
      }
    }

  }
}
public enum TrackType
{
  optimal,
  middle,
  wide
}
