using UnityEngine;

/// <summary>
/// Vehicle class for all vehicle controller classes to inherit
/// Includes basic functionality for AI and PlayerVehicles.
/// Cannot be used as a standalone class
/// </summary>
public abstract class I_VehicleController : MonoBehaviour
{
    //Cache transform to avoid extern calls
    #region Variables
    protected CarVisualController _vehicleVisualController;
    public CustomCarPhysics _vehiclePhysics;

    protected Vector3 _respawnPosition;
    protected Quaternion _respawnRotation;

    protected float _throttleInput;
    protected float _turningInput;
    
    [HideInInspector] public bool isDrifting;

    protected bool isGrounded = true;
    [Header("I_VehicleController member's")]
    [SerializeField] protected bool _isDebuging = true;
    [SerializeField] protected bool _useGroundCheck = false;
    [SerializeField] protected float _raycastDistance = 5f;
    [SerializeField] protected LayerMask _worldGeometryLayers;
    [SerializeField] protected ParticleSystem[] driftParticles;


    [HideInInspector] public bool isUsingNitro = false;
    protected bool canNitroAgain = true;
    protected bool isUsingDrift;
    [HideInInspector] public bool isBreaking = false;

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
    [SerializeField] protected float _nitroSpeedMultiplier = 2.5f;

    [Header("Character Abilities Basic setup")]

    [Tooltip("Ability Gauge counts to 100. Once at 100, can use ability")]
    [SerializeField] protected int _abilityGauge = 0;
    [SerializeField] protected float _abilityUseTimer = 5f;
    protected float _abilityElapsedTime = 0f;

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
    
    protected virtual void Update(){
        isDrifting = _vehiclePhysics.isDrifting;
        if(isDrifting){
            foreach(var ps in driftParticles){
                ps.Emit(3);
            }
            buildNitro();
        }
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
        Debug.Log(_vehiclePhysics.RigidBody.velocity);
    }
    public virtual void setNewRespawnPosition()
    {
        _respawnPosition = transform.position;
        _respawnRotation = transform.rotation;
    }
    public virtual void setNewRespawnPosition(Vector3 newPos)
    {
        _respawnPosition = newPos;
    }
    public virtual void setNewRespawnPosition(Transform newRespawn)
    {
        _respawnPosition = newRespawn.position;
        _respawnRotation = newRespawn.rotation;
    }
    //End of public methods
    public virtual void addNitro(){
        _nitroChargeAmounts++;
        Mathf.Clamp(_nitroChargeAmounts, 0 , MaxNitroChargeAmounts);
    }
    public virtual void addNitro(int nitroDelta){
        _nitroChargeAmounts += nitroDelta;
        Mathf.Clamp(_nitroChargeAmounts, 0 , MaxNitroChargeAmounts);
    }

    
    #endregion
    //Protected virtual methods
    public virtual void useCharacterAbility()
    {
        if(_abilityGauge >= 100)
        {
            Debug.Log($"{this.name} used their ability!");
        }
        else
        {
            Debug.Log($"ability gauge must be at 100 to use ability.");
        }
        
    }
    #region protected virtual methods
    protected virtual void groundCheck()
    {
        if (_useGroundCheck)
        {
            isGrounded = Physics.Raycast(transform.position, Vector3.down, _raycastDistance, _worldGeometryLayers);

            if (_isDebuging) Debug.DrawRay(transform.position, Vector3.down * _raycastDistance, isGrounded ? Color.green : Color.red);

            //if it's grounded, let rb rotate freely.

            _vehiclePhysics.RigidBody.freezeRotation = !isGrounded;
        }
    }
    protected virtual void startNitroBoost()
    {   
        
        if (_nitroChargeAmounts != 0)
        {
            _nitroChargeAmounts--;
            _builtUpNitroAmount -= 1;
            StartCoroutine(_vehiclePhysics.useNitro(_nitroSpeedMultiplier, _nitroTimeLength));
        }
    }
    protected void buildNitro(){
        _nitroIncrementThresholdValue += _nitroIncreaseScaler * Time.deltaTime;

        _builtUpNitroAmount += _nitroIncrementThresholdValue;
        Mathf.Clamp(_builtUpNitroAmount, 0, MaxNitroChargeAmounts);
        
        if(_nitroIncrementThresholdValue > 1f){
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
