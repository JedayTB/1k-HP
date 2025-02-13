using System.Collections;
using UnityEngine;


public class VehicleAIController : A_VehicleController
{

  #region AI Variables 
  [Header("AI Basic setup")]
  [HideInInspector] public aiState currentState = aiState.driving;
  public string dbgString;
  private Vector3 _steeringPosition = Vector3.zero;
  [SerializeField] protected float SteerPathingClock = 0.01f;
  [SerializeField] private bool _singleTarget = false;
  [SerializeField] private bool _driveVehicle = true;
  [SerializeField] private bool _circuitedpath = true;

  [Header("Steering parametres")]
  private waypointGizmos[] _NavigationTracks;
  private int waypointsArrayLength;
  [SerializeField] private float _reachedTargetDistance = 6f;
  [SerializeField] private float _reverseThreshold = 25f;
  [SerializeField] private float _turningThreshold = 15;

  private int _currentWaypointIndex = 0;
  private int _respawnWaypointIndex = 0;

  [Header("Advanced Steering Parametres")]
  public float averagedSteerAwayDirection;

  Transform raycastDirTr;
  private Vector3 previousSteeringPos = Vector3.zero;

  [SerializeField] private bool isReversing = false;
  [SerializeField] private bool steeringOffwall = false;
  [SerializeField] private bool foundOffwallPos = false;

  [SerializeField] private float reachedSteerOffWallDistance = 1f;
  [SerializeField] private SteeringRaycast[] raycastPositions;

  [Header("Raycast specifics")]

  private float rayHitStrength;

  [SerializeField] private float maxEffectiveDistanceForSteering = 25f;
  [SerializeField] private float frontalRCDivisor = 0.5f;
  [SerializeField] private LayerMask steerAwayFromLayers;

  private int amtFrontalChecks = 0;
  private float yAngleToTarget;

  /*
  [Header("Steering Weights")]
  [SerializeField] private float _middleTrackWeight = 50f;
  [SerializeField] private float _optimalTrackWeight = 25f;
  [SerializeField] private float _wideTrackWeight = 35f;

  private float[] weights;
  */

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

  public void Init(waypointGizmos[] tracks)
  {
    _NavigationTracks = new waypointGizmos[tracks.Length];

    for (int i = 0; i < tracks.Length; i++)
    {
      _NavigationTracks[i] = tracks[i];
    }

    waypointsArrayLength = _NavigationTracks[0].getWaypoints().Length - 1;

    if (_NavigationTracks.Length != 0)
    {
      _steeringPosition = _NavigationTracks[0].getWaypoints()[0].position;

    }

    Init();
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
    _currentWaypointIndex = _respawnWaypointIndex;
  }

  public override void setNewRespawnPosition(Transform newTransform)
  {
    base.setNewRespawnPosition(newTransform);
    _respawnWaypointIndex = _currentWaypointIndex;
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
    useCharacterAbility();
    currentState = aiState.LightningAbility;
    Debug.Log($"{this.name} has or tried to use Ability (Lightning)");
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
  }
  #endregion
  #region General Driving Methods
  private void generalDrivingLogic()
  {
    if (_singleTarget == false) updateTarget();

    if (_driveVehicle)
    {
      float turnAmtToDriveTarget = steerVehicleToDestination();
      avoidCollisions(turnAmtToDriveTarget);
      _vehiclePhysics.setInputs(_throttleInput, _turningInput);
    }
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
      bool hitACollider = raycastPositions[i].raycastForward(out RaycastHit hit, dist, steerAwayFromLayers);

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
    if (amtFrontalHit == amtFrontalChecks)
    {
      float newTurn = turnAmtToDriveTarget < 0 ? -1 : 1;
      isReversing = true;
      averagedSteerAwayDirection = 0f;
      reverseAction(newTurn);

      return;
    }
    else
    {
      isReversing = false;
    }

    if (amtOfRaycastsHitting == 0) averagedSteerAwayDirection = 0f;
    if (averagedSteerAwayDirection != 0 && isReversing == false) _turningInput = Mathf.Clamp(averagedSteerAwayDirection, -1, 1);
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

    float distanceToTarget = Vector3.Distance(transform.position, _steeringPosition);

    if (distanceToTarget > _reachedTargetDistance)
    {
      //Keep driving

      Vector3 dirToTarget = (_steeringPosition - transform.position).normalized;
      if (GameStateManager.Instance.UseDebug) Debug.DrawRay(transform.position, dirToTarget * distanceToTarget, Color.cyan);

      //Calculates wether target is positive on local Z axis or negative
      //Negative value is behind, pos is infront
      float frontBackCheck = Vector3.Dot(transform.forward, dirToTarget);

      if (frontBackCheck > 0)
      {
        _throttleInput = 1f;
      }
      else
      {
        //Go forward if the distance is further then reverse 
        _throttleInput = distanceToTarget > _reverseThreshold ? 1 : -1;
      }


      yAngleToTarget = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);

      // Set _turningInput Values below.

      // Scrapped magic number dw about it (does nothing)
      _turningInput = calculateTurnAmount(yAngleToTarget, 25f);
    }
    return _turningInput;
  }
  private void updateTarget()
  {
    float distanceToTarget = Vector3.Distance(transform.position, _steeringPosition);

    /*
    Vector3 dirToTarget = (_steeringPosition - transform.position).normalized;
    float frontBackCheck = Vector3.Dot(transform.forward, dirToTarget);

    if (frontBackCheck < 0 && isReversing == false)
    {
      updateTrackOption();
      updateWaypointIndex();
      _steeringPosition = _NavigationTracks[_currentTrackOption].getWaypoints()[_currentWaypointIndex].position;
      return;
    }
    */

    //Reached target

    if (steeringOffwall == true && distanceToTarget < reachedSteerOffWallDistance)
    {
      _steeringPosition = previousSteeringPos;
      steeringOffwall = false;
      foundOffwallPos = false;
    }
    else if (steeringOffwall == false && distanceToTarget < _reachedTargetDistance)
    {
      // Reached target after reverse acctions
      updateTrackOption();
      updateWaypointIndex();
      _steeringPosition = _NavigationTracks[_currentTrackOption].getWaypoints()[_currentWaypointIndex].position;
    }

  }
  private void updateWaypointIndex()
  {
    //  If got to the end of a path
    //  Without a circuit
    if ((_currentWaypointIndex + 1) == waypointsArrayLength && _circuitedpath == false)
    {
      _driveVehicle = false;
      _steeringPosition = transform.position;
      return;
    }

    //  If got to the end of a circuited path
    if (_currentWaypointIndex == waypointsArrayLength && _circuitedpath == true)
    {
      _currentWaypointIndex = 0;
    }
    //  No condition. Just go to the next waypoint
    else
    {
      _currentWaypointIndex++;
      _currentWaypointIndex = Mathf.Clamp(_currentWaypointIndex, 0, waypointsArrayLength);
    }
  }
  private void updateTrackOption()
  {
    int shortestAngleInd = int.MaxValue;
    float smallestYAngleDiff = float.MaxValue;

    for (int i = 0; i < _NavigationTracks.Length - 1; i++)
    {

      Vector3 waypointPosition = _NavigationTracks[i].getWaypoints()[_currentWaypointIndex].position;
      float yAngleToTarget = Vector3.SignedAngle(transform.forward, waypointPosition, Vector3.up);

      if (yAngleToTarget < smallestYAngleDiff)
      {
        shortestAngleInd = i;
        smallestYAngleDiff = yAngleToTarget;
      }
    }
    _currentTrackOption = shortestAngleInd;
  }

  private float calculateTurnAmount(float angleToTarget, float threshold)
  {
    //float turningAmount = Mathf.Clamp(angleToTarget / threshold, -1, 1);
    return angleToTarget > 0 ? 1 : -1;
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
