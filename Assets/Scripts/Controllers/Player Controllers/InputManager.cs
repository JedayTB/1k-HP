using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System;
public enum ControlSchemes
{
  KeyboardMouse, Gamepad,
}
public delegate void inputControlSchemeChanged(ControlSchemes newScheme);
public class InputManager : MonoBehaviour
{
  public inputControlSchemeChanged onInputSchemeChanged;
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

  private InputUser user;

  public static event Action<ControlSchemes> OnInputSchemeChanged;
  public static ControlSchemes CurrentControlScheme { get; private set; }
  public void Awake()
  {
    Init();
  }
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
    InputThingsICopiedFromStackOverflow();
  }
  private void OnEnable()
  {
    InputUser.onChange += InputUser_onChange;
  }
  private void OnDisable()
  {
    InputUser.onChange -= InputUser_onChange;
  }

  private void InputUser_onChange(InputUser arg1, InputUserChange arg2, InputDevice arg3)
  {
    //print($"Input user changed {arg1.controlScheme}");

    if (arg2 == InputUserChange.ControlSchemeChanged)
    {
      Debug.Log(arg1.controlScheme);
    }
  }

  private void InputThingsICopiedFromStackOverflow()
  {
    user = InputUser.CreateUserWithoutPairedDevices();
    ++InputUser.listenForUnpairedDeviceActivity;
    InputUser.onUnpairedDeviceUsed += (ctrl, eventPtr) =>
    {
      var device = ctrl.device;

      if ((CurrentControlScheme == ControlSchemes.KeyboardMouse) &&
                       ((device is Pointer) || (device is Keyboard)))
      {
        //InputUser.PerformPairingWithDevice(device, user);
        //if (OnInputSchemeChanged != null) OnInputSchemeChanged(ControlSchemes.KeyboardMouse);
        SetUserControlScheme(ControlSchemes.KeyboardMouse);
        return;
      }

      if (device is Gamepad)
      {
        //if (OnInputSchemeChanged != null) OnInputSchemeChanged(ControlSchemes.Gamepad);
        //CurrentControlScheme = ControlSchemes.Gamepad;
        SetUserControlScheme(ControlSchemes.Gamepad);
      }
      else if ((device is Keyboard) || (device is Pointer))
      {
        //if (OnInputSchemeChanged != null) OnInputSchemeChanged(ControlSchemes.KeyboardMouse);
        //CurrentControlScheme = ControlSchemes.KeyboardMouse;
        SetUserControlScheme(ControlSchemes.KeyboardMouse);
      }
      else return;

      //input_source.user.UnpairDevices();
      //InputUser.PerformPairingWithDevice(device, user);
    };
    //input_source.user.UnpairDevices();
  }
  public void SetUserControlScheme(ControlSchemes scheme)
  {
    /*IMPORTANT NOTE - For this to work correctly, your InputSchemes within the Input
    Settings must be spelled exactly "KeyboardMouse" and "Gamepad", or you just rename 
    the enum properties of ControlScheme*/
    //print(scheme);
    onInputSchemeChanged?.Invoke(scheme);
    //user.ActivateControlScheme(scheme.ToString());
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

    /*
    debugginString = $"Throttle: {PlayerThrottleInput}\tTurning: {PlayerTurningInput}\n" +
                $"Drifting: {isDrifting}\n" +
                $"Using Ability: {usedAbility}\n" +
                $"Nitro Attempt: {isUsingNitro}\n" +
                $"Gear Shift: {ShiftGearInput}";
                */
  }
}
