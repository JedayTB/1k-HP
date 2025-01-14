using UnityEngine;
using UnityEngine.UI;

public class HookshotController : A_Ability
{
  [Header("Ability Basics")]
  [SerializeField] private float _maxHookDistance = 100f;
  [SerializeField] private Image _hookCrossHair;
  [SerializeField] private LayerMask _HookShottableLayers;
  [SerializeField] private Collider _selfCollider;

  [Header("Hook shot Specifics")]

  [SerializeField] LineRenderer _lineRenderer;
  Transform  _camera;
  SpringJoint _springJoint;

  [SerializeField] private float _maxSpringDistanceMultiplier = 0.8f;
  [SerializeField] private float _minSpringDistanceMultiplier = 0.25f;

  [SerializeField] private float _springForceMultiplier = 1.3f;
  [SerializeField] private float _springDamper = 7f;
  [SerializeField] private float _massScale = 4.5f;

  [SerializeField] private float lerpSpeed = 6f;

  bool hookshotPositionLocked = false;
  bool isUsingHookshot = false;

    private bool _canUseAbility = false;
    private int _selfColliderID;
  Vector3 _hookShotPos;

  void Awake()
  {
    if (_selfCollider == null)
    {
      Debug.LogError("Set vehicles main Collider in Inspector!");
    }
    else
    {
      _selfColliderID = _selfCollider.GetInstanceID();
    }
        onAbility = AbilityUsed;
        _camera = Camera.main.transform;
    UIController uiCont = FindAnyObjectByType<UIController>();
    _hookCrossHair = uiCont.HookshotCrosshair;
    _hookCrossHair.gameObject.SetActive(true);
  }
  
  private void startAbility()
  {
      _canUseAbility = true;
      _hookCrossHair.gameObject.SetActive(true);

      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
        print("Start hookshot ability");
  }


  void Update()
  {

      if (_canUseAbility == true)
      {
          getHookShotTarget();
      }
      /*
      bool usingAbility = inputManager.usedAbility;
      bool holdingAbility = isUsingHookshot == true && usingAbility == true;

      if (holdingAbility == true) {
          grappleTowardsPoint(_springJoint);
      }
      else if(holdingAbility == false) 
      {
          onGrappleStop();
      }
      */

  }
  private void LateUpdate()
  {
      DrawRope();
  }
  private void onGrappleStop()
  {
      Destroy(_springJoint);
      vehicle.VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.None;  
      // Avoids "marked as destroy" bullshit
      _springJoint = null;
      isUsingHookshot = false;
  }
  private void grappleTowardsPoint(SpringJoint sprJoint)
  {
        vehicle.VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.FreezeRotationZ;
      sprJoint.maxDistance = LerpAndEasings.ExponentialDecay(_springJoint.maxDistance, _springJoint.minDistance, lerpSpeed, Time.deltaTime);

  }

  private void getHookShotTarget()
  {
      Ray aimray = Camera.main.ScreenPointToRay(Input.mousePosition);

      if (GameStateManager.Instance.UseDebug) Debug.DrawRay(aimray.origin, aimray.direction * _maxHookDistance);

      Physics.Raycast(aimray.origin, aimray.direction, out RaycastHit hitInfo, _maxHookDistance, _HookShottableLayers);

      // Logic when locking a position
      if (hitInfo.collider != null && hitInfo.collider.GetInstanceID() != _selfColliderID && Input.GetMouseButtonDown(0))
      {
          Debug.LogWarning("Ability isn't configured to use Input Manager");
          _hookCrossHair.enabled = true;
          _hookShotPos = hitInfo.point;

          hookshotPositionLocked = true;
      }
      //Logic if no position locked and not hitting any object
      else if (hookshotPositionLocked == false && hitInfo.collider == null) 
      {
          //Because 0,0,0 is a valid hookshot position
          _hookShotPos = transform.position;
          _hookCrossHair.gameObject.SetActive(false);  
          _lineRenderer.positionCount = 0;
      }

      Vector2 UIMove = Camera.main.WorldToScreenPoint(_hookShotPos);
      if (_hookCrossHair.enabled) _hookCrossHair.transform.position = UIMove;
  }

  /// <summary>
  /// Is a holding action
  /// </summary>
  public override void AbilityUsed()
  {
        if (_canUseAbility == false)
        {
            startAbility();
        }
      else if(_canUseAbility == true)
        {
            useHookshot();
        }
  }
    private void useHookshot()
    {
        if (_hookShotPos != transform.position)
        {
            isUsingHookshot = true;
            InitializeSpringJoint(_hookShotPos);
            InitializeLineRenderer();
        }
    }

  private void DrawRope()
  {
      // If not grappling, don't draw rope
      if (_springJoint != null)
      {
          //lerp positions somewhere in here
          _lineRenderer.positionCount = 2;

          _lineRenderer.SetPosition(0, transform.position);
          _lineRenderer.SetPosition(1, _hookShotPos);
      }
      else
      {
          _lineRenderer.positionCount = 0;
      }
  }
  private void InitializeSpringJoint(Vector3 springJointPosition)
  {
      // Dont make a new one if it exists!
      if (_springJoint != null) return;


      _springJoint = this.gameObject.AddComponent<SpringJoint>();

      _springJoint.autoConfigureConnectedAnchor = false;
      _springJoint.connectedAnchor = springJointPosition;

      float distanceFromPoint = Vector3.Distance(transform.position, _hookShotPos);

      //Configure later
      _springJoint.maxDistance = distanceFromPoint * 0.8f;
      _springJoint.minDistance = distanceFromPoint * 0.25f;

      _springJoint.spring = _springForceMultiplier * vehicle.VehiclePhysics.RigidBody.mass;
      _springJoint.damper = _springDamper;
      _springJoint.massScale = _massScale;

      print("lr summon");
  }
  private void InitializeLineRenderer()
  {
      _lineRenderer.positionCount = 2;
  }
  
}
