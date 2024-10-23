using System.Collections;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomCarPhysics : MonoBehaviour
{
    #region Variables
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
    //[SerializeField] private bool _frontWheelBreaking = false;
    //[SerializeField] private bool _backWheelBreaking = true;
    [SerializeField] private float _tireMass = 5f;
    public bool isDrifting = false;

    //Steering

    [Header("Steering Setup")]
    //Steering
    [SerializeField] private bool _frontWheelSteer = true;

    [Tooltip("Determines If the Back wheels steer the car. If front and back true, all wheels turn. NOTE back wheels are the last two elements of the Tires array.")]

    [SerializeField] private float _rotationAngleTimeToZero = 1.5f;
    [SerializeField] private AnimationCurve _tireGripCurve;
    [SerializeField] private float _tireGripHackFix = 100f;
    private float _tireTurnSpeed = 1f;
    private float _durationOfAngleTiming;
    private float _elapsedTime;
    private float _lowerClamp = -45;
    private float _higherClamp = 45;
    //Public members
    private float frontTiresRotationAngle;
    private float backTiresRotationAngle = 0f;

    public float FrontTiresRotationAngle { get => frontTiresRotationAngle; }
    public float BackTiresRotationAngle { get => backTiresRotationAngle; }

    #endregion
    //Public Functions
    public void Init()
    {

        _rigidBody = GetComponentInChildren<Rigidbody>();

        _transform = transform;

        _tireGroundHits = new RaycastHit[Tires.Length];
        halfTireLength = Tires.Length / 2;
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
        //_accelerationAmount = isUsingNitro ? _baseAccelerationAmount * _nitroMultiplier : _baseAccelerationAmount;
        float count = 0f;
        _accelerationAmount = _baseAccelerationAmount * _nitroMultiplier;
        //Set invunerable to offroad / physicsMaterials below when implemented
        while (count <= nitroTiming)
        {
            count += Time.deltaTime;
            yield return null;  // Might set to small timings between checks. maybe waitForSeconds(0.25f) or so
                                // this would make nitro timing 4x longer. don't do!
        }
        _accelerationAmount = _baseAccelerationAmount;
    }

    public void driftVehicle(bool isUsingDrift)
    {
        if (isDrifting == false && isUsingDrift == true)
        {
            Debug.Log("start drift!");
            isDrifting = true;
        }
    }
    public void endedDrifting(bool endedDrifting)
    {
        if (isDrifting == true && endedDrifting == true)
        {
            isDrifting = false;
            Debug.Log("end drift");
        }
       
    }

    //End of public functions

    //Physics Based functions.


    void FixedUpdate()
    {
        _tireGroundHits = rayCastFromTires();

        for (int i = 0; i < _tireGroundHits.Length; i++)
        {
            if (_tireGroundHits[i].collider != null)
            {

                applyTireSuspensionForces(Tires[i], _tireGroundHits[i]);

                
                    
                float currentTireGrip = applyTireRotation(Tires[i], i, _lowerClamp, _higherClamp);
                
                applyTireAcceleration(Tires[i], i);

                // Don't apply slide to back tires during drift
                // Really gotta change this function name...
                if(isDrifting){
                    //print("during drift?");
                    applyTireSlide(Tires[i],  0.1f, 1f);
                }else{
                    applyTireSlide(Tires[i],  1f, _tireMass);
                }
            }
        }
    }

    
    /// <summary>
    /// tire slide attempts to make the car stay in the Z direction of it's tires.
    /// </summary>
    /// <param name="Tire"> The transform of the tire </param>
    /// <param name="tireCount"> Which tire it is. </param>
    /// <param name="tireGrip"> The amount of grip the tire has</param>
    /// <param name="tireMass"> mass of the tire. Change for drifting </param>
    void applyTireSlide(Transform Tire, float tireGrip, float tireMass)
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

        //Debug.Log(steeringVelocity);

        // The change in velocity that we're looking for is -steeringVel * gripFactor
        // a negative steering vel is turning left (in terms of world 0, 0)
        // a positive steering vel is turning right. 
        //ie. If car is sliding right and turning left, negate that
        // gripfactor is withing the range of 0 - 1. 0 no grip, 1 full grip

        float desiredVelocityChange = -steeringVelocity * tireGrip;

        // Turn change in velocity into an acceleration (Acceleration =  deltaVel / time)
        // this will produce the accelerationn necessary to change the velocity 
        // by desired in 1 physics tick

        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

        // Force = Mass * acceleration. 
        Vector3 steerForce = tireMass * desiredAcceleration * steeringDir;
        if(debugRaycasts){
            Debug.DrawRay(Tire.transform.position, steerForce / 10, Color.blue);
        }
        _rigidBody.AddForceAtPosition(steerForce / 10, Tire.position);
    }
    /// <summary>
    /// Apply forward force in the Z direction of tires
    /// </summary>
    /// <param name="Tire"></param>
    /// <param name="tireCount"></param>
    void applyTireAcceleration(Transform Tire, int tireCount)
    {
        //only makes it so back tires accelerate while drifting
        //if(isDrifting && tireCount < halfTireLength) return;

        Vector3 accelerationDirection = _accelerationAmount * Tire.forward;

        if (Mathf.Abs(_throttleInput) > 0.0f)
        {
            // Forward speed of the car 
            float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);

            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _vehicleTopSpeed);

            float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * _throttleInput;

            _rigidBody.AddForceAtPosition(availableTorque * accelerationDirection, Tire.position);

            //Debug.DrawRay(Tire.position, 0.1f * availableTorque * accelerationDirection,tireCount > halfTireLength ? Color.blue : Color.green);
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

            //Back tires
        else if (tireCount > halfTireLength)
        {
            tireGrip = 1f;
            /*
            float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);

            //float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _carTopSpeed);
            //Lazy fix
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _tireGripHackFix);
            tireGrip = _tireGripCurve.Evaluate(normalizedSpeed);*/
        }
        return tireGrip;
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