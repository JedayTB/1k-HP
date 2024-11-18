using System.Collections;
using UnityEngine;

public class VehicleAIController : I_VehicleController
{
    #region AI Variables
    [Header("AI Basic setup")]
    [SerializeField] private Vector3 _steeringPosition;
    [SerializeField] protected float SteerPathingClock = 0.15f; 
    [SerializeField] private bool _debugOptions = true;
    [SerializeField] private bool _singleTarget = false;
    [SerializeField] private bool _driveVehicle = true;
    [SerializeField] private bool _circuitedpath = true;

    [Header("Steering parametres")]
    [SerializeField] private waypointGizmos[] _NavigationTracks;
    private int waypointsArrayLength;
    [SerializeField] private float _reachedTargetDistance = 6f;
    [SerializeField] private float _reverseThreshold = 25f;
    [SerializeField] private float _turningThreshold = 15;
    [SerializeField] private int _currentWaypointIndex = 0;
    [SerializeField] private int _respawnWaypointIndex = 0;

    private float yAngleToTarget;
    
    [Header("Steering Weights")]
    [SerializeField] private int _currentTrackOption;

    #endregion

    #region public I_Vehicle Methods
    public void Init(waypointGizmos waypointsMiddle, waypointGizmos waypointOptimal, waypointGizmos waypointWide)
    { 
        _NavigationTracks = new waypointGizmos[3];

        _NavigationTracks[0] = waypointsMiddle;
        _NavigationTracks[1] = waypointOptimal;
        _NavigationTracks[2] = waypointWide;

        waypointsArrayLength = waypointsMiddle.getWaypoints().Length - 1;

        _vehiclePhysics = GetComponent<CustomCarPhysics>();
        _vehicleVisualController = GetComponent<CarVisualController>();

        _vehiclePhysics.Init();
        _vehicleVisualController.Init();

        _steeringPosition = _NavigationTracks[0].getWaypoints()[0].position;

        StartCoroutine(SteerPathing(SteerPathingClock));
    }
    public override void Init()
    {
        _vehiclePhysics = GetComponent<CustomCarPhysics>();
        _vehicleVisualController = GetComponent<CarVisualController>();

        _vehiclePhysics.Init();
        _vehicleVisualController.Init();

        StartCoroutine(SteerPathing(SteerPathingClock));
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

    #region Helper Methods
    //For use in starting screen animation
    public void startDriving()
    {
        _driveVehicle = true;
    }
    #endregion


    IEnumerator SteerPathing(float waitTime)
    {
        while (true)
        {
            if (_singleTarget == false) updateTarget();

            if (_driveVehicle) steerVehicleToDestination();

            yield return new WaitForSeconds(waitTime);
        }


    }

    private void updateTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position, _steeringPosition);

        //Reached target
        if (distanceToTarget < _reachedTargetDistance)
        {

            updateWaypointIndex();
            updateTrackOption();
            _steeringPosition = getPosInsideWaypoint(_currentTrackOption);
        }
        
        if (_debugOptions)
        {
            Debug.DrawRay(transform.position, _steeringPosition - transform.position, Color.green);
        }
    }
    private void updateWaypointIndex()
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

            //Debug.Log($"Waypoint index updated {_currentWaypointIndex}, Time {Time.time}");
        }
    }
    private void updateTrackOption()
    {
        switch (_currentTrackOption)
        {
            case 0: // Middle track
                _currentTrackOption++;
                break;
            case 1: // Optimal          -1, 0, 1
                _currentTrackOption += Random.Range(-1, 2);
                break;
            case 2: // Wide track       // 1, 2
                _currentTrackOption -= Random.Range(1, 3);
                break;
        }
    }
    private Vector3 getPosInsideWaypoint(int trackOption)
    {
        Vector3 steerPos;

        float circleRadius = _NavigationTracks[trackOption].circleRadius;

        Vector3 waypointPos = _NavigationTracks[trackOption].getWaypoints()[_currentWaypointIndex].position;
        Vector3 rndCircleOffset = Random.insideUnitCircle * circleRadius;

        steerPos = new Vector3(rndCircleOffset.x + waypointPos.x, waypointPos.y, rndCircleOffset.z + waypointPos.z);

        //Debug.Log($"Rnd track{rndOption}, waypoint index {index}");

        return steerPos;
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
        Gizmos.color = Color.red;
        if (_debugOptions)
        {
            Gizmos.DrawSphere(_steeringPosition, 1f);
        }
    }
}
