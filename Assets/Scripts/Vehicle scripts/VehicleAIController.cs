using UnityEngine;

public class VehicleAIController : I_VehicleController
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private bool _debugOptions = true;
    [SerializeField] private bool _singleTarget = false;
    [SerializeField] private bool _driveVehicle = true;

    [Header("Steering parametres")]
    [SerializeField] private float _reachedTargetDistance = 6f;
    [SerializeField] private float _reverseThreshold = 25f;
    [SerializeField] private float _turningThreshold = 15;

    [SerializeField] private Transform[] _wayPoints;
    [SerializeField] private int _currentWaypointIndex = 0;
    [SerializeField] private int _respawnWaypointIndex = 0;

    float yAngleToTarget;


    public void Init(waypointGizmos waypoints)
    {
        if(_singleTarget == false) _wayPoints = waypoints.getWaypoints();

        _vehiclePhysics = GetComponent<CustomCarPhysics>();
        _vehicleVisualController = GetComponent<CarVisualController>();

        _vehiclePhysics.Init();
        _vehicleVisualController.Init();

        _targetTransform = _wayPoints[0].transform;
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
    //For use in starting screen animation
    public void startDriving()
    {
        _driveVehicle = true;
    }
    void Update()
    {
        groundCheck();

        if(_singleTarget == false) updateTarget();

        if(_driveVehicle) steerVehicleToDestination();
    }

    private void updateTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position,_targetTransform.position );

        //Reached target
        if (distanceToTarget < _reachedTargetDistance)
        {
            _targetTransform = _wayPoints[_currentWaypointIndex].transform;

            if (_currentWaypointIndex + 1 == _wayPoints.Length) {
                Debug.Log("Reached end of guides");
                _driveVehicle = false;
                return;
            }

            _currentWaypointIndex++;
        }
        else
        {
            _targetTransform = _wayPoints[_currentWaypointIndex].transform;
        }

        if (_debugOptions)
        {
            Debug.DrawRay(transform.position, _targetTransform.position - transform.position, Color.green);
        }
    }

    private void steerVehicleToDestination()
    {
        _throttleInput = 0f;
        _turningInput = 0f;
        yAngleToTarget = 0f;

        float distanceToTarget = Vector3.Distance(transform.position, _targetTransform.position);

        if (distanceToTarget > _reachedTargetDistance)
        {
            //Keep driving

            Vector3 dirToTarget = (_targetTransform.position - transform.position).normalized;

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
}
