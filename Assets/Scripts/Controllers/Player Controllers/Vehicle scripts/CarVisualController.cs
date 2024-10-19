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

    private CustomCarPhysics _vehiclePhysics;
    private Rigidbody _rb;

    public void Init()
    {
        _vehiclePhysics = GetComponent<CustomCarPhysics>();
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        for (int i = 0; i < _wheelContainers.Length; i++)
        {
            //Rotates the model
            SpinWheels(_wheels[i], _rb);
            
            //Rotates the container
            float rotAngle = i < _wheelContainers.Length /2 ?  _vehiclePhysics.FrontTiresRotationAngle: _vehiclePhysics.BackTiresRotationAngle;
            TurnWheels(_wheelContainers[i], rotAngle);
            
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
