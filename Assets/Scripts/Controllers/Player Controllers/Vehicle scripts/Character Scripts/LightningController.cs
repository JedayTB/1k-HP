using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LightningController : A_Ability
{
  [Header("Lightning Ability variables")]
  [SerializeField] private Image crosshair;
  [SerializeField] private LineRenderer _lr;
  [SerializeField] private Collider selfCollider;
  [SerializeField] private LayerMask VehicleLayer;
  private Ray aimRay;
  public static float _maxLightningDistance = 50f;

  [SerializeField] private float cubeSize = 0.5f;
  [SerializeField] private float lightningFadeoutTime = 1.1f;
  [SerializeField] private float timeTillLightningHit = 0.05f;

  [SerializeField] private int bubbleLayer;
  [SerializeField] private bool _canUseAbility = false;
  [SerializeField] private A_VehicleController lightningTarget;
  [SerializeField] private int selfColliderID;

  private Ray AimRay;
  private AbilityState state;
  // InputManager inputManager
  // Update is called once per frame
  private void initializeLightning()
  {
    selfColliderID = selfCollider.GetInstanceID();
    UIController uiCont = FindAnyObjectByType<UIController>();
    crosshair = uiCont.lightningCrossHair;
    crosshair.gameObject.SetActive(false);
    bubbleLayer = LayerMask.NameToLayer("Bubble");

    state = vehicle is PlayerVehicleController ? AbilityState.UserIsPlayer : AbilityState.UserIsAI;

    Debug.Log("Lightning Ability Initialized");
  }
  protected override void Awake()
  {
    base.Awake();
    initializeLightning();
  }

  protected override void OnDisable()
  {
    base.OnDisable();
    Debug.Log("Reset Lightning abilitty");
    resetAbility();
  }
  void resetAbility()
  {
    lightningTarget = null;
    _canUseAbility = false;

    crosshair.transform.position = Camera.main.WorldToScreenPoint(Vector3.zero);
    CursorController.setDefaultCursor();
    crosshair.gameObject.SetActive(false);
  }

  /// OnAbility
  public override void AbilityUsed()
  {
    if (_canUseAbility == false)
    {
      startAbility();
    }
    else if (_canUseAbility == true && lightningTarget != null)
    {
      strikeLightning();
      CursorController.setDefaultCursor();
    }

  }

  private void startAbility()
  {
    _canUseAbility = true;
    crosshair.gameObject.SetActive(false);
    CursorController.setNewCursor(GameStateManager.Instance.lightningCursor);
    Debug.Log("Lightning started. Start looking for target");
  }
  private void onVehicleHit()
  {
    print($"Hit {lightningTarget.name}. Respawning and resetting lightning");
    lightningTarget.respawn();
    resetAbility();
  }
  IEnumerator fireLightningTo(float lightningToDestTime, Vector3 target, bool hitBubble)
  {
    float count = 0;
    Vector3 lerpedPos = transform.position;

    _lr.positionCount = 2;

    float smoothedProgress = 0;

    while (count < lightningToDestTime)
    {
      count += Time.deltaTime;

      smoothedProgress = count / lightningToDestTime;

      lerpedPos = Vector3.Lerp(transform.position, target, smoothedProgress);

      _lr.SetPosition(0, transform.position);
      _lr.SetPosition(1, lerpedPos);

      yield return null;
    }
    if (hitBubble == false) onVehicleHit();

    _lr.positionCount = 0;
    // No more lightnign if you hit them, or tried to!
    gameObject.SetActive(false);
  }

  ///
  /// Update loop methods
  ///
  void Update()
  {
    if (_canUseAbility)
    {
      getAimDirection();
      getAbiliyTarget();
      positionCrosshairOnTarget();
    }
  }
  private void getAimDirection()
  {
    if (vehicle is PlayerVehicleController)
    {
      aimRay = Camera.main.ScreenPointToRay(Input.mousePosition);
      print("We is player");
    }
    else
    {
      aimRay = new Ray(vehicle.transform.position, vehicle.LightningAimDirection);
    }

  }
  private void getAbiliyTarget()
  {

    if (GameStateManager.Instance.UseDebug) Debug.DrawRay(aimRay.origin, aimRay.direction * _maxLightningDistance);

    RaycastHit hitInfo;
    bool hitsomething = Physics.Raycast(aimRay.origin, aimRay.direction, out hitInfo, _maxLightningDistance, VehicleLayer);

    if (hitsomething == true && hitInfo.collider.GetInstanceID() != selfColliderID)
    {
      //beautiful code
      //print($"{hitInfo.collider.gameObject.transform.parent.parent.name} id {hitInfo.collider.GetInstanceID()}");
      A_VehicleController vehicleTarget = hitInfo.collider.gameObject.transform.parent.parent.gameObject.GetComponent<A_VehicleController>();
      if (vehicleTarget != null)
      {

        lightningTarget = vehicleTarget;
        float distanceToTarget = Vector3.Distance(transform.position, lightningTarget.transform.position);

        if (distanceToTarget > _maxLightningDistance)
        {
          lightningTarget = null;
          crosshair.gameObject.SetActive(false);
        }
      }
    }
  }
  private void positionCrosshairOnTarget()
  {
    if (lightningTarget != null)
    {
      /*
      Vector3 delta = lightningTarget.transform.position - transform.forward;
      delta.Normalize();
      float frontbackcheck = Vector3.Dot(transform.forward.normalized, delta);

      if (frontbackcheck < 0)
      {
        crosshair.gameObject.SetActive(false);
        Debug.Log("target behind vehicle, disable crosshair");
        return;
      }
      */
      Vector2 UIMove = Camera.main.WorldToScreenPoint(lightningTarget.transform.position);
      crosshair.transform.position = UIMove;
      crosshair.gameObject.SetActive(true);
    }
    else if (lightningTarget == null)
    {
      crosshair.gameObject.SetActive(false);
    }
  }
  private void strikeLightning()
  {
    if (lightningTarget != null)
    {
      _canUseAbility = false;
      Vector3 lightningDir = lightningTarget.transform.position - Camera.main.transform.position;
      Vector3 boxSize = new(cubeSize, cubeSize, cubeSize);

      bool hitVehicle = Physics.BoxCast(Camera.main.transform.position, boxSize, lightningDir, out RaycastHit hit,
                                       Quaternion.identity, _maxLightningDistance, VehicleLayer);

      int layerNum = hit.collider.gameObject.layer;
      bool hitBubbleGum = false;
      if (hitVehicle)
      {
        if (layerNum == bubbleLayer)
        {
          Debug.Log("Hit bubble.");
          hitBubbleGum = true;
        }
        StartCoroutine(fireLightningTo(timeTillLightningHit, hit.point, hitBubbleGum));
      }
    }
  }


}
