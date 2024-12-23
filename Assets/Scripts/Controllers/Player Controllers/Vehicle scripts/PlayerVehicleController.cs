using System.Collections;
using UnityEngine;

public class PlayerVehicleController : A_VehicleController
{
  protected InputManager inputManager;

  protected override void Update()
  {
    base.Update();
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
    _throttleInput = inputManager.PlayerThrottleInput;
    _turningInput = inputManager.PlayerTurningInput;

    isUsingNitro = inputManager.isUsingNitro && _nitroChargeAmounts > 0;

    isUsingDrift = inputManager.isDrifting;
    float gearShiftInput = inputManager.ShiftGearInput;
    bool endedDrift = inputManager.endedDrifting;

    if (isUsingNitro && canNitroAgain)
    {
      startNitroBoost();
      StartCoroutine(driftPressCoolDown(0.25f));
    }

    if (inputManager.usedAbility) useCharacterAbility();
    if (gearShiftInput != 0) _vehiclePhysics.ShiftGears(gearShiftInput);

    _vehiclePhysics.driftVehicle(isUsingDrift);
    _vehiclePhysics.endedDrifting(endedDrift);

    _vehiclePhysics.setInputs(_throttleInput, _turningInput);

  }
  protected IEnumerator driftPressCoolDown(float time)
  {
    float count = 0f;
    canNitroAgain = false;
    while (count < time)
    {
      count += Time.deltaTime;
      yield return null;
    }
    canNitroAgain = true;
  }
}
