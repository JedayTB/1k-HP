using UnityEngine;

public class PlayerVehicleController : I_VehicleController
{
    
    void Update()
    {
        float throttleInput = Input.GetAxisRaw("Vertical");
        float turningInput = Input.GetAxisRaw("Horizontal");
        _vehiclePhysics.setInputs(throttleInput, turningInput);
    }
}
