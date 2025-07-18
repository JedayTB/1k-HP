using System;
using UnityEngine;

public enum TireType
{
  frontTireLeft,
  frontTireRight,
  backTire
}

public class CustomWheels : MonoBehaviour
{
  private CustomCarPhysics ParentPhys;
  private float _turningInput = 0;
  private float _throttleInput = 0;
  public TireType tireType;
  private Vector3 forceApplicationPoint;
  [SerializeField] private bool applyForcesAtWheelPoint = false;
  [Tooltip("Set in inspector with Wheel Specs scriptable object")]
  [SerializeField] private WheelSpecs _wheelSpecs;
  [SerializeField] private float _decaySpeed = 7.5f;
  [SerializeField] private bool isDebugging;

  private float steeringAngle;
  public float SteeringAngle { get => steeringAngle; }
  private bool tireIsGrounded = false;
  public bool TireIsGrounded { get => tireIsGrounded; }
  private Rigidbody _vehicleRB;
  private Transform _tireTransform;
  private RaycastHit rayCastHit;

  public float forwardAccTime = 0f;
  public float backwardAccTime = 0f;

  [SerializeField] private float _leftAckermanAngle, _rightAckermanAngle;

  [Header("Debug Visible")]
  [SerializeField] private Vector3 acceleration;

  // For use in visual controller
  private float suspensionOffset;
  public float SuspensionOffset { get => suspensionOffset; }
  public float LeftAckermanAngle { get => _leftAckermanAngle; }
  public float RightAckermanAngle { get => _rightAckermanAngle; }

  #region Public Physic's unrelated
  public void init(CustomCarPhysics veh, Rigidbody rb, float leftTurnAngle, float rightTurnAngle)
  {
    ParentPhys = veh;
    _vehicleRB = rb;
    _tireTransform = transform;

    _leftAckermanAngle = leftTurnAngle;
    _rightAckermanAngle = rightTurnAngle;
    //applyForcesAtWheelPoint = true;
  }
  /// <summary>
  /// Manual Setting of tire Y angle
  /// </summary>
  /// <param name="turningInput"> Value from 0-1 inside Custom Car physics</param>
  public void setTireRotation(float turningInput)
  {
    Vector3 rotation = transform.localRotation.eulerAngles;

    if (turningInput > 0) // Turning right
    {
      rotation.y = _rightAckermanAngle * Mathf.Abs(turningInput);
    }
    else if (turningInput < 0) // Turning Left
    {
      rotation.y = _leftAckermanAngle * Mathf.Abs(turningInput);
    }
    else
    {
      rotation.y = 0;
    }
    steeringAngle = rotation.y;
    _tireTransform.localRotation = Quaternion.Euler(rotation);
  }
  /// <summary>
  /// Smoothed turning of the tire! uses exponential decay
  /// </summary>
  /// <param name="modifier">Modifier that affects decayspeed, as well as the maximum desired angle</param>
  public void TurnTire(float modifier)
  {
    float desiredAngle = _turningInput > 0 ? _rightAckermanAngle : _leftAckermanAngle;
    desiredAngle *= _turningInput;
    desiredAngle *= modifier;

    float decaySpd = _decaySpeed * modifier;

    steeringAngle = LerpAndEasings.ExponentialDecay(steeringAngle, desiredAngle, decaySpd, Time.deltaTime);
    //moveTires(steeringAngle);

    Vector3 rotation = transform.localRotation.eulerAngles;

    rotation.y = steeringAngle;
    transform.localRotation = Quaternion.Euler(rotation);
  }

  public void setInputs(float throttle, float turning)
  {
    _throttleInput = throttle;
    _turningInput = turning;
  }
  public void setTimings(float forwardAccTime, float backwardAccTime)
  {
    this.forwardAccTime = forwardAccTime;
    this.backwardAccTime = backwardAccTime;
  }

  public void multiplyTimings(float fwMult, float bckMult)
  {
    this.forwardAccTime *= fwMult;
    this.backwardAccTime *= bckMult;
  }

  void Update()
  {
    // Timing bs

    Vector3 carVelocity = _vehicleRB.velocity;
    carVelocity = transform.InverseTransformDirection(carVelocity);

    float zVelSign = Mathf.Sign(carVelocity.z);

    float inputSign = Mathf.Sign(_throttleInput);

    bool SameAccelAsVelocity = zVelSign == inputSign;

    bool calcForward = tireIsGrounded && _throttleInput > 0f;
    bool calcBackward = tireIsGrounded && _throttleInput < 0f;

    if (SameAccelAsVelocity == false)
    {
      forwardAccTime = calcForward ? 1f : forwardAccTime - Time.deltaTime;
      backwardAccTime = calcBackward ? 1f : backwardAccTime - Time.deltaTime;
    }
    // Must find a way to set back to 0 once out
    // Start accelerating the same direction as input
    else
    {
      float modifier = ParentPhys.curGear == 0 ? 0.75f : 0.25f;

      forwardAccTime = calcForward ? forwardAccTime + (modifier * Time.deltaTime) : forwardAccTime - Time.deltaTime;
      forwardAccTime = Mathf.Max(forwardAccTime, 0f);

      backwardAccTime = calcBackward ? backwardAccTime + (modifier * Time.deltaTime) : backwardAccTime - Time.deltaTime;
      backwardAccTime = Mathf.Max(backwardAccTime, 0f);
    }

  }
  #endregion

  #region Physics Simulations
  public void raycastDown(LayerMask groundLayers, float raycastDistance)
  {
    tireIsGrounded = Physics.Raycast(transform.position, -transform.up, out rayCastHit, raycastDistance, groundLayers);
    if (tireIsGrounded)
    {
      forceApplicationPoint = applyForcesAtWheelPoint ? rayCastHit.point : transform.position;
    }
    //GameStateManager.Instance.UseDebug
    if (true)
    {
      Color rayColour = tireIsGrounded ? Color.green : Color.red;
      Debug.DrawRay(transform.position, -transform.up * raycastDistance, rayColour);
    }


  }



  /// <summary>
  /// tire slide attempts to make the car stay in the Z direction of it's tires.
  /// </summary>
  /// <param name="tireGrip"> The amount of grip the tire has</param>
  public void applyTireSlide(float tireGrip)
  {
    // world space direction of the steering force
    Vector3 steeringDir = _tireTransform.right;

    //world space velocity of the tire
    Vector3 tireWorldVelocity = _vehicleRB.GetPointVelocity(transform.position);

    // What's the velocity in the steering direction?
    // steerinDir is a unit vector, this returns the magnitude of tireWorldVel
    // As projected onto steeringDir

    // basically. How fast the tire is moving in the X axis.
    float steeringVelocity = Vector3.Dot(steeringDir, tireWorldVelocity);

    // The change in velocity that we're looking for is -steeringVel * gripFactor
    // gripfactor is withing the range of 0 - 1. 0 no grip, 1 full grip

    float desiredVelocityChange = -steeringVelocity * tireGrip;

    // Turn change in velocity into an acceleration (Acceleration =  deltaVel / time)
    // this will produce the accelerationn necessary to change the velocity 
    // by desired in 1 physics tick

    float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

    // Force = Mass * acceleration. 

    Vector3 steerForce = _wheelSpecs.tireMass * desiredAcceleration * steeringDir;

    _vehicleRB.AddForceAtPosition(steerForce, forceApplicationPoint);


  }
  /// <summary>
  /// tire slide attempts to make the car stay in the Z direction of it's tires.
  /// </summary>
  /// <param name="tireGrip"> The amount of grip the tire has</param>
  /// <param name="tireMassMultiplier"> Value between 0 - 1 that changes mass to use o ftire </param>
  public void applyTireSlideOnDrift(float tireGrip, float tireMassMultiplier)
  {
    // world space direction of the steering force
    Vector3 steeringDir = _tireTransform.right;

    //world space velocity of the tire
    Vector3 tireWorldVelocity = _vehicleRB.GetPointVelocity(transform.position);

    // What's the velocity in the steering direction?
    // steerinDir is a unit vector, this returns the magnitude of tireWorldVel
    // As projected onto steeringDir

    // basically. How fast the tire is moving in the X axis.
    float steeringVelocity = Vector3.Dot(steeringDir, tireWorldVelocity);

    // The change in velocity that we're looking for is -steeringVel * gripFactor
    // gripfactor is withing the range of 0 - 1. 0 no grip, 1 full grip

    float desiredVelocityChange = -steeringVelocity * tireGrip;

    // Turn change in velocity into an acceleration (Acceleration =  deltaVel / time)
    // this will produce the accelerationn necessary to change the velocity 
    // by desired in 1 physics tick

    float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

    // Force = Mass * acceleration. 
    // Quick ternary for tire driting


    Vector3 steerForce = (_wheelSpecs.tireMass * tireMassMultiplier) * desiredAcceleration * steeringDir;
    _vehicleRB.AddForceAtPosition(steerForce, forceApplicationPoint);

  }
  /// <summary>
  /// Apply force in  the local Z axis of the tire. 
  /// </summary>
  /// <param name="accelerationAmount">Vehicles acceleratoin force</param>
  /// <param name="throttle">Available torque the engine has. Calculated in  VehiclePhysics</param>

  public void applyTireAcceleration(float horsePower, float efficiency, float tireGrip, float throttle)
  {
    float accelerationTime = _throttleInput > 0f ? forwardAccTime : backwardAccTime;

    //V = V0*t + 0.5*a*t^2
    //V += 1/2at^2

    //70m/s = 250km/h / 3.6
    //F = m*v^2

    float velocity = Math.Abs(_vehicleRB.velocity.magnitude);

    if (velocity < 1)
    {
      velocity = 1;
    }

    float engineForce = horsePower * 745.7f / (velocity * efficiency);

    Vector3 dir = _tireTransform.forward;
    dir.y = 0;
    dir.Normalize();
    /* Divid engine force by amt of wheels */
    Vector3 accelerationDirection = 0.5f * (engineForce / 4) * accelerationTime * tireGrip * dir;

    if (throttle != 0)
    {
      accelerationDirection *= throttle;
    }
    else
    {
      accelerationDirection = Vector3.zero;
    }


    acceleration = transform.InverseTransformDirection(accelerationDirection.normalized);


    _vehicleRB.AddForceAtPosition(accelerationDirection, forceApplicationPoint);
  }
  /// <summary>
  /// Add spring force for the Vehicle
  /// </summary>
  public void applyTireSuspensionForces()
  {

    //World space direction of the spring force
    Vector3 springDir = _tireTransform.up;

    //World space velocity of this tire
    Vector3 tireWorldVelocity = _vehicleRB.GetPointVelocity(transform.position);

    suspensionOffset = _wheelSpecs.springRestDistance - rayCastHit.distance;

    //Calculate velocity along the spring direction
    //springDir is a unit vector, this returns the magnitude of tireWorldVel
    //as projected onto springDir

    float vel = Vector3.Dot(springDir, tireWorldVelocity);

    float force = (suspensionOffset * _wheelSpecs.springStrength) - (vel * _wheelSpecs.springDamping);

    _vehicleRB.AddForceAtPosition(springDir * force, forceApplicationPoint);

  }
  public void applyTireGroundStickForce()
  {

    //World space direction of the spring force
    Vector3 springDir = -_tireTransform.up;

    //World space velocity of this tire
    Vector3 tireWorldVelocity = _vehicleRB.GetPointVelocity(transform.position);

    suspensionOffset = _wheelSpecs.springRestDistance - rayCastHit.distance;

    //Calculate velocity along the spring direction
    //springDir is a unit vector, this returns the magnitude of tireWorldVel
    //as projected onto springDir

    float vel = Vector3.Dot(springDir, tireWorldVelocity);

    float force = (suspensionOffset * (_wheelSpecs.springStrength / 2)) - (vel * _wheelSpecs.springDamping);

    _vehicleRB.AddForceAtPosition(springDir * force, forceApplicationPoint);

  }
  #endregion
}

