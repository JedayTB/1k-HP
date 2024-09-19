using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleAIController : MonoBehaviour
{
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private bool _usingWaypoints = true;
    [SerializeField] private bool _debugOptions = true;
    [Header("Steering parametres")]
    [SerializeField] private float _reachedTargetDistance = 6f;
    [SerializeField] private float _reverseThreshold = 25f;
    [SerializeField] private float _turningThreshold = 15;

    [SerializeField] private Transform[] _wayPoints;
    [SerializeField] private int _currentWaypointIndex = 0;
    private CustomCarPhysics _carPhysics;

    float throttleAmt = 0f;
    float turningAmt = 0f;

    float yAngleToTarget;


    public void Init(waypointGizmos waypoints)
    {
        _wayPoints = waypoints.getWaypoints();

        _carPhysics = GetComponent<CustomCarPhysics>();

        _carPhysics.Init();

        _targetTransform = _wayPoints[0].transform;
    }
    void Update()
    {
        updateTarget();
        steerVehicleToDestination();
    }

    private void updateTarget()
    {
        float distanceToTarget = Vector3.Distance(transform.position,_targetTransform.position );

        if (distanceToTarget < _reachedTargetDistance)
        {
            _targetTransform = _wayPoints[_currentWaypointIndex].transform;
            _currentWaypointIndex++;
        }
        if (_debugOptions)
        {
            Debug.DrawRay(transform.position, _targetTransform.position - transform.position, Color.green);
        }
    }

    private void steerVehicleToDestination()
    {
        throttleAmt = 0f;
        turningAmt = 0f;
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
                throttleAmt = 1f;
            }
            else
            {
                //Go forward if the distance is further then reverse threshold
                throttleAmt = distanceToTarget > _reverseThreshold ? 1 : -1;
            }


            yAngleToTarget = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);

            if (Mathf.Abs(yAngleToTarget) > _turningThreshold)
            {
                turningAmt = yAngleToTarget > 0 ? 1f : -1f;
            }
            else
            {
                turningAmt = 0f;
            }


        }
        else
        {
            //Reached Target
            throttleAmt = _carPhysics.GetSpeed() > 15f ? -1 : 0;
            //_carPhysics.Break(); function doesn't exist yet
        }
        _carPhysics.setInputs(throttleAmt, turningAmt);
    }
}
