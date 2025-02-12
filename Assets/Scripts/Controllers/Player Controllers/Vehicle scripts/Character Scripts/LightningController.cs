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

  [SerializeField] private float _maxLightningDistance = 50f;
  [SerializeField] private float cubeSize = 0.5f;
  [SerializeField] private float lightningFadeoutTime = 1.1f;
  [SerializeField] private float timeTillLightningHit = 0.05f;

  private int bubbleLayer;
  private bool _canUseAbility = false;
  private A_VehicleController lightningTarget;
  private int selfColliderID;

  // InputManager inputManager
  // Update is called once per frame
  private void initializeLightning()
  {
    selfColliderID = selfCollider.GetInstanceID();
    UIController uiCont = FindAnyObjectByType<UIController>();
    crosshair = uiCont.lightningCrossHair;
    crosshair.gameObject.SetActive(false);

    bubbleLayer = LayerMask.NameToLayer("Bubble");

    resetLightning();
    Debug.Log("Lightning Ability started");
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
    _canUseAbility = false;
    resetCrosshair();
    resetLightning();
  }
  void resetLightning()
  {
    lightningTarget = null;
    crosshair.transform.position = Camera.main.WorldToScreenPoint(Vector3.zero);
    CursorController.setDefaultCursor();
  }

  public override void AbilityUsed()
  {
    if (_canUseAbility == false)
    {
      startAbility();
    }
    else if (_canUseAbility == true)
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
  private void resetCrosshair()
  {
    crosshair.transform.position = Camera.main.WorldToScreenPoint(Vector3.zero);
    crosshair.gameObject.SetActive(false);
  }

  private void onVehicleHit()
  {
    print($"Hit {lightningTarget.name}. Respawning");
    lightningTarget.respawn();
    resetCrosshair();
  }
  IEnumerator fireLightningTo(float lightningToDestTime, Vector3 target)
  {
    float count = 0;
    Vector3 lerpedPos = transform.position;

    _lr.positionCount = 2;

    float smoothedProgress = 0;
    while (count < lightningToDestTime)
    {
      count += Time.deltaTime;

      smoothedProgress = count / lightningToDestTime;
      smoothedProgress = LerpAndEasings.ExponentialDecay(smoothedProgress, 1, 7, Time.deltaTime);

      lerpedPos = Vector3.Lerp(transform.position, target, smoothedProgress);

      _lr.SetPosition(0, transform.position);
      _lr.SetPosition(1, lerpedPos);

      yield return null;
    }
    _lr.positionCount = 0;
    onVehicleHit();
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
      getAbiliyTarget();
      positionCrosshairOnTarget();
    }
  }

  private void getAbiliyTarget()
  {
    //Debug.LogWarning("Ability isn't configured to use Input Manager");

    Ray aimray = Camera.main.ScreenPointToRay(Input.mousePosition);

    if (GameStateManager.Instance.UseDebug) Debug.DrawRay(aimray.origin, aimray.direction * _maxLightningDistance);

    RaycastHit hitInfo;
    bool hitsomething = Physics.Raycast(aimray.origin, aimray.direction, out hitInfo, _maxLightningDistance, VehicleLayer);


    if (hitsomething == true && hitInfo.collider.GetInstanceID() != selfColliderID)
    {
      //beautiful code
      //print($"{hitInfo.collider.gameObject.transform.parent.parent.name} id {hitInfo.collider.GetInstanceID()}");
      A_VehicleController vehicleTarget = hitInfo.collider.gameObject.transform.parent.parent.gameObject.GetComponent<A_VehicleController>();

      if (vehicleTarget != null) lightningTarget = vehicleTarget;
    }
    else
    {
      return;
    }

    float distanceToTarget = Vector3.Distance(transform.position, lightningTarget.transform.position);

    if (distanceToTarget > _maxLightningDistance)
    {
      lightningTarget = null;
      crosshair.gameObject.SetActive(false);
    }
  }
  private void positionCrosshairOnTarget()
  {
    if (lightningTarget != null)
    {
      /*
      float frontbackcheck = Vector3.Dot(transform.forward, lightningTarget.transform.position);
      if (frontbackcheck < 0)
      {
        crosshair.gameObject.SetActive(false);
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
      Debug.Log(hit.collider.gameObject.layer);

      int layerNum = hit.collider.gameObject.layer;

      if (hitVehicle && layerNum == bubbleLayer)
      {
        Debug.Log("Hit bubble.");
      }
      else if (hitVehicle && layerNum != bubbleLayer)
      {
        resetCrosshair();
        StartCoroutine(fireLightningTo(timeTillLightningHit, hit.point));
      }
    }
  }


}
