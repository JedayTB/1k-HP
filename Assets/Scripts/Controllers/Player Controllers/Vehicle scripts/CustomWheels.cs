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
    public bool isDebugging;
    public float steeringAngle;
    [Tooltip("Set in inspector with Wheel Specs scriptable object")]
    [SerializeField] private WheelSpecs _wheelSpecs;

    private Rigidbody _vehicleRB;
    private Transform _tireTransform;
    private RaycastHit rayCastHit;

    private bool tireIsGrounded = false;
    public bool TireIsGrounded {get => tireIsGrounded;}
    public void init(Rigidbody rb)
    {
        _vehicleRB = rb;
        _tireTransform = transform;
    }

    public void setTireRotation(float yAngle)
    {
        Vector3 rotation = transform.localRotation.eulerAngles;
        rotation.y = yAngle;
        steeringAngle = yAngle;
        _tireTransform.localRotation = Quaternion.Euler(rotation);
    }


    public void raycastDown(LayerMask groundLayers, float raycastDistance)
    {
        tireIsGrounded = Physics.Raycast(_tireTransform.position, -_tireTransform.up, out rayCastHit, raycastDistance, groundLayers);
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

        _vehicleRB.AddForceAtPosition(steerForce, transform.position);
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

        _vehicleRB.AddForceAtPosition(steerForce, transform.position);
    }
    /// <summary>
    /// Apply force in  the local Z axis of the tire.
    /// 
    /// </summary>
    /// <param name="throttleInput">"Forward" Input by the controller class</param>
    /// <param name="accelerationAmount">Vehicles acceleratoin force</param>
    /// <param name="availableTorque">Available torque the engine has. Calculated in  VehiclePhysics</param>

    public void applyTireAcceleration(float throttleInput, float accelerationAmount, float availableTorque)
    {
        //only makes it so back tires accelerate while drifting
        Vector3 accelerationDirection = accelerationAmount * _tireTransform.forward;
        //Shouldn't do anything if throttle is 0?
        _vehicleRB.AddForceAtPosition(availableTorque * accelerationDirection, transform.position);

    }
    /// <summary>
    /// Add spring force for the Car
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

        _vehicleRB.AddForceAtPosition(springDir * force, transform.position);
    }
}

