using UnityEngine;

public class PlayerVehicleController : I_VehicleController
{
    protected InputManager inputManager;

    protected KeyCode _nitroKey = KeyCode.LeftShift;
    protected KeyCode _breakKey = KeyCode.Space;
    protected KeyCode _abilityKey = KeyCode.F;

    private bool isUsingNitro;
    private bool isUsingDrift;

    void Update()
    {
        playerControlsLogic();
    }
    public override void Init(InputManager playerInput)
    {
        base.Init();
        this.inputManager = playerInput;
    }
    /// <summary>
    /// Use in Update for child classes.
    /// Does all the frame by frame input
    /// </summary>
    protected virtual void playerControlsLogic()
    {
        //groundCheck();

        float throttleInput = inputManager.PlayerThrottleInput;
        float turningInput = inputManager.PlayerTurningInput;

        
        isUsingNitro = inputManager.isUsingNitro && _nitroAmount > 0;

        isUsingDrift = inputManager.isDrifting;

        if (isUsingNitro) _nitroAmount -= 10 * Time.deltaTime;

        if (inputManager.usedAbility) useCharacterAbility();
        

        _vehiclePhysics.useNitro(isUsingNitro, _nitroSpeedBoost);
        _vehiclePhysics.driftVehicle(isUsingDrift);
        _vehiclePhysics.setInputs(throttleInput, turningInput);
    }
}
