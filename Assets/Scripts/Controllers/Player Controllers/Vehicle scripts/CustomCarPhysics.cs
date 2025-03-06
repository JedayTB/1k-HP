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
    [HideInInspector] public bool isDrifting = false;

    [Header("Basic Setup")]
    [SerializeField] private float _raycastDistance = 1.5f;
    [SerializeField] private LayerMask _groundLayers;
    [SerializeField] private CustomWheels[] wheels;

    public CustomWheels[] WheelArray { get => wheels; }
    private int halfTireLength;
    private Rigidbody _rigidBody;
    public Rigidbody RigidBody { get => _rigidBody; }
    [SerializeField] private Transform colliderFront;

    [Header("Angle Clamping")]

    [SerializeField] private float minXAngle = -10, maxXAngle = 10;
    [SerializeField] private float minZAngle = -15, maxZAngle = 15;

    [Header("Gears")]
    // Gear specifics

    [SerializeField] public VehicleGearSpecs GearOne;
    [SerializeField] public VehicleGearSpecs GearTwo;
    // Only for UI display
    [HideInInspector] public string gearText = "Low";

    [Header("Acceleration setup")]

    private static float driftTireWeightMultiplier = 0.35f;
    private static float tireGripWhileDrifting = 0.1f;
    [HideInInspector] public float horsePower;
    public float currentTopSpeed;
    private static float nitroMaxSpeedMultiplier = 1.5f;

    public static readonly float _terminalVelocity = 200f;
    public float TerminalVelocity { get => _terminalVelocity; }
    private Vector3 cachedLocalVelocity;
    private VehicleGearSpecs currentGear;
    [SerializeField] private float momentumModifier;

    [Header("Steering Setup")]

    [Tooltip("Grip Of wheels Between speed 0 - Terminal Velocity")]
    [SerializeField] private AnimationCurve tireGripCurve;

    [SerializeField] private float tireTurnModifier = 1;
    [SerializeField] private float minimumModifier = 0.25f;

    [Tooltip("The Distance between the Front and Back tires")]
    [SerializeField] public float wheelbase = 5f;

    [Tooltip("Minimum Space rquired to turn Vehicle 180 degree's in metres")]
    [SerializeField] public float turnRadius = 10f;

    [Tooltip("Distance between back Wheels")]
    [SerializeField] private float rearTrack = 2f;


    [Header("RigidBody mod Settings")]
    [Tooltip("The minimum angluar drag the car will experience For steering mechanics")]
    [SerializeField] private float minimumAngularDrag = 0.05f;
    [Tooltip("The maximum angular drag the car will experience For steering mechanics")]
    [SerializeField] private float maximumAngularDrag = 0.5f;

    [Header("Ground Stick Setup")]
    private static float groundCheckDistance = 1.5f;
    private static float additionalGravity = 2f;
    private static float GravConstant = 9.806f;

    [Header("Collisoin Bump Properties")]
    [SerializeField] private float headOnCollisionThreshold = 0.85f;
    [SerializeField] private float upDownThreshold = -0.2f;
    [SerializeField] private Vector3 collisionPoint;
    [SerializeField] private float collisionRayDistance = 1;

    private float collisionCooldown = 0;

    #endregion

    #region Public use Methods
    public void Init()
    {

        _rigidBody = GetComponentInChildren<Rigidbody>();

        _transform = transform;
        currentGear = GearOne;

        horsePower = GearOne.HorsePower;
        currentTopSpeed = GearOne.MaxSpeed;
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
    public void ShiftGears(float delta)
    {
        if (delta != 0)
        {
            currentGear = delta > 0 ? GearTwo : GearOne;
            gearText = delta > 0 ? "High" : "Low";
            horsePower = currentGear.HorsePower;
            currentTopSpeed = currentGear.MaxSpeed;
            foreach (var wheel in wheels)
            {
                wheel.setTimings(0.25f, 0.25f);
            }
        }

    }
    public float getSpeed()
    {
        return Vector3.Dot(_transform.forward, _rigidBody.velocity);
    }
    public Vector3 getVelocity()
    {
        return _rigidBody.velocity;
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
    public IEnumerator useNitro(float _nitroMultiplier, float nitroTiming, float NAccelTimgMult)
    {
        float count = 0f;
        float pastTopSpeed = currentGear.MaxSpeed;
        horsePower = currentGear.HorsePower * _nitroMultiplier;
        isUsingNitro = true;

        currentTopSpeed *= nitroMaxSpeedMultiplier;
        foreach (var w in WheelArray)
        {
            w.multiplyTimings(NAccelTimgMult, NAccelTimgMult);
        }

        while (count <= nitroTiming)
        {
            count += Time.deltaTime;
            yield return null;  // Might set to small timings between checks. maybe waitForSeconds(0.25f) or so
                                // this would make nitro timing 4x longer. don't do!
        }
        isUsingNitro = false;
        horsePower = currentGear.HorsePower;
        currentTopSpeed = pastTopSpeed;

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

        tireTurnModifier = Mathf.Max(1 - (_rigidBody.velocity.magnitude / _terminalVelocity), minimumModifier);

        //Tire turning shit
        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].setInputs(_throttleInput, _turningInput);

            if (i < halfTireLength)
            {
                wheels[i].TurnTire(tireTurnModifier);
            }
        }
    }


    #region Physics Simulations

    void FixedUpdate()
    {
        float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _terminalVelocity);

        float tireGrip = tireGripCurve.Evaluate(normalizedSpeed);
        //To Resist turning at higher speeds
        Vector3 momentumDir = transform.forward;

        float speedModifier = 1 - normalizedSpeed;


        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].raycastDown(_groundLayers, _raycastDistance);
            if (wheels[i].TireIsGrounded)
            {
                wheels[i].applyTireSuspensionForces();

                //wheels[i].applyTireGroundStickForce();
                wheels[i].applyTireAcceleration(horsePower, currentGear.AxleEfficiency, tireGrip, _throttleInput);

                if (isDrifting)
                {
                    wheels[i].applyTireSlideOnDrift(tireGripWhileDrifting, driftTireWeightMultiplier);
                }
                else
                {
                    wheels[i].applyTireSlide(tireGrip);
                }
            }
        }
        cachedLocalVelocity = _rigidBody.velocity;
        cachedLocalVelocity.z = Mathf.Clamp(cachedLocalVelocity.z, -currentTopSpeed, currentTopSpeed);
        cachedLocalVelocity.y = Mathf.Clamp(cachedLocalVelocity.y, -currentTopSpeed, currentTopSpeed);
        cachedLocalVelocity.x = Mathf.Clamp(cachedLocalVelocity.x, -currentTopSpeed, currentTopSpeed);

        _rigidBody.velocity = cachedLocalVelocity;

        stickToGround();
        ClampLocalRotation();
    }
    private void ClampLocalRotation()
    {
        Vector3 localEulerAngles = transform.localRotation.eulerAngles;

        localEulerAngles.x = ClampAngle(localEulerAngles.x, minXAngle, maxXAngle);
        localEulerAngles.z = ClampAngle(localEulerAngles.z, minZAngle, maxZAngle);

        transform.localRotation = Quaternion.Euler(localEulerAngles);
    }
    public static float ClampAngle(float current, float min, float max)
    {
        float dtAngle = Mathf.Abs(((min - max) + 180) % 360 - 180);
        float hdtAngle = dtAngle * 0.5f;
        float midAngle = min + hdtAngle;

        float offset = Mathf.Abs(Mathf.DeltaAngle(current, midAngle)) - hdtAngle;
        if (offset > 0)
            current = Mathf.MoveTowardsAngle(current, midAngle, offset);
        return current;
    }


    private void stickToGround()
    {
        bool isGrounded = Physics.Raycast(transform.position, -transform.up, groundCheckDistance, _groundLayers);
        //if (GameStateManager.Instance.UseDebug) Debug.DrawRay(transform.position, -transform.up * groundCheckDistance, isGrounded == true ? Color.green : Color.red);

        float curentAngDrag = isGrounded ? minimumAngularDrag : maximumAngularDrag;
        _rigidBody.angularDrag = LerpAndEasings.ExponentialDecay(_rigidBody.angularDrag, curentAngDrag, 10f, Time.deltaTime);

        if (isGrounded == false)
        {
            float speedGravMultiplier = Mathf.Lerp(1, 2, _rigidBody.velocity.y / 25f);
            Vector3 extraGravStrength = (additionalGravity * GravConstant * speedGravMultiplier) * Vector3.down;
            _rigidBody.AddForce(extraGravStrength, ForceMode.Acceleration);

            //if (GameStateManager.Instance.UseDebug) print($"Ex Grav {extraGravStrength}: y vel mult = {speedGravMultiplier}");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("PLAYER"))
        {
            ContactPoint cpoint = collision.GetContact(0);

            NewCollisionBump(cpoint);

        }
    }

    private void NewCollisionBump(ContactPoint contactPoint)
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        float contactForce = contactPoint.impulse.magnitude / 2;
        collisionPoint = contactPoint.point;

        Physics.Raycast(ray, out hit, collisionRayDistance);
        //Debug.DrawRay(transform.position, transform.forward * collisionRayDistance, Color.blue);
        Vector3 orientaion = transform.rotation.eulerAngles;
        orientaion.x = orientaion.x > 0 ? -90 : 90;

        GameStateManager.Instance.spawnSkidParticles(contactPoint.point, orientaion, _rigidBody.velocity.magnitude / 4);

        Vector3 dirToCollisionPoint = (contactPoint.point - transform.position).normalized;
        float upDownDot = Vector3.Dot(-transform.up, dirToCollisionPoint);

        if (upDownDot < 0)
        {
            float ZEROTO1 = contactForce / 4000f;
            float intensity = Mathf.Lerp(0f, 2f, ZEROTO1);
            float harshness = Mathf.Lerp(0.5f, 1f, ZEROTO1);
            float duration = Mathf.Lerp(float.Epsilon, 0.5f, ZEROTO1);
            CameraShake.Instance.StartShake(duration, intensity, harshness, true, 0.5f);

            if (hit.collider != null && !hit.collider.gameObject.CompareTag("Vehicle"))
            {
                //Debug.Log("STOP NOW!");
                //Debug.DrawRay(transform.position, transform.forward * collisionRayDistance, Color.blue);
                _rigidBody.AddForce(-transform.forward * contactForce, ForceMode.Impulse);
            }
            else
            {
                //Debug.Log("WE ARE skidding");
                //Debug.DrawRay(transform.position, transform.forward * collisionRayDistance, Color.red);

                contactForce = Mathf.Clamp(contactForce, 1000f, 5000f);

                _rigidBody.AddForceAtPosition(transform.forward * contactForce, contactPoint.point, ForceMode.Impulse);
                //Debug.Log("applying force at " + contactPoint.point);
                //
            }
        }
    }
    #endregion
}
