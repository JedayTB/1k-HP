using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class InputManager : MonoBehaviour
{
  [SerializeField] private PlayerInput _inputActions;

  private InputAction _Accelerate;
  private InputAction _Turn;
  private InputAction _Drift;
  private InputAction _Ability;
  private InputAction _Nitro;
  private InputAction _GearShift;
    private InputAction _Confirm;
    private InputAction _Deny;
    private InputAction _PauseGame;
  //1D Axes. (Floats)
  public float PlayerThrottleInput;
  public float PlayerTurningInput;


  //Booleans
  public bool isDrifting;
  public bool endedDrifting;
  public bool usedAbility;
  public bool isUsingNitro;
    public bool pressedConfirm;
    public bool pressedDeny;
    public bool pressedPause;

  public bool ShiftGearInput;
  [SerializeField] private string debugginString;
  public void Init()
  {
    _inputActions = new PlayerInput();

    _inputActions.Enable();

    _Accelerate = _inputActions.Driving.Accelerate;
    _Turn = _inputActions.Driving.Turn;
    _GearShift = _inputActions.Driving.GearShift;
    _Drift = _inputActions.Driving.Drift;
    _Ability = _inputActions.Driving.Ability;
    _Nitro = _inputActions.Driving.Nitro;
        _Confirm = _inputActions.Menu.Confirm;
        _Deny = _inputActions.Menu.Deny;
        _PauseGame = _inputActions.Driving.PauseGame;

    InputUser.onChange += InputUser_onChange;
  }

  private void OnDisable()
  {
    InputUser.onChange -= InputUser_onChange;
  }

  private void InputUser_onChange(InputUser arg1, InputUserChange arg2, InputDevice arg3)
  {
    if (arg2 == InputUserChange.ControlSchemeChanged)
    {
      Debug.Log(arg1.controlScheme);
    }
  }

  void Update()
  {
    PlayerThrottleInput = _Accelerate.ReadValue<float>();
    PlayerTurningInput = _Turn.ReadValue<float>();

    isDrifting = _Drift.IsPressed();

    endedDrifting = _Drift.WasReleasedThisFrame();

    usedAbility = _Ability.IsPressed();
    isUsingNitro = _Nitro.IsPressed();

    ShiftGearInput = _GearShift.WasPerformedThisFrame();

        pressedConfirm = _Confirm.WasPerformedThisFrame();
        pressedDeny = _Deny.WasPerformedThisFrame();

        pressedPause = _PauseGame.WasPerformedThisFrame();

    debugginString = $"Throttle: {PlayerThrottleInput}\tTurning: {PlayerTurningInput}\n" +
                $"Drifting: {isDrifting}\n" +
                $"Using Ability: {usedAbility}\n" +
                $"Nitro Attempt: {isUsingNitro}\n" +
                $"Gear Shift: {ShiftGearInput}";
  }
}
