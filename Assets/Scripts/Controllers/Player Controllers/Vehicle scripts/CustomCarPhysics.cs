using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomCarPhysics : MonoBehaviour
{
  #region Variables
  private float _throttleInput;
  private float _turningInput;
  private Transform _transform;
  [HideInInspector] public bool isUsingNitro = false;
  [HideInInspector] public bool isDrifting = false;

  [Header("Basic Setup")]
  [SerializeField] private float _raycastDistance = 1.5f;
  [SerializeField] private LayerMask _groundLayers;
  [SerializeField] private CustomWheels[] wheels;

  public CustomWheels[] WheelArray { get => wheels; }
  private int halfTireLength;
  private Rigidbody _rigidBody;
  public Rigidbody RigidBody { get => _rigidBody; }

  [Header("Angle Clamping")]

  [SerializeField] private float minXAngle = -10, maxXAngle = 10;
  [SerializeField] private float minZAngle = -15, maxZAngle = 15;

  [Header("Gears")]
  // Gear specifics

  [SerializeField] VehicleGearSpecs GearOne;
  [SerializeField] VehicleGearSpecs GearTwo;
  // Only for UI display
  [HideInInspector] public string gearText = "1st Gear";

  [Header("Acceleration setup")]

  [HideInInspector] public float horsePower;
  public float _terminalVelocity = 250f;
  public float TerminalVelocity { get => _terminalVelocity; }
  private Vector3 cachedLocalVelocity;
  private VehicleGearSpecs currentGear;
  [SerializeField] private float momentumModifier;

  [Header("Steering Setup")]

  [Tooltip("Grip Of wheels Between speed 0 - Terminal Velocity")]
  [SerializeField] private AnimationCurve tireGripCurve;

  [SerializeField] private float tireTurnModifier = 1;
  [SerializeField] private float minimumModifier = 0.25f;

  [Tooltip("The Distance between the Front and Back tires")]
  [SerializeField] public float wheelbase = 5f;

  [Tooltip("Minimum Space rquired to turn Vehicle 180 degree's in metres")]
  [SerializeField] public float turnRadius = 10f;

  [Tooltip("Distance between back Wheels")]
  [SerializeField] private float rearTrack = 2f;

  [Header("RigidBody mod Settings")]
  [Tooltip("The minimum angluar drag the car will experience For steering mechanics")]
  [SerializeField] private float minimumAngularDrag = 0.05f;
  [Tooltip("The maximum angular drag the car will experience For steering mechanics")]

  [SerializeField] private float maximumAngularDrag = 0.5f;

  [Header("Ground Stick Setup")]
  [Tooltip("The amount of speed to stop sticking to the ground")]
  [SerializeField] private float thresholdToJump = 145f;
  [Tooltip("Distance from above the ground the car is considered 'grounded'. (How far until groundStick is applies)")]
  [SerializeField] private float groundCheckDistance = 4f;
  private Vector3 setGroundPos;
  [SerializeField] private float lastGroundedXAngle;
  bool doStickGround;

  #endregion

  #region Public use Methods
  public void Init()
  {

    _rigidBody = GetComponentInChildren<Rigidbody>();

    _transform = transform;
    currentGear = GearOne;

    horsePower = GearOne.HorsePower;
    _terminalVelocity = GearTwo.MaxSpeed;

    halfTireLength = wheels.Length / 2;

    foreach (var tire in wheels)
    {
      float leftAckAngle = 0f;
      float rightAckAngle = 0f;

      switch (tire.tireType)
      {
        case TireType.frontTireLeft:
          leftAckAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius + (rearTrack / 2)));
          rightAckAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius - (rearTrack / 2)));
          break;
        case TireType.frontTireRight:
          leftAckAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius - (rearTrack / 2)));
          rightAckAngle = Mathf.Rad2Deg * Mathf.Atan(wheelbase / (turnRadius + (rearTrack / 2)));
          break;
      }

      tire.init(_rigidBody, leftAckAngle, rightAckAngle);
    }
    StartCoroutine(delayGroundCheck(3.5f));
  }

  public void setInputs(float throttleAmt, float turningAmt)
  {
    _throttleInput = throttleAmt;
    _turningInput = turningAmt;
  }
  public void ShiftGears(float delta)
  {
    if (delta != 0)
    {
      currentGear = delta > 0 ? GearTwo : GearOne;
      gearText = delta > 0 ? "2nd Gear" : "1st Gear";
      horsePower = currentGear.HorsePower;

      foreach (var wheel in wheels)
      {
        wheel.forwardAccTime = 0f;
        wheel.backwardAccTime = 0f;
      }
    }

  }
  public float getSpeed()
  {
    return _rigidBody.velocity.magnitude;
  }
  public Vector3 getVelocity()
  {
    return _rigidBody.velocity;
  }
  public void setRigidBodyVelocity(Vector3 vel)
  {
    _rigidBody.velocity = vel;
  }
  /// <summary>
  /// 
  /// Coroutine for more simple nitro. Just start coroutine and forget
  /// More simple then adding multiple values and counting in update
  /// 
  /// </summary>
  /// <param name="_nitroMultiplier">The amount that accelleration is multiplied by. EG 2f</param>
  /// <param name="nitroTiming">How long the nitro should last</param>
  /// <returns></returns>
  public IEnumerator useNitro(float _nitroMultiplier, float nitroTiming)
  {

    float count = 0f;
    horsePower = currentGear.HorsePower * _nitroMultiplier;
    isUsingNitro = true;
    //Set invunerable to offroad / physicsMaterials below when implemented
    while (count <= nitroTiming)
    {
      count += Time.deltaTime;
      yield return null;  // Might set to small timings between checks. maybe waitForSeconds(0.25f) or so
                          // this would make nitro timing 4x longer. don't do!
    }
    isUsingNitro = false;
    horsePower = currentGear.HorsePower;
  }

  public void driftVehicle(bool isUsingDrift)
  {
    if (isDrifting == false && isUsingDrift == true)
    {
      isDrifting = true;
    }
  }
  public void endedDrifting(bool endedDrifting)
  {
    if (isDrifting == true && endedDrifting == true)
    {
      isDrifting = false;
    }

  }
  #endregion

  public void Update()
  {

    tireTurnModifier = Mathf.Max(1 - (_rigidBody.velocity.magnitude / _terminalVelocity), minimumModifier);

    //Tire turning shit
    for (int i = 0; i < wheels.Length; i++)
    {
      wheels[i].setInputs(_throttleInput, _turningInput);

      if (i < halfTireLength)
      {
        wheels[i].TurnTire(tireTurnModifier);
      }

    }

  }


  #region Physics Simulations

  void FixedUpdate()
  {
    float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);
    float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / currentGear.MaxSpeed);

    float tireGrip = tireGripCurve.Evaluate(normalizedSpeed);
    //To Resist turning at higher speeds
    Vector3 momentumDir = transform.forward;

    float angleoffsetFromMomentum = Mathf.Abs(Vector3.SignedAngle(wheels[0].transform.forward, momentumDir, Vector3.up) + Vector3.SignedAngle(wheels[1].transform.forward, momentumDir, Vector3.up) / 2);
    float speedModifier = 1 - normalizedSpeed;

    momentumModifier = 1f;
    /*
if(angleoffsetFromMomentum != 0)
{
    momentumModifier = angleoffsetFromMomentum * speedModifier;
}
else
{
    momentumModifier = 1f;
}*/


    
    for (int i = 0; i < wheels.Length; i++)
    {
      wheels[i].raycastDown(_groundLayers, _raycastDistance);

      if (wheels[i].TireIsGrounded)
      {
        wheels[i].applyTireSuspensionForces();

        wheels[i].applyTireAcceleration(horsePower, currentGear.AxleEfficiency, tireGrip, momentumModifier, _throttleInput);

        if (isDrifting)
        {
          wheels[i].applyTireSlideOnDrift(0.1f, 1f);
        }
        else
        {
          wheels[i].applyTireSlide(tireGrip);
        }
      }
    }
    cachedLocalVelocity = _rigidBody.velocity;
    cachedLocalVelocity.z = Mathf.Clamp(cachedLocalVelocity.z, -currentGear.MaxSpeed, currentGear.MaxSpeed);

    _rigidBody.velocity = cachedLocalVelocity;

    checkIfGrounded();
    ClampLocalRotation();
  }
  private void ClampLocalRotation()
  {
    Vector3 localEulerAngles = transform.localRotation.eulerAngles;

    localEulerAngles.x = ClampAngle(localEulerAngles.x, minXAngle, maxXAngle);
    localEulerAngles.z = ClampAngle(localEulerAngles.z, minZAngle, maxZAngle);

    transform.localRotation = Quaternion.Euler(localEulerAngles);
  }

  public static float ClampAngle(float current, float min, float max)
  {
    float dtAngle = Mathf.Abs(((min - max) + 180) % 360 - 180);
    float hdtAngle = dtAngle * 0.5f;
    float midAngle = min + hdtAngle;

    float offset = Mathf.Abs(Mathf.DeltaAngle(current, midAngle)) - hdtAngle;
    if (offset > 0)
      current = Mathf.MoveTowardsAngle(current, midAngle, offset);
    return current;
  }

  private void checkIfGrounded()
  {

    bool isGrounded = Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, groundCheckDistance, _groundLayers);
    Debug.DrawRay(transform.position, -transform.up * _raycastDistance, isGrounded == true ? Color.green : Color.red);
    // Not enough speed to jump
    if (isGrounded == false && _rigidBody.velocity.magnitude < thresholdToJump)
    {
      if (doStickGround) stickToGround();
    }
    else
    {
      lastGroundedXAngle = transform.rotation.eulerAngles.x;
    }
  }
  private void stickToGround()
  {
    Physics.Raycast(transform.position, Vector3.down, out RaycastHit GroundHit, _groundLayers);
    setGroundPos = GroundHit.point;
    transform.position = setGroundPos;
    Vector3 rot = transform.rotation.eulerAngles;
    rot.x = lastGroundedXAngle;

    transform.rotation = Quaternion.Euler(rot);
    StartCoroutine(delayGroundCheck(1.5f));
  }
  public IEnumerator delayGroundCheck(float delayTime)
  {
    float timeCount = 0;
    while (timeCount < delayTime)
    {
      timeCount += Time.deltaTime;
      yield return null;
    }
    doStickGround = true;
  }

    private void OnCollisionEnter(Collision collision)
    {
        Vector3 newVelocity = Vector3.zero;
        ContactPoint cpoint  = collision.GetContact(0);

        Vector3 bumpDir = transform.position - cpoint.point;
        bumpDir.y = 1;
        bumpDir.Normalize();

        RigidBody.AddForce(1* RigidBody.mass * bumpDir );
        _rigidBody.velocity = newVelocity;
    }
    #endregion
}
