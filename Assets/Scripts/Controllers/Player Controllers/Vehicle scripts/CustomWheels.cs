using JetBrains.Annotations;
using UnityEngine;
public enum TireType
{
    frontTireLeft,
    frontTireRight,
    backTire
}

public class CustomWheels : MonoBehaviour
{
    //Delete after finished
    public string debugstr;

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
    [SerializeField]private float _leftAckermanAngle, _rightAckermanAngle;

    public float LeftAckermanAngle { get => _leftAckermanAngle; }
    public float RightAckermanAngle{ get => _rightAckermanAngle; }

    #region Public Physic's unrelated
    public void init(Rigidbody rb, float leftTurnAngle, float rightTurnAngle)
    {
        _vehicleRB = rb;
        _tireTransform = transform;

        _leftAckermanAngle = leftTurnAngle;
        _rightAckermanAngle = rightTurnAngle;

        Debug.LogWarning("Funny business with neg vals in tire turning. ");
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
    public void TurnTire(float turningInput)
    { 
        Vector3 rotation = transform.localRotation.eulerAngles;

        float desiredAngle = 0;
        float currentRotY = rotation.y;


        desiredAngle = _leftAckermanAngle * turningInput;   // Ackerman angles are always positive. 
                                                                // Therefore, when turning left,
                                                                // Desired angle should always be negative
                                                                // (turningInput is negative when turning left)

        if (turningInput > 0) // Turning right
        {
            currentRotY = LerpAndEasings.ExponentialDecay(rotation.y, desiredAngle, _decaySpeed, Time.deltaTime);

            rotation.y = currentRotY;
        }
        else if(turningInput < 0) // Turning Left
        {
            /*
            currentRotY = LerpAndEasings.ExponentialDecay(currentRotY, desiredAngle, _decaySpeed, Time.deltaTime);
            currentRotY = Mathf.Clamp(currentRotY, -LeftAckermanAngle, 0);
            
            rotation.y = currentRotY;
            */
            rotation.y = _leftAckermanAngle * turningInput;
        }

        
        // Workaround for funky Quaternion derived from euler with negative angles
        Quaternion newRot = Quaternion.Euler(rotation);
        _tireTransform.localRotation = Quaternion.Slerp(transform.localRotation, newRot, 1);
        steeringAngle = rotation.y;

        debugstr = $"CurrentRotY {currentRotY} \nFinished Rot {_tireTransform.localRotation.eulerAngles}";
    }

    #endregion

    #region Physics Simulations
    public void raycastDown(LayerMask groundLayers, float raycastDistance)
    {
        tireIsGrounded = Physics.Raycast(_tireTransform.position, -_tireTransform.up, out rayCastHit, raycastDistance, groundLayers);
        if(tireIsGrounded){
            forceApplicationPoint = applyForcesAtWheelPoint ? rayCastHit.point: transform.position;
        }
        if (isDebugging)
        {
            bool rayHit = transform.position != Vector3.zero;

            Color rayColour = rayHit ? Color.green : Color.red;

            Debug.DrawRay(_tireTransform.position, -_tireTransform.up * raycastDistance, rayColour);
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
    /// <param name="throttleInput">"Forward" Input by the controller class</param>
    /// <param name="accelerationAmount">Vehicles acceleratoin force</param>
    /// <param name="availableTorque">Available torque the engine has. Calculated in  VehiclePhysics</param>

    public void applyTireAcceleration(float throttleInput, float accelerationAmount, float availableTorque)
    {
        Vector3 accelerationDirection = accelerationAmount * _tireTransform.forward;
        _vehicleRB.AddForceAtPosition(throttleInput * availableTorque * accelerationDirection, forceApplicationPoint);

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

        float offset = _wheelSpecs.springRestDistance - rayCastHit.distance;

        //Calculate velocity along the spring direction
        //springDir is a unity vector, this returns the magnitude of trieWorldVel
        //as projected onto springDir
        float vel = Vector3.Dot(springDir, tireWorldVelocity);

        float force = (offset * _wheelSpecs.springStrength) - (vel * _wheelSpecs.springDamping);

        _vehicleRB.AddForceAtPosition(springDir * force, forceApplicationPoint);
    }
    #endregion
}

