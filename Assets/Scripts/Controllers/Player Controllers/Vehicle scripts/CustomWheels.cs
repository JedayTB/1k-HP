using UnityEngine;
public enum TireType
{
    frontTireLeft,
    frontTireRight,
    backTire
}

public class CustomWheels : MonoBehaviour
{
   
    public TireType tireType;
    private Vector3 forceApplicationPoint;
    [SerializeField] private bool applyForcesAtWheelPoint = false;
    [Tooltip("Set in inspector with Wheel Specs scriptable object")]
    [SerializeField] private WheelSpecs _wheelSpecs;
    [SerializeField] private float _decaySpeed = 7.5f;
    [SerializeField] private bool isDebugging;
    private float steeringAngle;
    public float SteeringAngle {get => steeringAngle;}
    private bool tireIsGrounded = false;
    public bool TireIsGrounded { get => tireIsGrounded; }
    private Rigidbody _vehicleRB;
    private Transform _tireTransform;
    private RaycastHit rayCastHit;
    [SerializeField] private float _leftAckermanAngle, _rightAckermanAngle;
    private float suspensionOffset;
    public float SuspensionOffset {get => suspensionOffset;}

    public float LeftAckermanAngle { get => _leftAckermanAngle; }
    public float RightAckermanAngle{ get => _rightAckermanAngle; }

    #region Public Physic's unrelated
    public void init(Rigidbody rb, float leftTurnAngle, float rightTurnAngle)
    {
        _vehicleRB = rb;
        _tireTransform = transform;

        _leftAckermanAngle = leftTurnAngle;
        _rightAckermanAngle = rightTurnAngle;
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
    /// Lerp smoothed setting of tire Y angle. Uses ExponentialDecay smoothing, which is frame independent.
    /// </summary>
    /// <param name="turningInput">value from 0-1 inside Custom Car physics</param>
    public void TurnTire(float turningInput, float angleModifier)
    { 
        float desiredAngle = turningInput > 0 ? _rightAckermanAngle: _leftAckermanAngle;
        desiredAngle *= turningInput * angleModifier;

        steeringAngle = LerpAndEasings.ExponentialDecay(steeringAngle, desiredAngle, _decaySpeed, Time.deltaTime);
        
        Vector3 rotation = transform.localRotation.eulerAngles;

        rotation.y = steeringAngle;
        transform.localRotation = Quaternion.Euler(rotation);
    }

    #endregion

    #region Physics Simulations
    public void raycastDown(LayerMask groundLayers, float raycastDistance)
    {
        tireIsGrounded = Physics.Raycast(transform.position, -transform.up, out rayCastHit, raycastDistance, groundLayers);
        if(tireIsGrounded){
            forceApplicationPoint = applyForcesAtWheelPoint ? rayCastHit.point: transform.position;
        }
        if (isDebugging)
        {
            bool rayHit = transform.position != Vector3.zero;

            Color rayColour = rayHit ? Color.green : Color.red;

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
    /// <param name="availableTorque">Available torque the engine has. Calculated in  VehiclePhysics</param>

    public void applyTireAcceleration(float accelerationAmount, float availableTorque)
    {
        Vector3 accelerationDirection = accelerationAmount * _tireTransform.forward;
        _vehicleRB.AddForceAtPosition(availableTorque * accelerationDirection, forceApplicationPoint);
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

