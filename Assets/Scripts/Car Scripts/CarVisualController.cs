using UnityEngine;

public class CarVisualController : MonoBehaviour
{
    //Wheel containers and wheels are separate
    //This is so we can have wheel yaw (left, right), and pitch(spin)
    [SerializeField] private Transform[] WheelContainers;
    
    [SerializeField] private Transform[] wheels;
    [SerializeField] private Material carTailLightBreaking;
    [SerializeField] private Material carTailLight;

    private CustomCarPhysics _carPhysics;
    private Rigidbody _rb;

    public void Init()
    {
        _carPhysics = GetComponent<CustomCarPhysics>();
        _rb = GetComponent<Rigidbody>();

        
    }

    void Update()
    {
        for (int i = 0; i < WheelContainers.Length; i++)
        {

            SpinWheels(wheels[i], _rb);
            
            if (i < WheelContainers.Length / 2)
            {
                TurnWheels(WheelContainers[i], _carPhysics.frontTiresRotationAngle);
            }
            else
            {
                TurnWheels(WheelContainers[i], _carPhysics.backTiresRotationAngle);
            }
            
        }
    }
    void SpinWheels(Transform wheel, Rigidbody carRb)
    {
        float velocityAtWheelPoint = carRb.GetPointVelocity(wheel.position).z;
        Vector3 rotation = new(0, velocityAtWheelPoint, 0);
        wheel.Rotate(rotation);
    }
    void TurnWheels(Transform wheel, float rotationAngle)
    {
        Vector3 currentRotation = wheel.rotation.eulerAngles;

        Vector3 rotation = new(currentRotation.x, currentRotation.y, rotationAngle * Time.deltaTime);

        Quaternion rot = Quaternion.Euler(rotation);

        wheel.rotation = rot;
    }
}
