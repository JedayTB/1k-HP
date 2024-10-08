using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomCarPhysics : MonoBehaviour
{
    private float _throttleInput;
    private float _turningInput;

    private Transform _transform;

    [Header("Basic Setup")]
    [SerializeField] private Transform[] Tires;
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private bool debugRaycasts = true;
    [SerializeField] private float _tireRaycastDistance = 0.1f;

    private int halfTireLength;
    RaycastHit[] _tireGroundHits;
    private Rigidbody _rigidBody;


    public Rigidbody RigidBody { get => _rigidBody; }

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
    private float _lowerClamp;
    private float _higherClamp;
    //Public members
    [HideInInspector] private float frontTiresRotationAngle;
    [HideInInspector] private float backTiresRotationAngle;

    public float FrontTiresRotationAngle { get => frontTiresRotationAngle; }
    public float BackTiresRotationAngle { get => backTiresRotationAngle; }


    //Public Functions
    public void Init()
    {

        _rigidBody = GetComponentInChildren<Rigidbody>();

        _transform = transform;

        _tireGroundHits = new RaycastHit[Tires.Length];
        halfTireLength = (Tires.Length / 2);
        _baseAccelerationAmount = _accelerationAmount;
    }
    public void setInputs(float throttleAmt, float turningAmt)
    {
        _throttleInput = throttleAmt;
        _turningInput = turningAmt;
    }
    public float getVelocity()
    {
        return _rigidBody.velocity.magnitude;
    }
    public void setRigidBodyVelocity(Vector3 vel)
    {
        _rigidBody.velocity = vel;
    }

    public void useNitro(bool isUsingNitro, float _nitroMultiplier)
    {
        _accelerationAmount = isUsingNitro ? _baseAccelerationAmount * _nitroMultiplier : _baseAccelerationAmount;
    }

    public void driftVehicle(bool isUsingDrift)
    {
        /// how drifting works (right dir)
        ///   / /   front wheels
        ///   | |
        ///   \ \   back wheels
        // If they weren't drifting before. 
        // i.e Only run this logic at the beginning of the drift.
        if (isDrifting == false && isUsingDrift == true)
        {
            print("start drift!");
            isDrifting = true;

            float invertedTurningInput = -Mathf.CeilToInt(_turningInput);

            _lowerClamp = 25f * invertedTurningInput;
            _higherClamp = 45f * invertedTurningInput;


            /// Makes back tires pivot 35 degree's opposite of player input
            /// player input -> (right)
            /// | | current back tires
            /// after
            /// \ \ 

            for (int i = halfTireLength; i < Tires.Length; i++)
            {

                Vector3 backTireRot = Tires[i].localRotation.eulerAngles;

                backTireRot.y = 35 * -Mathf.CeilToInt(_turningInput);

                Tires[i].localRotation = Quaternion.Euler(backTireRot);

            }

        }
    }
    public void endedDrifting(bool endedDrifting)
    {
        isDrifting = endedDrifting;
        int TiresLenHalf = halfTireLength;

        _lowerClamp = -110;
        _higherClamp = 110;

        // Reset back tires
        for (int i = TiresLenHalf; i < Tires.Length; i++)
        {

            Vector3 backTireRot = Tires[i].localRotation.eulerAngles;

            backTireRot.y = 0f;

            Tires[i].localRotation = Quaternion.Euler(backTireRot);
        }
    }

    //End of public functions

    //Physics
    void FixedUpdate()
    {
        _tireGroundHits = rayCastFromTires();

        for (int i = 0; i < _tireGroundHits.Length; i++)
        {
            if (_tireGroundHits[i].collider != null)
            {

                applyTireSuspensionForces(Tires[i], _tireGroundHits[i]);

                float currentTireGrip = 1f;
                float currentTireMass = 5f;


                //   to use calculated tire grip withing applyTireSlide.
                if (isDrifting)
                {
                    currentTireGrip = applyTireRotation(Tires[i], i, _lowerClamp, _higherClamp);
                }
                else
                {
                    applyTireRotation(Tires[i], i, _lowerClamp, _higherClamp);
                }
                applyTireAcceleration(Tires[i], i);

                applyTireSlide(Tires[i], i, currentTireGrip, currentTireMass);

            }
        }
    }

    /// <summary>
    /// Rotates the tire for use in tire accelleration. Changes Z axis
    /// lerps tire rotation back to 0 if no input detected
    /// </summary>
    /// <param name="Tire"></param>
    /// <param name="tireCount"></param>
    /// <returns> Returns current tire grip calculated using _tireGripCurve. Still has side effect on tires if returns.</returns>
    float applyTireRotation(Transform Tire, int tireCount, float lowerClamp, float higherClamp)
    {
        float tireGrip = 5f;

        if (tireCount < halfTireLength && _frontWheelSteer)
        {
            Vector3 tireRotation = Vector3.zero;

            if (Mathf.Abs(_turningInput) < 0.1f)
            {
                //When Player let's go of X input, lerp to 0

                _durationOfAngleTiming += Time.fixedDeltaTime;
                _elapsedTime = _durationOfAngleTiming / _rotationAngleTimeToZero;

                frontTiresRotationAngle = Mathf.Lerp(frontTiresRotationAngle, 0, _elapsedTime);

                tireRotation.y = frontTiresRotationAngle;

                Tire.localRotation = Quaternion.Euler(tireRotation);
            }
            else
            {
                _durationOfAngleTiming = 0;

                float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);

                //float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _carTopSpeed);
                //Lazy fix
                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _tireGripHackFix);

                tireGrip = _tireGripCurve.Evaluate(normalizedSpeed);

                //Debug.Log($"Car spd {carSpeed} Norm Spd: {normalizedSpeed}, tireGrip { tireGrip}");

                frontTiresRotationAngle += (_turningInput * tireGrip) * _tireTurnSpeed;

                frontTiresRotationAngle = Mathf.Clamp(frontTiresRotationAngle, lowerClamp, higherClamp);

                tireRotation.y = frontTiresRotationAngle;

                Tire.localRotation = Quaternion.Euler(tireRotation);


            }

        }
        else if (tireCount >= halfTireLength)
        {
            float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);

            //float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _carTopSpeed);
            //Lazy fix
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _tireGripHackFix);


            tireGrip = _tireGripCurve.Evaluate(normalizedSpeed);
        }
        return tireGrip;
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
        Vector3 tireWorldVelocity = _rigidBody.GetPointVelocity(Tire.position);

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

        _rigidBody.AddForceAtPosition(steerForce, Tire.position);
    }
    /// <summary>
    /// Apply forward force in the Z direction of tires
    /// </summary>
    /// <param name="Tire"></param>
    /// <param name="tireCount"></param>
    void applyTireAcceleration(Transform Tire, int tireCount)
    {
        Vector3 accelerationDirection = _accelerationAmount * Tire.forward;

        if (Mathf.Abs(_throttleInput) > 0.0f)
        {
            // Forward speed of the car 
            float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);

            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _vehicleTopSpeed);

            float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * _throttleInput;

            _rigidBody.AddForceAtPosition(availableTorque * accelerationDirection, Tire.position);

            Debug.DrawRay(Tire.position, availableTorque * accelerationDirection * 0.1f,
             tireCount > halfTireLength ? Color.blue : Color.green);
        }

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
        Vector3 tireWorldVelocity = _rigidBody.GetPointVelocity(Tire.position);

        float offset = _springRestDistance - rayHit.distance;

        //Calculate velocity along the spring direction
        //springDir is a unity vector, this returns the magnitude of trieWorldVel
        //as projected onto springDir
        float vel = Vector3.Dot(springDir, tireWorldVelocity);

        float force = (offset * _springStrength) - (vel * _springDamping);

        _rigidBody.AddForceAtPosition(springDir * force, Tire.position);
    }

    RaycastHit[] rayCastFromTires()
    {
        RaycastHit[] raycastHits = new RaycastHit[Tires.Length];

        for (int i = 0; i < Tires.Length; i++)
        {

            Physics.Raycast(Tires[i].position, -Tires[i].up, out RaycastHit hit, _tireRaycastDistance, _groundLayers);
            raycastHits[i] = hit;
            if (debugRaycasts)
            {
                bool rayHit = hit.point != Vector3.zero;

                Color rayColour = rayHit ? Color.green : Color.red;

                Debug.DrawRay(Tires[i].position, -Tires[i].up * _tireRaycastDistance, rayColour);
            }

        }

        return raycastHits;
    }
}