using UnityEditor.SearchService;
using UnityEngine;

public class PrettyMenuVehicleController : MonoBehaviour
{
    private CustomCarPhysics ccp;
    private CarVisualController cvc;
    public Transform driveTarget;
    [SerializeField] private float wheelSpeed = 40f;
    [SerializeField] private float _reachedTargetDistance;

    [Header("Debug Info")]

    public float throttleAmt;
    public float turningInput;


    void Start()
    {
        //cam = Camera.main;


        ccp = GetComponent<CustomCarPhysics>();
        cvc = GetComponent<CarVisualController>();

        ccp.Init();
        cvc.Init();

        cvc.activateTrails(true);
    }

    void Update()
    {
        for (int i = 0; i < cvc._wheelModels.Length; i++)
        {
            cvc.SpinWheels(cvc._wheelModels[i], wheelSpeed);
        }
        steerVehicleToDestination(driveTarget.position);
    }
    private void steerVehicleToDestination(Vector3 _steeringPosition)
    {
        throttleAmt = 0f;
        turningInput = 0f;


        float distanceToTarget = Vector3.Distance(transform.position, _steeringPosition);


        //Keep driving

        Vector3 dirToTarget = (_steeringPosition - transform.position).normalized;

        //Calculates wether target is positive on local Z axis or negative
        //Negative value is behind, pos is infront
        float frontBackCheck = Vector3.Dot(transform.forward, dirToTarget);

        if (frontBackCheck > 0)
        {
            throttleAmt = 1f;
        }
        else
        {
            throttleAmt = 0f;
        }

        //float yAngleToTarget = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);

        // Set _turningInput Values below.

        //turningInput = calculateTurnAmount(yAngleToTarget, 5f);

        ccp.setInputs(throttleAmt, 0f);
    }

    private float calculateTurnAmount(float angleToTarget, float threshold)
    {
        float turningAmount = Mathf.Clamp(angleToTarget / threshold, -1, 1);

        return turningAmount;
    }
}




