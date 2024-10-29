using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomCarPhysics : MonoBehaviour
{
    #region Variables
    private float _throttleInput;
    private float _turningInput;
    private Transform _transform;

    [Header("Basic Setup")]
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private CustomWheels[] wheels;

    private int halfTireLength;
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
    //Steering

    [Header("Steering Setup")]
    //Steering
    [SerializeField] private bool _frontWheelSteer = true;
    [SerializeField] private float _rotationAngleTimeToZero = 1.5f;
    [SerializeField] private AnimationCurve _tireGripCurve;
    [SerializeField] private float _tireGripHackFix = 100f;

    public bool isDrifting = false;
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
        _baseAccelerationAmount = _accelerationAmount;
        halfTireLength = wheels.Length / 2;
        foreach(var tire in wheels){
            tire.init(_rigidBody);
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

        for (int i = 0; i < wheels.Length; i++)
        {
            applyTireRotation(wheels[i], i, _lowerClamp, _higherClamp);

            wheels[i].raycastDown(_groundLayers);

            wheels[i].applyTireSuspensionForces();

            float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _vehicleTopSpeed);
            float availableTorque = torqueCurve.Evaluate(normalizedSpeed) * _throttleInput;

            wheels[i].applyTireAcceleration(_throttleInput, _accelerationAmount, availableTorque);
            //Janky but fuck it
            if (isDrifting)
            {
                //print("during drift?");
                wheels[i].applyTireSlide(0.1f, 1f, isDrifting);
            }
            else
            {
                wheels[i].applyTireSlide(0.1f, 1f, isDrifting);
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
    float applyTireRotation(CustomWheels Tire, int tireCount, float lowerClamp, float higherClamp)
    {
        float tireGrip = 5f;

        if (tireCount < halfTireLength)
        {
            Vector3 tireRotation = Vector3.zero;

            if (Mathf.Abs(_turningInput) < 0.1f)
            {
                //When Player let's go of X input, lerp to 0

                _durationOfAngleTiming += Time.fixedDeltaTime;
                _elapsedTime = _durationOfAngleTiming / _rotationAngleTimeToZero;

                frontTiresRotationAngle = Mathf.Lerp(frontTiresRotationAngle, 0, _elapsedTime);

                tireRotation.y = frontTiresRotationAngle;

                
                Tire.setTireRotation(Quaternion.Euler(tireRotation));
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

                frontTiresRotationAngle += _turningInput * tireGrip;

                frontTiresRotationAngle = Mathf.Clamp(frontTiresRotationAngle, lowerClamp, higherClamp);

                tireRotation.y = frontTiresRotationAngle;

                Tire.setTireRotation(Quaternion.Euler(tireRotation));
            }

        }

            //Back tires
        else if (tireCount > halfTireLength)
        {
            tireGrip = 1f;
            
            float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);

            //float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _carTopSpeed);
            //Lazy fix
            float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _tireGripHackFix);
            tireGrip = _tireGripCurve.Evaluate(normalizedSpeed);
        }
        return tireGrip;
    }
}