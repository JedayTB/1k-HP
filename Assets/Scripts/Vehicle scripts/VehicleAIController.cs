using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class VehicleAIController : MonoBehaviour
{
    [SerializeField] private Transform _targetPositionTransform;
    [SerializeField] private float _reachedTargetDistance = 6f;
    [SerializeField] private float _reverseThreshold = 25f;
    [SerializeField] private float _turningThreshold = 15;
    private CustomCarPhysics _carPhysics;
    private Vector3 _targetPosition;

    float throttleAmt = 0f;
    float turningAmt = 0f;

    [SerializeField] float yAngleToTarget;
    void Awake()
    {
        _carPhysics = GetComponent<CustomCarPhysics>();

        _carPhysics.Init();
    }
    void Update()
    {
        _targetPosition = _targetPositionTransform.position;

        throttleAmt = 0f;
        turningAmt = 0f;
        yAngleToTarget = 0f;
        float distanceToTarget = Vector3.Distance(transform.position, _targetPosition);

        if (distanceToTarget > _reachedTargetDistance)
        {
            //Keep driving

            Vector3 dirToTarget = (_targetPosition - transform.position).normalized;

            //Calculates wether target is positive on local Z axis or negative
            //Negative value is behind, pos is infront
            float frontBackCheck = Vector3.Dot(transform.forward, dirToTarget);

            if(frontBackCheck > 0){
                throttleAmt = 1f;
            }else{
                //Go forward if the distance is further then reverse threshold
                throttleAmt = distanceToTarget > _reverseThreshold ? 1: -1;
            }
            

            yAngleToTarget = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);
            if(Mathf.Abs(yAngleToTarget) > _turningThreshold){
                turningAmt = yAngleToTarget > 0 ? 1f : -1f;
            }else{
                turningAmt = 0f;
            }
            
            
        }else{
            //Reached Target
            throttleAmt = _carPhysics.GetSpeed() > 15f ? -1: 0;
            //_carPhysics.Break(); function doesn't exist yet
        }
        _carPhysics.setInputs(throttleAmt, turningAmt);
    }

}
