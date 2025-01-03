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
  private float suspensionOffset;
  public float SuspensionOffset { get => suspensionOffset; }
  public float LeftAckermanAngle { get => _leftAckermanAngle; }
  public float RightAckermanAngle { get => _rightAckermanAngle; }

  //Drag Variables
  private float dragForce;
  private float airDensity = 1.225f;
  private float frontalArea = 2.16f;
  private float dragCoefficient = 0.3f;
  private float rollingResistanceForce;
  private float rrCoefficient = 0.15f;



  #region Public Physic's unrelated
  public void init(Rigidbody rb, float leftTurnAngle, float rightTurnAngle)
  {
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

  /*
  public void moveTires(float steeringAngle)
  {
    double turningRadius = wheelbase / Math.Tan(steeringAngle);s
    velocity = _vehicleRB.velocity.magnitude;
    double lateralAcceleration = Math.Pow(velocity, 2f) / turningRadius;
    double lateralForce = mass * lateralAcceleration;
    double gripForce = gripCoefficient * mass * Physics.gravity.y;
    if (lateralForce > gripForce)
    {
      skid();
    }

  }
  */

  public void skid()
  {

  }
  void Update()
  {
    // Timing bs


    bool calcForward = tireIsGrounded && _throttleInput > 0f;
    bool calcBackward = tireIsGrounded && _throttleInput < 0f;

    forwardAccTime = calcForward ? forwardAccTime + Time.deltaTime : 0f;
    backwardAccTime = calcBackward ? backwardAccTime + Time.deltaTime : 0f;
    // If the forward vector angle of the tire is past a certain threshold of the 
    // Forward vector of the car, skid (lose traction)
    // find a way to put speed into the calculation
    Vector3 carVelocity = _vehicleRB.velocity;
    carVelocity = transform.InverseTransformDirection(carVelocity);
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
    if (isDebugging)
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
    // Quick ternary for tire driting

    Vector3 steerForce = _wheelSpecs.tireMass * desiredAcceleration * steeringDir;

    _vehicleRB.AddForceAtPosition(steerForce, forceApplicationPoint);


  }
  /// <summary>
  /// tire slide attempts to make the car stay in the Z direction of it's tires.
  /// </summary>
  /// <param name="tireGrip"> The amount of grip the tire has</param>
  public void applyTireSlideOnDrift(float tireGrip, float tireMass)
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


    Vector3 steerForce = tireMass * desiredAcceleration * steeringDir;

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
    
    Vector3 accelerationDirection = 0.5f * (engineForce / 4) * accelerationTime * tireGrip * dir;

    accelerationDirection *= throttle;

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
  #endregion
}

