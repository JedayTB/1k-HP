using UnityEngine;

public class PlayerVehicleController : I_VehicleController
{
    protected InputManager inputManager;

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

        _throttleInput = inputManager.PlayerThrottleInput;
        _turningInput = inputManager.PlayerTurningInput;

        
        isUsingNitro = inputManager.isUsingNitro && _nitroAmount > 0;

        isUsingDrift = inputManager.isDrifting;
        bool endedDrift = inputManager.endedDrifting;
        if (isUsingNitro) startTurboBoost();

        if (inputManager.usedAbility) useCharacterAbility();
        

        _vehiclePhysics.driftVehicle(isUsingDrift);
        _vehiclePhysics.endedDrifting(endedDrift);
        _vehiclePhysics.setInputs(_throttleInput, _turningInput);
    }
}
