using UnityEngine;

public class MotorcycleVisualController : CarVisualController
{
    [SerializeField] private Transform ModelParentTransform;
    Vector3 cachedModelLocalRotation;
    [SerializeField] Transform[] _modelWheels;
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

        _leanCircle = (Mathf.Rad2Deg * Mathf.Atan(wheelBase / turnRadius));
    }

    // Update is called once per frame
    void Update()
    {
        SpinWheels(_modelWheels[0], _rb);
        SpinWheels(_modelWheels[1], _rb);

        applyModelRoll(_vehiclePhysics.WheelArray[0]);

        emitDriftParticles();
        activateTrails();
    }

    void applyModelRoll(CustomWheels wheel)
    {
        cachedModelLocalRotation = ModelParentTransform.localRotation.eulerAngles;

        //Use LeftAckermanAngle because Motorcycle only has 1 steering wheel
        
        float turnProgress01 = wheel.SteeringAngle / wheel.LeftAckermanAngle;

        float zRoll = turnProgress01 * _leanCircle;

        zRoll = Mathf.Clamp(zRoll, _minVisualizeLean, _maxVisualizeLean) * -1;
        ModelParentTransform.localRotation = Quaternion.Euler(cachedModelLocalRotation.x, cachedModelLocalRotation.y, zRoll);
    }

}
