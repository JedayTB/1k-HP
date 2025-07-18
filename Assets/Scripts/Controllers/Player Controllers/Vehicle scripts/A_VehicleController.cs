using UnityEngine;

public delegate void AbilityAction();
/// <summary>
/// Vehicle class for all vehicle controller classes to inherit
/// Includes basic functionality for AI and PlayerVehicles.
/// Cannot be used as a standalone class
/// </summary>
public abstract class A_VehicleController : MonoBehaviour
{
  #region Variables

  [Header("I_VehicleController member's")]
  protected CarVisualController _vehicleVisualController;
  protected CustomCarPhysics _vehiclePhysics;

  public CustomCarPhysics VehiclePhysics { get => _vehiclePhysics; }
  protected Vector3 _respawnPosition;
  protected Quaternion _respawnRotation;

  public float _throttleInput;
  public float _turningInput;
  /////////////////////////////
  ///                       ///
  ///  Race progressions    ///
  ///                       ///
  /////////////////////////////
  public int racePlacement;

  [HideInInspector] public float raceProgression = 0f;
  public bool[] checkpointsPassedThrough;
  public int nextCheckpointIndex = 0;
  public int lapsPassed = 0;
  public bool needsToPassFirstTwice = false;
  //
  [HideInInspector] public bool isDrifting;

  [SerializeField] protected bool _isDebuging = true;
  [SerializeField] protected float _raycastDistance = 5f;

  [HideInInspector] public bool isUsingNitro = false;

  [HideInInspector] public bool isBreaking = false;
  protected bool canNitroAgain = true;
  protected bool isUsingDrift;

  [Header("Nitro Setup")]

  [Tooltip("The amount of nitro boosts the player can use")]
  public int MaxNitroChargeAmounts = 4;
  [HideInInspector] public float _builtUpNitroAmount;
  [HideInInspector] public int _nitroChargeAmounts = 2;

  [Tooltip("Value that keep tracks of when to increment _driftChargeAmounts. counts from 0 - 1, once reached 1 increment.")]
  public float _nitroIncrementThresholdValue = 0f;
  [Tooltip("Scale value for _nitroIncrementThresholdValue. Change based off of tightness of drift and character stats.")]
  [SerializeField] protected float _nitroIncreaseScaler = 0.75f;

  [SerializeField] protected float _nitroTimeLength = 1f;

  protected static float _nitroSpeedMultiplier = 2.5f;
  protected static float nitroAccelTimingMultiplier = 1.75f;

  [Header("Ability Stuffs")]

  protected AbilityAction onAbilityUsed;
  [HideInInspector] public addedAbility currentAbility;
  public Vector3 LightningAimDirection;

  #endregion
  public virtual void Init()
  {
    _vehiclePhysics = GetComponent<CustomCarPhysics>();
    _vehicleVisualController = GetComponent<CarVisualController>();

    _vehiclePhysics.Init();
    _vehicleVisualController?.Init();

    _respawnPosition = transform.position;
    _respawnRotation = transform.rotation;
  }
  //  For inherited script's that take input
  //  Since you can't inherit a method with new params
  //  Fuck OOP sometimes. this ugly as shit
  public virtual void Init(InputManager playerInput)
  {
    Init();
  }

  protected virtual void Update()
  {
    isDrifting = _vehiclePhysics.isDrifting;
    if (isDrifting)
    {
      buildNitro();
    }

    if (_vehicleVisualController.gameObject.activeSelf) _vehicleVisualController?.Process();

  }
  #region Public use Methods
  public virtual void setAsAutoDriveAI()
  {
    Debug.LogError("Not implemented yet");
  }

  public virtual void unflipSelf()
  {
    Debug.LogError("Not Implemented yet");
  }
  public virtual void respawn()
  {
    transform.position = _respawnPosition;
    transform.rotation = _respawnRotation;

    //_vehiclePhysics._rigidBody.freezeRotation = true;

    _vehiclePhysics.setRigidBodyVelocity(Vector3.zero);
    _vehiclePhysics.RigidBody.angularVelocity = Vector3.zero;
    foreach (var w in _vehiclePhysics.WheelArray)
    {
      w.setTimings(0f, 0f);
    }
  }
  public virtual void enlistAbilityAction(AbilityAction action)
  {
    onAbilityUsed = action;
  }
  public virtual void delistAbilityAction(AbilityAction action)
  {
    onAbilityUsed = null;
  }
  public virtual void setNewRespawnPosition(Transform newRespawn)
  {
    _respawnPosition = newRespawn.position;
    _respawnRotation = newRespawn.rotation;
  }
  public virtual void addNitro()
  {
    _nitroChargeAmounts++;
    _nitroChargeAmounts = Mathf.Clamp(_nitroChargeAmounts, 0, MaxNitroChargeAmounts);
  }

  #endregion

  #region protected virtual methods
  //Protected virtual methods
  public virtual void useCharacterAbility()
  {
    if (onAbilityUsed != null)
    {
      onAbilityUsed?.Invoke();
    }
    else
    {
      Debug.Log($"{this.name} has no ability to used. Forgot to enlist?");
    }
  }


  protected virtual void startNitroBoost()
  {
    if (_nitroChargeAmounts != 0)
    {
      _nitroChargeAmounts--;
      _builtUpNitroAmount -= 1;
      StartCoroutine(_vehiclePhysics.useNitro(_nitroSpeedMultiplier, _nitroTimeLength, nitroAccelTimingMultiplier));
    }
  }
  protected void buildNitro()
  {


    _nitroIncrementThresholdValue += (_nitroIncreaseScaler * Time.deltaTime) * Mathf.Max(Mathf.Abs(_turningInput), 0.25f);
    _builtUpNitroAmount += _nitroIncrementThresholdValue;
    Mathf.Clamp(_builtUpNitroAmount, 0, MaxNitroChargeAmounts);

    if (_nitroIncrementThresholdValue > 1f)
    {
      addNitro();
      _nitroIncrementThresholdValue = 0;
    }


  }
  //End of protected virtual methods

  protected virtual void OnTriggerEnter(Collider other)
  {
    var collectable = other.GetComponent<Collectables>();
    /*
    // Unity object's should not use propogation
    // Well your mother soon to be full of lacerations
    // Sure, it's uncool to me the emancipation of a life 
    // Gunned - hope it not be in this nasty nation
    // - Ethan arrazola
    // Real reason is because Unity doesn't set objects to actual null
    // instead, It set's it as unity null or "due for destruction" before actually deleting.
    // Regardless. This is a useless paragraph. fuck it, though
    */
    collectable?.onPickup(this);
  }
  #endregion
}
