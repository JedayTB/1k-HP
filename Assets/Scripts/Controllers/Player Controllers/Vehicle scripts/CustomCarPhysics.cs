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
  public Rigidbody RigidBody { get => _rigidBody; }

  public AudioSource driftAudio;

  //Accelerations

  [Header("Acceleration Setup")]
  [Tooltip("Top speed of the car")]
  [SerializeField] public float _terminalVelocity = 250f;
  [Tooltip("How fast the car accelerates")]
  [SerializeField] public float Acceleration = 3500f;
  private float _baseAccelerationAmount;
  [Tooltip("How much force is available at certain speeds.")]
  [SerializeField] private AnimationCurve torqueCurve;
  [SerializeField] private AnimationCurve tireGripCurve;
  //Steering

  [Header("Steering Setup")]
  [Tooltip("The Distance between the Front and Back tires")]
  [SerializeField] public float wheelbase = 5f;
  [Tooltip("Minimum Space rquired to turn Vehicle 180 degree's in metres")]
  [SerializeField] public float turnRadius = 10f;
  [Tooltip("Distance between back Wheels")]
  [SerializeField] private float rearTrack = 2f;
  [SerializeField] private float timeToFullTurnAnggle = 3;
  [HideInInspector] public bool isDrifting = false;
  private float turnProgress = 0f;

  #endregion

  #region Public use Methods
  public void Init()
  {

    _rigidBody = GetComponentInChildren<Rigidbody>();

    _transform = transform;
    _baseAccelerationAmount = Acceleration;
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

    print($"{this.gameObject.name} finished init");
  }

  public void setInputs(float throttleAmt, float turningAmt)
  {
    _throttleInput = throttleAmt;
    _turningInput = turningAmt;
  }
  public float getSpeed()
  {
    return _rigidBody.velocity.magnitude;
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
  public IEnumerator useNitro(float _nitroMultiplier, float nitroTiming)
  {

    float count = 0f;
    Acceleration = _baseAccelerationAmount * _nitroMultiplier;
    isUsingNitro = true;
    //Set invunerable to offroad / physicsMaterials below when implemented
    while (count <= nitroTiming)
    {
      count += Time.deltaTime;
      yield return null;  // Might set to small timings between checks. maybe waitForSeconds(0.25f) or so
                          // this would make nitro timing 4x longer. don't do!
    }
    isUsingNitro = false;
    Acceleration = _baseAccelerationAmount;
  }

  public void driftVehicle(bool isUsingDrift)
  {
    if (isDrifting == false && isUsingDrift == true)
    {
      isDrifting = true;
    }

    /*
    if (isDrifting == true){
        driftAudio.Play();
    }
    if (driftAudio.isPlaying == true & isDrifting == false)
    {
        driftAudio.Stop();
    }
    */
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
    for (int i = 0; i < wheels.Length; i++)
    {
      if (i < halfTireLength)
      {
        //Tire turning shit
        //
        //  
        if (Mathf.Abs(_turningInput) > 0)
        {

          turnProgress += Time.deltaTime / (timeToFullTurnAnggle / Mathf.Abs(_turningInput));
        }
        else
        {
          turnProgress = 0f;
        }
        applyTireRotation(wheels[i], turnProgress);
      }
    }

  }

  /// <summary>
  /// Tells the tire to turn!
  /// </summary>
  /// <param name="Tire">The tire to turn</param>
  void applyTireRotation(CustomWheels Tire, float tireGrip)
  {
    Tire.TurnTire(_turningInput, tireGrip);
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

        float tireGrip = tireGripCurve.Evaluate(normalizedSpeed);
        //Cetrifugal motion to affect turnRadius
        wheels[i].applyTireAcceleration(Acceleration, availableTorque);

        // If the forward vector angle of the tire is past a certain threshold of the 
        // Forward vector of the car, skid (lose traction)
        // find a way to put speed into the calculation
        if (isDrifting)
        {
          wheels[i].applyTireSlideOnDrift(0.1f, 1f);
        }
        else
        {
          wheels[i].applyTireSlide(tireGrip);
        }
      }
    }



  }

  #endregion
}
