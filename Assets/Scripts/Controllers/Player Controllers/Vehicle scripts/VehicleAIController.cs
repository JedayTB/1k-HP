using System.Collections;
using UnityEngine;

public class VehicleAIController : A_VehicleController
{

  #region AI Variables 
  [Header("AI Basic setup")]
  [SerializeField] NodeCloudRetriever nodeCloudRetriever;
  public aiState currentState = aiState.driving;

  public Vector3 aggregatedDirectionFromNodeCloud;
  public string dbgString;
  private Vector3 _steeringPosition = Vector3.zero;
  [SerializeField] protected float SteerPathingClock = 0.01f;
  [SerializeField] private bool _singleTarget = false;
  [SerializeField] public bool _driveVehicle = true;
  [SerializeField] private bool _circuitedpath = true;

  Transform raycastDirTr;
  private Vector3 previousSteeringPos = Vector3.zero;

  [SerializeField] private bool isReversing = false;
  [SerializeField] private bool steeringOffwall = false;
  [SerializeField] private bool foundOffwallPos = false;

  [SerializeField] private float reachedSteerOffWallDistance = 1f;
  [SerializeField] private SteeringRaycast[] raycastPositions;

  private int quadrantIndex = 0;
  private float effectiveNodeDistance = 15f;
  [Header("Raycast specifics")]

  private float rayHitStrength;

  [SerializeField] private float maxEffectiveDistanceForSteering = 25f;
  [SerializeField] private float frontalRCDivisor = 0.3f;
  [SerializeField] private LayerMask steerAwayFromLayers;
  private float averagedSteerAwayDirection;
  private int amtFrontalChecks = 0;
  private float yAngleToTarget;

  // Lightning Specifics 
  A_VehicleController closestCurrentVehicle;
  private static float lightningDotFOV = 0.7f;
  private bool targetChanged;
  private static float TimeToFireLightning = 3f;
  private float elapsedLockedOnTime = 0f;

  private int _currentTrackOption;

  #endregion

  #region public I_Vehicle Methods

  public override void Init()
  {
    _vehiclePhysics = GetComponent<CustomCarPhysics>();
    _vehicleVisualController = GetComponent<CarVisualController>();

    _vehiclePhysics.Init();
    _vehicleVisualController.Init();

    setRaycastVariables();
    StartCoroutine(AILogic(SteerPathingClock));
  }
  public void Init(NodeCloudUtil nodeCloud)
  {
    base.Init();
    Debug.LogWarning("This probably doesn't work");
  }

  private void setRaycastVariables()
  {
    rayHitStrength = 1f / raycastPositions.Length;
    raycastDirTr = new GameObject().transform;

    for (int i = 0; i < raycastPositions.Length; i++)
    {
      switch (raycastPositions[i].rcType)
      {
        case SteeringRaycastType.frontal:
          amtFrontalChecks++;
          break;

      }
    }
  }
  public override void respawn()
  {
    base.respawn();
  }

  public override void setNewRespawnPosition(Transform newTransform)
  {
    base.setNewRespawnPosition(newTransform);
  }
  public void changeAIState(aiState newState)
  {
    Debug.Log($"AI State has been changed to {newState}");
  }
  #endregion

  #region Helper Methods
  //For use in starting screen animation
  public void startDriving()
  {
    _driveVehicle = true;
  }
  #endregion

  protected override void Update()
  {
    base.Update();
    if (_driveVehicle == false)
    {
      _vehiclePhysics.setInputs(0, 0);
      VehiclePhysics.RigidBody.velocity = Vector3.zero;
    }
  }
  IEnumerator AILogic(float waitTime)
  {

    while (true)
    {

      switch (currentState)
      {

        case aiState.driving:
          generalDrivingLogic();
          break;
        case aiState.usingNitro:
          generalDrivingLogic();
          break;
        case aiState.bubbleAbility:
          bubbleAbilityLogic();
          break;
        case aiState.LightningAbility:
          lightningAbilityLogic();
          break;
      }
      yield return new WaitForSeconds(waitTime);
    }

  }
  #region Switch State Logic
  public void addAbilityToVehicle(addedAbility newAbility)
  {
    switch (newAbility)
    {
      case addedAbility.Bubblegum:
        switchToBubbleState();
        break;

      case addedAbility.Lightning:
        switchToLightningState();
        break;

    }
  }
  public void switchToBubbleState()
  {
    useCharacterAbility();
    currentState = aiState.bubbleAbility;
    Debug.Log($"{this.name} has or tried to use Ability (Bubble)");
  }

  public void switchToLightningState()
  {
    // First use activates ability;
    useCharacterAbility();
    currentState = aiState.LightningAbility;
    elapsedLockedOnTime = 0f;
    Debug.Log($"{this.name} Picked up lightning ability.");
  }

  #endregion
  #region State Specific Mechanics

  private void bubbleAbilityLogic()
  {
    generalDrivingLogic();
  }

  private void lightningAbilityLogic()
  {
    generalDrivingLogic();
    // Can be modified later. Aim to closest Vehicle
    getLightningTarget();
    lightningFireTiming();
  }

  private void lightningFireTiming()
  {
    if (targetChanged)
    {
      print("Reset lightning");
      elapsedLockedOnTime = 0f;
    }
    else
    {
      elapsedLockedOnTime += Time.deltaTime;
      if (elapsedLockedOnTime >= TimeToFireLightning)
      {
        useCharacterAbility();
        print("fired lightning?");
        currentState = aiState.driving;
      }
    }
  }

  private void getLightningTarget()
  {
    A_VehicleController clV = null;
    float closestDistance = float.MaxValue;
    for (int i = 0; i < GameStateManager.Instance.vehicles.Count; i++)
    {
      if (GameStateManager.Instance.vehicles[i] != this)
      {

        float dist = Vector3.Distance(transform.position, GameStateManager.Instance.vehicles[i].transform.position);
        if (dist < closestDistance && dist < LightningController._maxLightningDistance)
        {
          closestDistance = dist;
          clV = GameStateManager.Instance.vehicles[i];
        }
      }
    }
    if (clV != null)
    {
      if (clV != closestCurrentVehicle)
      {
        closestCurrentVehicle = clV;

      }
      Vector3 dir = clV.transform.position - transform.position;
      float dot = Vector3.Dot(transform.forward.normalized, dir.normalized);
      if (dot > lightningDotFOV)
      {
        LightningAimDirection = dir;

      }
      if (GameStateManager.Instance.UseDebug)
      {
        Color c = dot > lightningDotFOV ? Color.yellow : Color.red;
        Debug.DrawRay(transform.position, dir * LightningController._maxLightningDistance, c);
      }
    }
    else
    {
      closestCurrentVehicle = null;
    }
  }
  #endregion
  #region General Driving Methods
  private void generalDrivingLogic()
  {
    if (_driveVehicle)
    {

      calculateAggregateDirectinoFromNodeCloud();
      Debug.DrawRay(transform.position, transform.TransformDirection(aggregatedDirectionFromNodeCloud) * 5, Color.magenta);

      Debug.DrawRay(transform.position, aggregatedDirectionFromNodeCloud * 5, Color.red);
      float turnAmtToDriveTarget = steerVehicleToDestination();
      avoidCollisions(turnAmtToDriveTarget);
      _vehiclePhysics.setInputs(_throttleInput, _turningInput);
    }
  }
  private void calculateAggregateDirectinoFromNodeCloud()
  {

    Vector3 newVal = Vector3.zero;
    int amtNodesInQuadrant = nodeCloudRetriever.CurrentNodes.Count;
    for (int i = 0; i < amtNodesInQuadrant; i++)
    {
      if (Vector3.Distance(transform.position, nodeCloudRetriever.CurrentNodes[i].transform.position) < effectiveNodeDistance)
      {
        newVal += nodeCloudRetriever.CurrentNodes[i].OptimalDrivingDir;
      }
    }
    aggregatedDirectionFromNodeCloud = (newVal / amtNodesInQuadrant).normalized;
    aggregatedDirectionFromNodeCloud = transform.InverseTransformDirection(aggregatedDirectionFromNodeCloud);
  }

  private void avoidCollisions(float turnAmtToDriveTarget)
  {
    Transform tempTransform;
    int amtOfRaycastsHitting = 0;
    int amtFrontalHit = 0;

    averagedSteerAwayDirection = 0f;

    for (int i = 0; i < raycastPositions.Length; i++)
    {
      tempTransform = raycastPositions[i].transform;

      float dist = raycastPositions[i].rcType == SteeringRaycastType.frontal ? maxEffectiveDistanceForSteering * frontalRCDivisor : maxEffectiveDistanceForSteering;
      bool hitACollider = raycastPositions[i].raycastForward(out RaycastHit hit, dist, 0.3f, steerAwayFromLayers);

      if (GameStateManager.Instance.UseDebug) Debug.DrawRay(tempTransform.position, tempTransform.forward * dist, hitACollider ? Color.red : Color.green);
      //Find out how far left / right it is from origin
      if (hitACollider)
      {
        float hitDist = Vector3.Distance(transform.position, hit.point);

        if (hitDist < maxEffectiveDistanceForSteering)
        {
          float steerAwayStrength = 1 - (hitDist / maxEffectiveDistanceForSteering);

          averagedSteerAwayDirection += rayHitStrength * -Mathf.Sign(raycastPositions[i].transform.localPosition.x) * steerAwayStrength;

          amtOfRaycastsHitting++;
          switch (raycastPositions[i].rcType)
          {
            case SteeringRaycastType.frontal:
              amtFrontalHit++;
              break;
          }

        }

      }

      averagedSteerAwayDirection = Mathf.Clamp(averagedSteerAwayDirection, -1, 1);
    }
    // If all frontal raycast hit,
    // We are hitting a wall!
    // reverse.
    /*
    if (amtFrontalHit == amtFrontalChecks)
    {
      float newTurn = turnAmtToDriveTarget < 0 ? -1 : 1;
      isReversing = true;
      averagedSteerAwayDirection = 0f;
      reverseAction(newTurn);
      return;
    }
    */


    if (amtOfRaycastsHitting == 0) averagedSteerAwayDirection = 0f;
    if (averagedSteerAwayDirection != 0f && isReversing == false) _turningInput = Mathf.Clamp(averagedSteerAwayDirection, -1, 1);
  }
  private void reverseAction(float newTurn)
  {
    // Setup
    _turningInput = newTurn * -1;
    averagedSteerAwayDirection = 0f;
    _throttleInput = -1;

    if (true)
    {
      float rayLen = maxEffectiveDistanceForSteering * 1.5f;
      // New steer pos var
      float distanceAway = 0f;
      // Remember to destroy to avoid memory leak during runtime
      raycastDirTr.SetPositionAndRotation(transform.position, transform.rotation);
      float ogYAngle = raycastDirTr.rotation.eulerAngles.y;

      Vector3 newRotEul = raycastDirTr.rotation.eulerAngles;

      for (int i = 0; i < 90; i++)
      {
        // Goes both directions
        int angleOffset = i % 2 == 0 ? i : -i;
        newRotEul.y = ogYAngle + angleOffset;
        raycastDirTr.rotation = Quaternion.Euler(newRotEul);
        bool hitCollider = Physics.Raycast(transform.position, raycastDirTr.forward, out RaycastHit rayHit, rayLen, steerAwayFromLayers);
        if (GameStateManager.Instance.UseDebug)
        {
          Debug.DrawRay(transform.position, raycastDirTr.forward * rayLen, hitCollider ? Color.red : Color.green);
        }

        if (hitCollider == true)
        {
          distanceAway = Vector3.Distance(transform.position, rayHit.point);
        }
        else
        {
          steeringOffwall = true;
          foundOffwallPos = true;
          previousSteeringPos = _steeringPosition;

          Vector3 newSteerPos = (raycastDirTr.forward.normalized * distanceAway) + transform.position;
          _steeringPosition = newSteerPos;
          break;
        }

      }

    }
  }
  private float steerVehicleToDestination()
  {

    _throttleInput = 0f;
    _turningInput = 0f;
    yAngleToTarget = 0f;

    yAngleToTarget = Vector3.SignedAngle(transform.forward, aggregatedDirectionFromNodeCloud, Vector3.up);

    _turningInput = yAngleToTarget > 0 ? -1 : 1;
    if (yAngleToTarget == 0) _turningInput = 0f;

    _throttleInput = Mathf.Clamp(aggregatedDirectionFromNodeCloud.z, -1f, 1f);

    return _turningInput;
  }


  #endregion


  // Misc
  private void OnDrawGizmos()
  {
    Gizmos.color = Color.magenta;
    Gizmos.DrawSphere(_steeringPosition, 1.5f);
  }
  private void OnDisable()
  {
    if (raycastDirTr != null) Destroy(raycastDirTr.gameObject);
  }
}
