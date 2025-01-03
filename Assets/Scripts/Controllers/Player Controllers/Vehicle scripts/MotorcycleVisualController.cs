using UnityEngine;

public class MotorcycleVisualController : CarVisualController
{
    [Header("Motorcycle Specific ")]
    [SerializeField] private Vector3 rollDirection = Vector3.zero;
    [SerializeField] private Transform ModelParentTransform;
    Vector3 cachedModelLocalRotation;
    [SerializeField] private float _maxVisualizeLean;
    [SerializeField] private float _minVisualizeLean;
    [SerializeField] private float _leanCircle;

    private float wheelBase;
    private float turnRadius;
    public override void Init()
    {
        base.Init();
        wheelBase = _vehiclePhysics.wheelbase;
        turnRadius = _vehiclePhysics.turnRadius;

        _leanCircle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / turnRadius);
    }

    // Update is called once per frame
    void Update()
    {
        float wheelVelOne = _rb.GetPointVelocity(PhysicsWheels[0].transform.position).z;
        float wheelVelTwo = _rb.GetPointVelocity(PhysicsWheels[1].transform.position).z;

        SpinWheels(_wheelModels[0], wheelVelOne);
        SpinWheels(_wheelModels[1], wheelVelTwo);

        offsetTireWithSuspension(_wheelContainers[0], 0);
        offsetTireWithSuspension(_wheelContainers[1], 1);

        applyModelRoll(_vehiclePhysics.WheelArray[0]);
        
        bool useDriftParticles = _vehiclePhysics.isDrifting;
        bool useTrails = _vehiclePhysics.isUsingNitro;

        emitDriftParticles(useDriftParticles);
        activateTrails(useTrails);
    }
    // No need to ease roll. 
    // calculation is already based off an eased value
    void applyModelRoll(CustomWheels wheel)
    {
        cachedModelLocalRotation = ModelParentTransform.localRotation.eulerAngles;

        //Use LeftAckermanAngle because Motorcycle only has 1 steering wheel

        float turnProgress01 = wheel.SteeringAngle / wheel.LeftAckermanAngle;

        float zRoll = turnProgress01 * _leanCircle;

        zRoll = Mathf.Clamp(zRoll, _minVisualizeLean, _maxVisualizeLean) * -1;
        Vector3 zrollVec = rollDirection * zRoll;
        cachedModelLocalRotation = zrollVec;
        ModelParentTransform.localRotation = Quaternion.Euler(cachedModelLocalRotation);
    }

}
