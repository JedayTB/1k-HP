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

  //Accelerations

  [Header("Acceleration Setup")]
  [Tooltip("Top speed of the car")]
  [SerializeField] public float _terminalVelocity = 250f;
  [Tooltip("Horse Power of the Vehicle")]
  [SerializeField] public float horsePower = 500f;
  private float _baseHP;
  [SerializeField] private float axleEffiency = 0.85f;

  [Tooltip("Grip")]
  [SerializeField] private AnimationCurve tireGripCurve;
  //Steering

  [Header("Steering Setup")]
  [SerializeField] private float tireTurnModifier = 1;
  [Tooltip("The Distance between the Front and Back tires")]
  [SerializeField] public float wheelbase = 5f;
  [Tooltip("Minimum Space rquired to turn Vehicle 180 degree's in metres")]
  [SerializeField] public float turnRadius = 10f;
  [Tooltip("Distance between back Wheels")]
  [SerializeField] private float rearTrack = 2f;
  [HideInInspector] public bool isDrifting = false;

  #endregion

  #region Public use Methods
  public void Init()
  {

    _rigidBody = GetComponentInChildren<Rigidbody>();

    _transform = transform;
    _baseHP = horsePower;
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
    horsePower = _baseHP * _nitroMultiplier;
    isUsingNitro = true;
    //Set invunerable to offroad / physicsMaterials below when implemented
    while (count <= nitroTiming)
    {
      count += Time.deltaTime;
      yield return null;  // Might set to small timings between checks. maybe waitForSeconds(0.25f) or so
                          // this would make nitro timing 4x longer. don't do!
    }
    isUsingNitro = false;
    horsePower = _baseHP;
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

    tireTurnModifier = Mathf.Max(1 - (_rigidBody.velocity.magnitude / _terminalVelocity), 0.1f);

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

    for (int i = 0; i < wheels.Length; i++)
    {
      wheels[i].raycastDown(_groundLayers, _raycastDistance);

      if (wheels[i].TireIsGrounded)
      {
        wheels[i].applyTireSuspensionForces();

        float carSpeed = Vector3.Dot(_transform.forward, _rigidBody.velocity);
        float normalizedSpeed = Mathf.Clamp01(Mathf.Abs(carSpeed) / _terminalVelocity);

        float tireGrip = tireGripCurve.Evaluate(normalizedSpeed);

        wheels[i].applyTireAcceleration(horsePower, axleEffiency, _throttleInput);

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
