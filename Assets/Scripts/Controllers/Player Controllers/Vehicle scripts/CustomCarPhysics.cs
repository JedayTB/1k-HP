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
    [Header("Basic Setup")]
    [SerializeField] private float _raycastDistance = 1.5f;
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private CustomWheels[] wheels;
    public CustomWheels[] WheelArray { get => wheels; }
    private int halfTireLength;
    private Rigidbody _rigidBody;

    [SerializeField] private float _max_RB_ZRotationAngle = 15f;
    [SerializeField] private float _max_RB_XRotationAngle = 20f;
    Vector3 cachedRbRotation;
    public Rigidbody RigidBody { get => _rigidBody; }

    //Accelerations

    [Header("Acceleration Setup")]
    [Tooltip("Top speed of the car")]
    [SerializeField] private float _terminalVelocity = 500f;
    [Tooltip("How fast the car accelerates")]
    [SerializeField] private float _accelerationAmount = 3500f;
    private float _baseAccelerationAmount;
    [Tooltip("How much force is available at certain speeds.")]
    [SerializeField] private AnimationCurve torqueCurve;
    //Steering

    [Header("Steering Setup")]
    [Tooltip("The Distance between the Front and Back tires")]
    [SerializeField] public float wheelbase = 5f;
    [Tooltip("Minimum Space rquired to turn Vehicle 180 degree's in metres")]
    [SerializeField] public float turnRadius = 10f;
    [Tooltip("Distance between back Wheels")]
    [SerializeField] private float rearTrack = 2f;

    [SerializeField] private float _rotationAngleTimeToZero = 1.5f;
    [SerializeField] private float _tireGripHackFix = 100f;
    [HideInInspector] public bool isDrifting = false;
    private float _durationOfAngleTiming;
    private float _elapsedTime;

    #endregion

    #region Public use Methods
    public void Init()
    {

        _rigidBody = GetComponentInChildren<Rigidbody>();

        _transform = transform;
        _baseAccelerationAmount = _accelerationAmount;
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
        
        float count = 0f;
        _accelerationAmount = _baseAccelerationAmount * _nitroMultiplier;
        isUsingNitro = true;
        //Set invunerable to offroad / physicsMaterials below when implemented
        while (count <= nitroTiming)
        {
            count += Time.deltaTime;
            yield return null;  // Might set to small timings between checks. maybe waitForSeconds(0.25f) or so
                                // this would make nitro timing 4x longer. don't do!
        }
        isUsingNitro = false;
        _accelerationAmount = _baseAccelerationAmount;
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
        for(int i = 0; i < wheels.Length; i++)
        {
            applyTireRotation(wheels[i], i);
        }
    }

    /// <summary>
    /// Rotates the tire for use in tire accelleration. Changes Z axis
    /// lerps tire rotation back to 0 if no input detected
    /// </summary>
    /// <param name="Tire"></param>
    /// <param name="tireCount"></param>
    /// <returns> Returns current tire grip calculated using _tireGripCurve. Still has side effect on tires if returns.</returns>
    void applyTireRotation(CustomWheels Tire, int tireCount)
    {
        float tireYAngle = 0f;
        //Only Steer front tires
        if (tireCount < halfTireLength)
        {

            if (Mathf.Abs(_turningInput) < 0.1f)
            {
                //When Player let's go of X input, lerp to 0
                _durationOfAngleTiming += Time.fixedDeltaTime;

                _elapsedTime = _durationOfAngleTiming / _rotationAngleTimeToZero;

                tireYAngle = Mathf.Lerp(tireYAngle, 0, _elapsedTime);

                Tire.setTireRotation(tireYAngle);
            }
            else
            {
                _durationOfAngleTiming = 0;

                Tire.TurnTire(_turningInput);
            }

        }

    }

    #region Physics Simulations

    void FixedUpdate()
    {

        for (int i = 0; i < wheels.Length; i++)
        {
            

            wheels[i].raycastDown(_groundLayers, _raycastDistance);

            if (wheels[i].TireIsGrounded)
            {
                wheels[i].applyTireSuspensionForces();

                float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);
                float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _terminalVelocity);

                float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * _throttleInput;



                wheels[i].applyTireAcceleration(_throttleInput, _accelerationAmount, availableTorque);

                if (isDrifting)
                {

                    wheels[i].applyTireSlideOnDrift(0.1f, 1f);
                }
                else
                {
                    wheels[i].applyTireSlide(1f);
                }
            }
            
        }



    }
    
    #endregion
}