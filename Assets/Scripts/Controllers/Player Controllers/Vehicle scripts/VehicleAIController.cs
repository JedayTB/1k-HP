using System.Collections;
using UnityEngine;

/// Ideas
///
/// Make a direction from current steer node to next after to aid orientiation of car
public class VehicleAIController : A_VehicleController
{

  #region AI Variables 
  [Header("AI Basic setup")]
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
  public bool useAvoidCollisions = true;
  public float averagedSteerAwayDirection;
  private bool isReversing = false;
  [SerializeField] private float _angleThresholdOfDrift = 25f;
  [SerializeField] private Transform[] raycastPositions;
  //public float averagedSteerAwayDirection;

  [Header("Raycast specifics")]

  [SerializeField] private float rayHitStrength;
  [SerializeField] private float maxEffectiveDistanceForSteering = 25f;
  [SerializeField] private LayerMask steerAwayFromLayers;



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
    transform.position += new Vector3(1, 1, 1);

    rayHitStrength = 1f / raycastPositions.Length;
    StartCoroutine(SteerPathing(SteerPathingClock));
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
  IEnumerator SteerPathing(float waitTime)
  {

    while (true)
    {
      if (_singleTarget == false) updateTarget();

      if (_driveVehicle)
      {
        float turnAmtToDriveTarget = steerVehicleToDestination();
        if (useAvoidCollisions) avoidCollisions(turnAmtToDriveTarget);
        _vehiclePhysics.setInputs(_throttleInput, _turningInput);
      }
      yield return new WaitForSeconds(waitTime);
    }


  }
  private void avoidCollisions(float turnAmtToDriveTarget)
  {
    Transform tempTransform;
    int amtOfRaycastsHitting = 0;
    averagedSteerAwayDirection = 0f;

    for (int i = 0; i < raycastPositions.Length; i++)
    {
      tempTransform = raycastPositions[i];
      bool hitACollider = Physics.Raycast(tempTransform.position, tempTransform.forward, out RaycastHit hit, maxEffectiveDistanceForSteering, steerAwayFromLayers);
      if (GameStateManager.Instance.UseDebug) Debug.DrawRay(tempTransform.position, tempTransform.forward * maxEffectiveDistanceForSteering, hitACollider ? Color.red : Color.green);
      //Find out how far left / right it is from origin
      if (hitACollider)
      {
        float hitDist = Vector3.Distance(transform.position, hit.point);

        if (hitDist < maxEffectiveDistanceForSteering)
        {
          float steerAwayStrength = 1 - (hitDist / maxEffectiveDistanceForSteering);

          averagedSteerAwayDirection += rayHitStrength * -Mathf.Sign(raycastPositions[i].localPosition.x) * steerAwayStrength;
          amtOfRaycastsHitting++;
        }

      }

      averagedSteerAwayDirection = Mathf.Clamp(averagedSteerAwayDirection, -1, 1);
    }
    // This is if all raycasts are hitting 
    // 0.66 so it only reverses if over a third of the
    // raycast's are hitting

    if ((float)amtOfRaycastsHitting >= (float)(raycastPositions.Length * 0.85f) && VehiclePhysics.RigidBody.velocity.magnitude < 10f)
    {
      float newTurn = turnAmtToDriveTarget < 0 ? -1 : 1;
      _turningInput = newTurn * -1;
      averagedSteerAwayDirection = 0f;
      _throttleInput = -1;
      isReversing = true;
    }
    else
    {
      isReversing = false;
    }

    if (amtOfRaycastsHitting == 0) averagedSteerAwayDirection = 0f;
    if (averagedSteerAwayDirection != 0) _turningInput = Mathf.Clamp(_turningInput + averagedSteerAwayDirection, -1, 1);
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
      if (GameStateManager.Instance.UseDebug) Debug.DrawRay(transform.position, dirToTarget * distanceToTarget, Color.white);

      //Calculates wether target is positive on local Z axis or negative
      //Negative value is behind, pos is infront
      float frontBackCheck = Vector3.Dot(transform.forward, dirToTarget);

      if (frontBackCheck > 0)
      {
        _throttleInput = 1f;
      }
      else
      {
        //Go forward if the distance is further then reverse threshold
        _throttleInput = distanceToTarget > _reverseThreshold ? 1 : -1;
      }


      yAngleToTarget = Vector3.SignedAngle(transform.forward, dirToTarget, Vector3.up);

      // Set _turningInput Values below.

      if (Mathf.Abs(yAngleToTarget) > _angleThresholdOfDrift)
      {

        //_vehiclePhysics.driftVehicle(true);
        //Find a way to make the AI's counter steer
        // Might as well be fucking impossible.
        _turningInput = calculateTurnAmount(yAngleToTarget, _angleThresholdOfDrift);
      }
      else
      {
        //_vehiclePhysics.endedDrifting(true);
        _turningInput = calculateTurnAmount(yAngleToTarget, _turningThreshold);
      }

    }
    return _turningInput;
  }
  private void updateTarget()
  {
    float distanceToTarget = Vector3.Distance(transform.position, _steeringPosition);


    Vector3 dirToTarget = (_steeringPosition - transform.position).normalized;
    float frontBackCheck = Vector3.Dot(transform.forward, dirToTarget);

    if (frontBackCheck < 0 && isReversing == false)
    {
      updateTrackOption();
      updateWaypointIndex();
      _steeringPosition = _NavigationTracks[_currentTrackOption].getWaypoints()[_currentWaypointIndex].position;
      return;
    }

    //Reached target
    if (distanceToTarget < _reachedTargetDistance)
    {
      updateTrackOption();
      updateWaypointIndex();
      _steeringPosition = _NavigationTracks[_currentTrackOption].getWaypoints()[_currentWaypointIndex].position;
    }

  }
  private void updateWaypointIndex()
  {
    /*
    //Find closest waypoint on track
    float lowestDistance = float.MaxValue;
    int setNextInd = -1;
    // Do 10 iterations and pick closest waypoint
    int negIndCount = _currentWaypointIndex - 1;
    int indCount = _currentWaypointIndex + 1;
    int count = 0;

    var track = _NavigationTracks[_currentTrackOption].getWaypoints();
    int trackLn = track.Length;

    Vector3 waypointPos;
    while (count < 10)
    {
      count++;
      int useIndex;
      // Clamp to avoid out of bounds nonsense
      negIndCount = Mathf.Clamp(negIndCount, 0, trackLn);

      indCount = Mathf.Clamp(indCount, 0, trackLn);
      // Even
      if (count % 2 == 0)
      {
        waypointPos = track[indCount].position;
        useIndex = indCount;
        indCount++;
      }
      //Odd
      else
      {
        waypointPos = track[negIndCount].position;
        useIndex = negIndCount;
        negIndCount--;
      }
      float dist = Vector3.Distance(transform.position, waypointPos);

      if (dist < lowestDistance)
      {
        lowestDistance = dist;
        setNextInd = useIndex;
      }
    }
    _currentWaypointIndex = setNextInd;
    */
    //  If got to the end of a path
    //  Without a circuit
    if ((_currentWaypointIndex + 1) == waypointsArrayLength && _circuitedpath == false)
    {
      _driveVehicle = false;
      _steeringPosition = transform.position;
      return;
    }
    //

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
    float turningAmount = Mathf.Clamp(angleToTarget / threshold, -1, 1);
    return turningAmount;
  }

}
