using System.Collections.Generic;
using UnityEngine;

public class CarVisualController : MonoBehaviour
{
    //Wheel containers and wheels are separate
    //This is so we can have wheel yaw (left, right), and pitch(spin)
    // Ask Ethan Arr for further clarification

    [SerializeField] protected Vector3 spinWheelDirection;
    [SerializeField] protected Vector3 turnWheelDirection;

    [SerializeField] private Transform[] _wheelContainers;
    
    [SerializeField] private Transform[] _wheels;

    [SerializeField] private MeshRenderer _vehicleMesh;
    [SerializeField] protected PlayerVehicleController _playerVehicleController;

    [SerializeField] private Material _vehicleTailLightBreaking;
    [SerializeField] private Material _vehicleTailLight;

    [SerializeField] protected List<GameObject> _trails;
    [SerializeField] protected ParticleSystem[] driftParticles;
    [SerializeField] private List<Material> _carMats = new List<Material>();

    protected CustomCarPhysics _vehiclePhysics;
    protected CustomWheels[] PhysicsWheels;
    protected Rigidbody _rb;

    

    public virtual void Init()
    {
        _vehiclePhysics = GetComponent<CustomCarPhysics>();
        PhysicsWheels = _vehiclePhysics.WheelArray;
        _rb = GetComponent<Rigidbody>();
        //_vehicleMesh.GetMaterials(_carMats);
    }

    void Update()
    {
        for (int i = 0; i < _wheelContainers.Length; i++)
        {
            //Rotates the model
            SpinWheels(_wheels[i], _rb);
            
            //Rotates the container
            float rotAngle = PhysicsWheels[i].SteeringAngle;
            TurnWheels(_wheelContainers[i], rotAngle);
        }

        // I have last change so it's not setting the materials every frame, not sure if it's really necassary tho
        // 0 - doing nothing, 1 - breaking, 2 - nitro, 3 - drift
        /*
        if (_playerVehicleController.isBreaking && lastChange != 0)
        {
            _carMats[6] = _vehicleTailLightBreaking;
            _vehicleMesh.SetMaterials(_carMats);
            lastChange = 0;
        }
        
        else if (!_playerVehicleController.isBreaking && lastChange != 1)
        {
            _carMats[6] = _vehicleTailLight;
            _vehicleMesh.SetMaterials(_carMats);
            lastChange = 1;
        }
        */
        emitDriftParticles();
        activateTrails();
        

        
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
