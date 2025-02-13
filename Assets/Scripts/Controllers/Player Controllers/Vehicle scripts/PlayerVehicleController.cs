using System.Collections;
using UnityEngine;

public class PlayerVehicleController : A_VehicleController
{
  protected InputManager inputManager;
  protected KeyCode resetInput = KeyCode.R;

  protected bool canUseAbilityAgain = true;
  protected override void Update()
  {
    base.Update();
    if (Input.GetKeyDown(resetInput)) resetPlayer();
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

    if (inputManager.usedAbility && canUseAbilityAgain)
    {
      useCharacterAbility();
    }
    if (gearShiftInput != 0) _vehiclePhysics.ShiftGears(gearShiftInput);

    _vehiclePhysics.driftVehicle(isUsingDrift);
    _vehiclePhysics.endedDrifting(endedDrift);

    _vehiclePhysics.setInputs(_throttleInput, _turningInput);

  }
  public override void useCharacterAbility()
  {
    if (onAbilityUsed != null)
    {
      onAbilityUsed?.Invoke();
      GameStateManager.Instance._uiController.playerUsedAbility(base.currentAbility);
    }
    else
    {
      Debug.Log($"{this.name} has no ability to used. Forgot to enlist?");
    }

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
  protected IEnumerator abilityPresscooldown(float time)
  {
    float count = 0f;
    canUseAbilityAgain = false;
    while (count < time)
    {
      count += Time.deltaTime;
      yield return null;
    }
    canUseAbilityAgain = true;
  }
  protected void resetPlayer()
  {
    transform.position += new Vector3(0, 5f, 0);

    transform.rotation = Quaternion.identity;
    VehiclePhysics.RigidBody.velocity = Vector3.zero;
    StartCoroutine(FreezeRotation(1.5f));

  }
  // I moved it here, Ethan :kissyface:
  public IEnumerator FreezeRotation(float time) // my first coroutine omg are you proud of me :3
  {
    float count = 0;
    VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
    while (count < time)
    {
      count += Time.deltaTime;
      yield return null;
    }
    VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.None;
  }
}
