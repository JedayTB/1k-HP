using UnityEngine;

public class CarVisualController : MonoBehaviour
{
    [SerializeField] private Transform[] wheels;
    [SerializeField] private Material carTailLightBreaking;
    [SerializeField] private Material carTailLight;

    private CustomCarPhysics carPhysics;
    private Rigidbody _rb;

    public void Init()
    {
        carPhysics = GetComponent<CustomCarPhysics>();
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        for (int i = 0; i < wheels.Length; i++) {

            RotateWheelsOnZAxis(wheels[i], _rb);
        }
    }
    void RotateWheelsOnZAxis(Transform wheel, Rigidbody carRb) 
    {
        float velocityAtWheelPoint = carRb.GetPointVelocity(wheel.position).z * Time.deltaTime;
        Vector3 rotation = new (0, 0, velocityAtWheelPoint);
        wheel.Rotate(rotation);
    }
}
