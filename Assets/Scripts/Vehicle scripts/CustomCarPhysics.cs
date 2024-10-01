using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomCarPhysics : MonoBehaviour
{
    private float _throttleInput;
    private float _turningInput;

    private Transform _transform;
    private Vector3 _respawnPosition;


    [Header("Basic Setup")]
    [SerializeField] private Transform[] Tires;
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private bool debugRaycasts = true;
    [SerializeField] private float _tireRaycastDistance = 0.1f;
    public Rigidbody _rigidBody;
    RaycastHit[] _tireGroundHits;
    //Public members

    //Accelerations

    [Header("Acceleration Setup")]
    [Tooltip("Top speed of the car")]
    [SerializeField] private float _vehicleTopSpeed = 500f;
    [Tooltip("How fast the car accelerates")]
    [SerializeField] private float _accelerationAmount = 3500f;
    [Tooltip("How much force is available at certain speeds.")]
    [SerializeField] private AnimationCurve torqueCurve;
    [SerializeField] private bool _frontWheelDrive = true;

    //suspension

    [Header("Suspension Setup")]

    [Tooltip("The force at which the spring tries to return to rest distanc with")]
    [SerializeField] private float _springStrength;
    [Tooltip("Dampens speed at which spring returns to rest. Lower is bouncy, higher is stiff")]
    [SerializeField] private float _springDamping;
    [Tooltip("Distance in unity units the springs rest below the tire")]
    [SerializeField] private float _springRestDistance;

    //Steering

    [Header("Steering Setup")]
    //Steering
    [SerializeField] private bool _frontWheelSteer = true;

    [Tooltip("Determines If the Back wheels steer the car. If front and back true, all wheels turn. NOTE back wheels are the last two elements of the Tires array.")]
    [SerializeField] private bool _backWheelSteer = false;
    [SerializeField] private float _rotationAngleTimeToZero = 0.5f;
    [SerializeField] private AnimationCurve _tireGripCurve;
    [SerializeField] private float _tireGripHackFix = 100f;
    private float _durationOfAngleTiming;
    private float _elapsedTime;
    //Public members
    public float frontTiresRotationAngle;
    public float backTiresRotationAngle;

    //Public Functions
    public void Init()
    {

        _rigidBody = GetComponentInChildren<Rigidbody>();

        _transform = transform;

        _tireGroundHits = new RaycastHit[Tires.Length];
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
        _accelerationAmount = isUsingNitro ? _accelerationAmount * _nitroMultiplier : _accelerationAmount;
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
                applyTireSlide(Tires[i], i);
                applyTireRotation(Tires[i], i);
                applyTireAcceleration(Tires[i], i);
            }

        }
    }
   
    
    void applyTireRotation(Transform Tire, int tireCount)
    {
        
        if (tireCount < 2 && _frontWheelSteer && _backWheelSteer != true)
        {
            Vector3 tireRotation = Vector3.zero;

            if ((int)_turningInput == 0)
            {
                //When Player let's go of X input
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
                

                float tireGrip = _tireGripCurve.Evaluate(normalizedSpeed);
                
                //Debug.Log($"Car spd {carSpeed} Norm Spd: {normalizedSpeed}, tireGrip { tireGrip}");

                frontTiresRotationAngle += _turningInput * tireGrip;

                tireRotation.y = frontTiresRotationAngle;

                Tire.localRotation = Quaternion.Euler(tireRotation);
            }

        }
        else if (tireCount > 2 && _backWheelSteer && _frontWheelSteer != true)
        {
            Debug.LogError("Back wheel turning not implemented yet");
        }

    }
    void applyTireAcceleration(Transform Tire, int tireCount)
    {

        if (tireCount < 2)
        {
            Vector3 accelerationDirection = _accelerationAmount * Tire.forward;

            if (Mathf.Abs(_throttleInput) > 0.0f)
            {
                // Forward speed of the car 
                float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);

                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _vehicleTopSpeed);

                float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * _throttleInput;

                _rigidBody.AddForceAtPosition(availableTorque * accelerationDirection, Tire.position);
            }
        }
    }
    void applyTireSlide(Transform Tire, int tireCount)
    {


        // world space direction of the spring force
        Vector3 steeringDir = Tire.right;

        //world space velocity of the suspension 
        Vector3 tireWorldVelocity = _rigidBody.GetPointVelocity(Tire.position);

        // What's the velocity in the steering direction?
        // steerinDir is a unit vector, this returns the magnitude of tireWorldVel
        // As projected onto steeringDir

        float steeringVelocity = Vector3.Dot(steeringDir, tireWorldVelocity);

        // The change in velocity that we're looking for is -steeringVel * gripFactor
        // gripfactor is withing the range of 0 - 1. 0 no grip, 1 full grip

        float desiredVelocityChange = -steeringVelocity * 1;

        // Turn change in velocity into an acceleration (Acceleration =  deltaVel / time)
        // this will produce the accelerationn necessary to change the velocity 
        // by desired in 1 physics tick

        float desiredAcceleration = desiredVelocityChange / Time.fixedDeltaTime;

        // Force = Mass * acceleration. 
        Vector3 steerForce = 5 * desiredAcceleration * steeringDir;

        _rigidBody.AddForceAtPosition(steerForce, Tire.position);

    }
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
