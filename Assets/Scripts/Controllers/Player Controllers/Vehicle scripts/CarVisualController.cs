using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CarVisualController : MonoBehaviour
{
    // Wheel containers and wheels are separate
    // This is so we can have wheel yaw (left, right), and pitch(spin)
    // Ask Ethan Arr for further clarification

    [SerializeField] protected Vector3 spinWheelDirection;
    [SerializeField] protected Vector3 turnWheelDirection;
    [SerializeField] protected Transform[] _wheelContainers;
    [SerializeField] protected Transform[] _wheelModels;
    [SerializeField] protected List<GameObject> _trails;
    [SerializeField] protected ParticleSystem[] driftParticles;
    protected CustomCarPhysics _vehiclePhysics;
    protected CustomWheels[] PhysicsWheels;
    protected Rigidbody _rb;

    // Car models have tires at different heights.
    // offsetTireWithSuspension must take that into consideration.
    protected float[] baseTireRestHeights;

    public virtual void Init()
    {
        _vehiclePhysics = GetComponent<CustomCarPhysics>();
        PhysicsWheels = _vehiclePhysics.WheelArray;
        _rb = GetComponent<Rigidbody>();

        createbaseTireRestHeights();
    }
    protected void createbaseTireRestHeights()
    {
        baseTireRestHeights = new float[_wheelContainers.Length];
        for (int i = 0; i < _wheelContainers.Length; i++)
        {
            baseTireRestHeights[i] = _wheelContainers[i].transform.localPosition.y;
        }
    }
    void Update()
    {
        for (int i = 0; i < _wheelContainers.Length; i++)
        {
            //Rotates the model
            SpinWheels(_wheelModels[i], _rb);

            //Rotates the container
            float rotAngle = PhysicsWheels[i].SteeringAngle;
            TurnWheels(_wheelContainers[i], rotAngle);
            offsetTireWithSuspension(_wheelContainers[i], i);
        }

        emitDriftParticles();
        activateTrails();



    }
    protected void offsetTireWithSuspension(Transform visualWheel, int index)
    {
        Vector3 currentLocalPosition = visualWheel.localPosition;
        float yOffset = baseTireRestHeights[index] + PhysicsWheels[index].SuspensionOffset;
        Vector3 offsetPosition = new(currentLocalPosition.x, yOffset, currentLocalPosition.z);

        visualWheel.localPosition = offsetPosition;
    }
    protected void emitDriftParticles()
    {
        if (_vehiclePhysics.isDrifting)
        {
            for (int i = 0; i < driftParticles.Length; i++)
            {
                driftParticles[i].Emit(3);
            }
        }
    }
    protected void activateTrails()
    {
        if (_vehiclePhysics.isUsingNitro)
        {
            foreach (GameObject trail in _trails)
            {
                trail.gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject trail in _trails)
            {
                trail.gameObject.SetActive(false);
            }
        }
    }
    protected void SpinWheels(Transform wheel, Rigidbody carRb)
    {
        float velocityAtWheelPoint = carRb.GetPointVelocity(wheel.position).z;

        Vector3 rotation = spinWheelDirection * velocityAtWheelPoint;
        wheel.Rotate(rotation);
    }
    protected void TurnWheels(Transform wheel, float rotationAngle)
    {
        Vector3 currentRotation = wheel.localRotation.eulerAngles;

        Vector3 rotationEul = new(currentRotation.x, rotationAngle, currentRotation.z);

        Quaternion rotation = Quaternion.Euler(rotationEul);

        wheel.localRotation = rotation;
    }

}
