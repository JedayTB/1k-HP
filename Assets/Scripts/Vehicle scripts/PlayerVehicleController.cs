using UnityEngine;

public class PlayerVehicleController : I_VehicleController
{

    private KeyCode _nitroKey = KeyCode.LeftShift;
    private KeyCode _breakKey = KeyCode.Space;

    void Update()
    {
        float throttleInput = Input.GetAxisRaw("Vertical");
        float turningInput = Input.GetAxisRaw("Horizontal");

        bool isUsingNitro = Input.GetKeyDown(KeyCode.LeftShift);
        

        _vehiclePhysics.useNitro(isUsingNitro, _nitroSpeedBoost);
        
        _vehiclePhysics.setInputs(throttleInput, turningInput);
    }
    
}
