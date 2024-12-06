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
        _vehiclePhysics = GetComponent<CustomCarPhysics>();
        PhysicsWheels = _vehiclePhysics.WheelArray;
        _rb = GetComponent<Rigidbody>();

        wheelBase = _vehiclePhysics.wheelbase;
        turnRadius = _vehiclePhysics.turnRadius;

        _leanCircle = Mathf.Rad2Deg * Mathf.Atan(wheelBase / turnRadius);
    }

    // Update is called once per frame
    void Update()
    {
        SpinWheels(_wheelModels[0], _rb);
        SpinWheels(_wheelModels[1], _rb);

        offsetTireWithSuspension(_wheelContainers[0], 0);
        offsetTireWithSuspension(_wheelContainers[1], 1);

        applyModelRoll(_vehiclePhysics.WheelArray[0]);

        emitDriftParticles();
        activateTrails();
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
