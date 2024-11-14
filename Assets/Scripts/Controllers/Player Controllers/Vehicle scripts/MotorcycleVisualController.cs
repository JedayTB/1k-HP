using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotorcycleVisualController : CarVisualController
{
    [SerializeField] private Transform ModelParentTransform;
    Vector3 cachedModelLocalRotation;
    [SerializeField] Transform[] _modelWheels;
    
    private CustomCarPhysics _vehiclePhysics;
    private CustomWheels[] PhysicsWheels;
    public Rigidbody _rb;

    private float wheelBase;
    private float turnRadius;
    public override void Init()
    {
        _vehiclePhysics = GetComponent<CustomCarPhysics>();
        PhysicsWheels = _vehiclePhysics.WheelArray;
        _rb = GetComponent<Rigidbody>();

        wheelBase = _vehiclePhysics.wheelbase;
        turnRadius = _vehiclePhysics.turnRadius;
    }

    // Update is called once per frame
    void Update()
    {
        SpinWheels(_modelWheels[0], _rb);
        SpinWheels(_modelWheels[1], _rb);

        applyModelRoll(_vehiclePhysics.WheelArray[0]);
    }

    private void SpinWheels(Transform wheel, Rigidbody rb)
    {
        float velocityAtWheelPoint = rb.GetPointVelocity(wheel.position).z;

        Vector3 rotation = new(0, velocityAtWheelPoint, 0);
        wheel.Rotate(rotation);
    }
    void applyModelRoll(CustomWheels wheel)
    {
        
        float zRoll = Mathf.Rad2Deg * Mathf.Atan(wheelBase / turnRadius) * wheel.SteeringAngle;
        print(zRoll);
        cachedModelLocalRotation = ModelParentTransform.localRotation.eulerAngles;
        ModelParentTransform.localRotation = Quaternion.Euler(cachedModelLocalRotation.x, cachedModelLocalRotation.z, zRoll);
    }

}
