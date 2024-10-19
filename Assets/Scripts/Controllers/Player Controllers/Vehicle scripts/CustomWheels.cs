using UnityEngine;


public class CustomWheels : MonoBehaviour
{
    #region Variables
    
    [Header("Basic Setup")]
    private bool wheelIsHittingGround;
    private Rigidbody _vehicleRB;

    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private bool debugRaycasts = true;
    [SerializeField] private bool isFrontWheel;
    [SerializeField] private float wheelSpeed;
    [SerializeField] private float _tireRaycastDistance = 1f;

    //Accelerations

    [Header("Acceleration Setup")]
    [Tooltip("Top speed of the car")]
    [SerializeField] private float _vehicleTopSpeed = 500f;
    [Tooltip("How fast the car accelerates")]
    [SerializeField] private float _accelerationAmount = 3500f;
    private float _baseAccelerationAmount;
    [Tooltip("How much force is available at certain speeds.")]
    [SerializeField] private AnimationCurve torqueCurve;
    [SerializeField] private bool _frontWheelDrive = true;

    //suspension

    [Header("Suspension Setup")]

    [Tooltip("The force at which the spring tries to return to rest distanc with")]
    [SerializeField] private float _springStrength = 1000f;

    [Tooltip("Dampens speed at which spring returns to rest. Lower is bouncy, higher is stiff")]
    [SerializeField] private float _springDamping = 70f;

    [Tooltip("Distance in unity units the springs rest below the tire")]
    [SerializeField] private float _springRestDistance = 0.8f;

    //Breaking

    [Header("Breaking Setup")]
    [SerializeField] private bool _frontWheelBreaking = false;
    [SerializeField] private bool _backWheelBreaking = true;
    [SerializeField] private float _tireMass = 5f;
    private bool isDrifting = false;

    //Steering

    [Header("Steering Setup")]
    //Steering
    [SerializeField] private bool _frontWheelSteer = true;

    [Tooltip("Determines If the Back wheels steer the car. If front and back true, all wheels turn. NOTE back wheels are the last two elements of the Tires array.")]
    [SerializeField] private bool _backWheelSteer = false;

    [SerializeField] private float _rotationAngleTimeToZero = 1.5f;
    [SerializeField] private AnimationCurve _tireGripCurve;
    [SerializeField] private float _tireGripHackFix = 100f;
    private float _tireTurnSpeed = 1f;
    private float _durationOfAngleTiming;
    private float _elapsedTime;
    private float _lowerClamp = -110;
    private float _higherClamp = 110;
    //Public members
    [HideInInspector] private float frontTiresRotationAngle;
    [HideInInspector] private float backTiresRotationAngle;

    public float FrontTiresRotationAngle { get => frontTiresRotationAngle; }
    public float BackTiresRotationAngle { get => backTiresRotationAngle; }

    
    #endregion

    public void init(Rigidbody rb){
        _vehicleRB = rb;
    }
    public void setTireRotation(float yAngle){

    }
    /// <summary>
    /// tire slide attempts to make the car stay in the Z direction of it's tires.
    /// </summary>
    /// <param name="Tire"> The transform of the tire </param>
    /// <param name="tireCount"> Which tire it is. </param>
    /// <param name="tireGrip"> The amount of grip the tire has</param>
    /// <param name="tireMass"> mass of the tire. Change for drifting </param>
    void applyTireSlide(Transform Tire, int tireCount, float tireGrip, float tireMass)
    {
        // world space direction of the steering force
        Vector3 steeringDir = Tire.right;

        //world space velocity of the tire
        Vector3 tireWorldVelocity = _vehicleRB.GetPointVelocity(Tire.position);

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
        Vector3 steerForce = tireMass * desiredAcceleration * steeringDir;

        _vehicleRB.AddForceAtPosition(steerForce, Tire.position);
    }
    /// <summary>
    /// Apply forward force in the Z direction of tires
    /// </summary>
    /// <param name="Tire"></param>
    /// <param name="tireCount"></param>
    void applyTireAcceleration(Transform Tire, int tireCount)
    {   
        //only makes it so back tires accelerate while drifting
        /*

        Vector3 accelerationDirection = _accelerationAmount * Tire.forward;

        if (Mathf.Abs(_throttleInput) > 0.0f)
        {
            // Forward speed of the car 
            float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);

            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _vehicleTopSpeed);

            float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * _throttleInput;

            _rigidBody.AddForceAtPosition(availableTorque * accelerationDirection, Tire.position);

            Debug.DrawRay(Tire.position, 0.1f * availableTorque * accelerationDirection,
             tireCount > halfTireLength ? Color.blue : Color.green);
        }
        */
    }
    /// <summary>
    /// Add spring force for the Car
    /// </summary>
    /// <param name="Tire"></param>
    /// <param name="rayHit"></param>
    void applyTireSuspensionForces(Transform Tire, RaycastHit rayHit)
    {

        //World space direction of the spring force
        Vector3 springDir = Tire.up;

        //World space velocity of this tire
        Vector3 tireWorldVelocity = _vehicleRB.GetPointVelocity(Tire.position);

        float offset = _springRestDistance - rayHit.distance;

        //Calculate velocity along the spring direction
        //springDir is a unity vector, this returns the magnitude of trieWorldVel
        //as projected onto springDir
        float vel = Vector3.Dot(springDir, tireWorldVelocity);

        float force = (offset * _springStrength) - (vel * _springDamping);

        _vehicleRB.AddForceAtPosition(springDir * force, Tire.position);
    }

}
