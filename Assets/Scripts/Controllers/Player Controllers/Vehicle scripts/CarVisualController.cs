using System.Collections.Generic;
using UnityEngine;

public class CarVisualController : MonoBehaviour
{
    //Wheel containers and wheels are separate
    //This is so we can have wheel yaw (left, right), and pitch(spin)
    // Ask Ethan Arr for further clarification
    [SerializeField] private Transform[] _wheelContainers;
    
    [SerializeField] private Transform[] _wheels;

    [SerializeField] private MeshRenderer _vehicleMesh;

    [SerializeField] private Material _vehicleTailLightBreaking;
    [SerializeField] private Material _vehicleTailLight;

    [SerializeField] private List<Material> _tailLightColors = new List<Material>();
    [SerializeField] private List<Material> _carMats = new List<Material>();

    private CustomCarPhysics _vehiclePhysics;
    private CustomWheels[] PhysicsWheels;
    private Rigidbody _rb;


    public void Init()
    {
        _vehiclePhysics = GetComponent<CustomCarPhysics>();
        PhysicsWheels = _vehiclePhysics.WheelArray;
        _rb = GetComponent<Rigidbody>();
        //_vehicleMesh?.GetMaterials(_carMats);
    }

    void Update()
    {
        for (int i = 0; i < _wheelContainers.Length; i++)
        {
            //Rotates the model
            SpinWheels(_wheels[i], _rb);
            
            //Rotates the container
            float rotAngle = PhysicsWheels[i].steeringAngle;
            TurnWheels(_wheelContainers[i], rotAngle);
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            _carMats[6] = _tailLightColors[1];
            _vehicleMesh.SetMaterials(_carMats);
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            _carMats[6] = _tailLightColors[0];
            _vehicleMesh.SetMaterials(_carMats);
        }
    }
    void SpinWheels(Transform wheel, Rigidbody carRb)
    {
        Vector3 currentRotation = wheel.localRotation.eulerAngles;

        float velocityAtWheelPoint = carRb.GetPointVelocity(wheel.position).z;

        Vector3 rotation = new(0, velocityAtWheelPoint, 0);
        wheel.Rotate(rotation);
    }
    void TurnWheels(Transform wheel, float rotationAngle)
    {

        Vector3 currentRotation = wheel.localRotation.eulerAngles;
        
        Vector3 rotationEul = new(currentRotation.x, rotationAngle, currentRotation.z);



        Quaternion rotation = Quaternion.Euler(rotationEul);

        wheel.localRotation = rotation;
    }
}
