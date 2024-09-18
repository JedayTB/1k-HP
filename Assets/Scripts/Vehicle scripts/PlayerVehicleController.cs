using UnityEngine;

public class PlayerVehicleController : MonoBehaviour
{
    private CarVisualController _carVisualController;
    private CustomCarPhysics _customCarPhysics;

    void Awake()
    {
        _customCarPhysics = GetComponent<CustomCarPhysics>();
        _carVisualController = GetComponent<CarVisualController>();

        _customCarPhysics.Init();
        _carVisualController.Init();
        Debug.Log("Car finished Initialization");
    }
    void Update()
    {
        float throttleInput = Input.GetAxisRaw("Vertical");
        float turningInput = Input.GetAxisRaw("Horizontal");
        _customCarPhysics.setInputs(throttleInput, turningInput);
    }
}
