using UnityEngine;

public class waypointGizmos : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Waypouint Settings")]
    public float waypointRadius = 0.25f;
    public Color waypointColour = Color.yellow;
    public float waypointHeight = 1f;
    public bool showWaypoint = true;

    [Header("Label Settings")]
    public Color labelColor = Color.blue;
    public int fontSize = 18;
    public float labelHeight = 1f;
    public bool showLabels = true;

    [Space(15)]
    public bool autoRefreshWaypoints  = true;
    public bool circuit = true;


    public Transform[] Waypoints {get; private set;}
    
    private void  Awake()
    {
        RefreshWaypoints();
    }
    public void RefreshWaypoints(){
        Transform[] potentialWaypoints = GetComponentsInChildren<Transform>();
        Waypoints = new Transform[potentialWaypoints.Length -1];

        for(int i = 1; i < potentialWaypoints.Length; i++){
            Waypoints[i- 1] = potentialWaypoints[i];
        }
    }

    void OnDrawGizmos()
    {
        if(!showWaypoint) return;
        if(autoRefreshWaypoints) RefreshWaypoints();
        if(Waypoints.Length == 0) return;

        Gizmos.color = waypointColour;

        drawWaypPoints();
    }
    private void drawWaypPoints(){
        Vector3 lastWaypointposition = Waypoints[^1].position;

        for(int i = 0; i < Waypoints.Length; i++){
            Transform temp = Waypoints[i];
            
            Gizmos.DrawSphere(temp.position, waypointRadius);
            temp.name = $"Waypoint {i + 1}";
            if(circuit){
                Gizmos.DrawLine(lastWaypointposition, temp.position);
                lastWaypointposition = temp.position;
            }else{
                if(i != Waypoints.Length -1){
                    Gizmos.DrawLine(temp.position, Waypoints[i + 1].position);
                }
            }
        }

    }
}
