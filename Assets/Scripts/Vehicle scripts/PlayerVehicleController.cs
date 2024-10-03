using UnityEngine;

public class PlayerVehicleController : I_VehicleController
{

    protected KeyCode _nitroKey = KeyCode.LeftShift;
    protected KeyCode _breakKey = KeyCode.Space;
    protected KeyCode _abilityKey = KeyCode.F;

    private bool isUsingNitro;
    private bool isUsingDrift;

    void Update()
    {
        playerControlsLogic();
    }
    /// <summary>
    /// Use in Update for child classes.
    /// Does all the frame by frame input
    /// </summary>
    protected virtual void playerControlsLogic()
    {
        groundCheck();

        float throttleInput = Input.GetAxisRaw("Vertical");
        float turningInput = Input.GetAxisRaw("Horizontal");

        isUsingNitro = Input.GetKey(_nitroKey) && _nitroAmount > 0;

        isUsingDrift = Input.GetKey(_breakKey);

        if (isUsingNitro) _nitroAmount -= 10 * Time.deltaTime;

        if (Input.GetKeyDown(_abilityKey)) useCharacterAbility();


        _vehiclePhysics.useNitro(isUsingNitro, _nitroSpeedBoost);
        _vehiclePhysics.driftVehicle(isUsingDrift);
        _vehiclePhysics.setInputs(throttleInput, turningInput);
    }
}
