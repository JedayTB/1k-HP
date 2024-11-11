using UnityEngine;

public class VehicleAIController : I_VehicleController
{
    [Header("AI Basic setup")]
    [SerializeField] private Vector3 _steeringPosition;
    [SerializeField] private bool _debugOptions = true;
    [SerializeField] private bool _singleTarget = false;
    [SerializeField] private bool _driveVehicle = true;
    [SerializeField] private bool _circuitedpath = true;

    [Header("Steering parametres")]
    [SerializeField] private waypointGizmos[] waypoints;
    private int waypointsArrayLength;
    [SerializeField] private float _reachedTargetDistance = 6f;
    [SerializeField] private float _reverseThreshold = 25f;
    [SerializeField] private float _turningThreshold = 15;
    [SerializeField] private int _currentWaypointIndex = 0;
    [SerializeField] private int _respawnWaypointIndex = 0;

    private float yAngleToTarget;
    
    [Header("Steering Weights")]
    [SerializeField] private float _currentPathingWeight;



    #region public I_Vehicle Methods
    public void Init(waypointGizmos waypointsMiddle, waypointGizmos waypointOptimal, waypointGizmos waypointWide)
    { 
        waypoints = new waypointGizmos[3];

        waypoints[0] = waypointsMiddle;
        waypoints[1] = waypointOptimal;
        waypoints[2] = waypointWide;

        waypointsArrayLength = waypointsMiddle.getWaypoints().Length - 1;

        _vehiclePhysics = GetComponent<CustomCarPhysics>();
        _vehicleVisualController = GetComponent<CarVisualController>();

        _vehiclePhysics.Init();
        _vehicleVisualController.Init();

        _steeringPosition = waypoints[0].transform.position;
    }
    
    public override void respawn()
    {
        base.respawn();
        _currentWaypointIndex = _respawnWaypointIndex;
    }

    public override void setNewRespawnPosition()
    {
        base.setNewRespawnPosition();
        _respawnWaypointIndex = _currentWaypointIndex;
    }
    public override void setNewRespawnPosition(Vector3 newpos)
    {
        base.setNewRespawnPosition(newpos);
        _respawnWaypointIndex = _currentWaypointIndex;
    }
    public override void setNewRespawnPosition(Transform newTransform)
    {
        base.setNewRespawnPosition(newTransform);
        _respawnWaypointIndex = _currentWaypointIndex;
    }
    #endregion
    
    protected override void Update()
    {
        //groundCheck();

        if(_singleTarget == false) updateTarget();

        if(_driveVehicle) steerVehicleToDestination();
    }

    private void updateTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, _steeringPosition);

        //Reached target
        if (distanceToTarget < _reachedTargetDistance)
        {
            getNextSteeringPosition(); 
        }
        
        if (_debugOptions)
        {
            Debug.DrawRay(transform.position, _steeringPosition - transform.position, Color.green);
        }
    }
    private void getNextSteeringPosition()
    {
        //  If got to the end of a path
        //  Without a circuit
        if ((_currentWaypointIndex + 1) == waypointsArrayLength && _circuitedpath == false)
        {
            _driveVehicle = false;
            _steeringPosition = transform.position;
            return;
        }
        //

        //  If got to the end of a circuited path
        if (_currentWaypointIndex == waypointsArrayLength && _circuitedpath == true)
        {
            _currentWaypointIndex = 0;
        }
        //  No condition. Just go to the next waypoint
        else
        {
            _currentWaypointIndex++;
            _currentWaypointIndex = Mathf.Clamp(_currentWaypointIndex,0, waypointsArrayLength);

            Debug.Log($"Waypoint index updated {_currentWaypointIndex}, Time {Time.time}");
        }
        _steeringPosition = getNextWaypointType(_currentWaypointIndex);
    }
    private Vector3 getNextWaypointType(int index)
    {
        Vector3 steerPos;

        int rndOption = Random.Range(0, 3);

        //float circleRadius = waypoints[rndOption].circleRadius;

        //Vector3 waypointPos = waypoints[rndOption].getWaypoints()[index].position;

        //steerPos = (circleRadius * (Vector3) Random.insideUnitCircle) + waypointPos;
        Debug.Log($"Rnd track{rndOption}, waypoint index {index}");
        steerPos = waypoints[rndOption].getWaypoints()[index].position;
        return steerPos;

    }
    //For use in starting screen animation
    public void startDriving()
    {
        _driveVehicle = true;
    }

    private void steerVehicleToDestination()
    {
        _throttleInput = 0f;
        _turningInput = 0f;
        yAngleToTarget = 0f;

        float distanceToTarget = Vector3.Distance(transform.position, _steeringPosition);

        if (distanceToTarget > _reachedTargetDistance)
        {
            //Keep driving

            Vector3 dirToTarget = (_steeringPosition - transform.position).normalized;

            //Calculates wether target is positive on local Z axis or negative
            //Negative value is behind, pos is infront
            float frontBackCheck = Vector3.Dot(transform.forward, dirToTarget);

            if (frontBackCheck > 0)
            {
                _throttleInput = 1f;
            }
            else
            {
                //Go forward if the distance is further then reverse threshold
                _throttleInput = distanceToTarget > _reverseThreshold ? 1 : -1;
            }


            yAngleToTarget = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);

            if (Mathf.Abs(yAngleToTarget) > _turningThreshold)
            {
                _turningInput = yAngleToTarget > 0 ? 1f : -1f;
            }
            else
            {
                _turningInput = 0f;
            }


        }
        else
        {
            //Reached Target
            _throttleInput = _vehiclePhysics.getVelocity() > 15f ? -1 : 0;
            //_carPhysics.Break(); function doesn't exist yet
        }
        _vehiclePhysics.setInputs(_throttleInput, _turningInput);
    }
    private void OnDrawGizmos()
    {
        if (_debugOptions)
        {
            Gizmos.DrawSphere(_steeringPosition, 0.2f);
        }
    }
}
