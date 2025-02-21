using System.Collections.Generic;
using System.Collections;
using UnityEngine.VFX;
using TMPro;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
  // Every 0.25 seconds, calculate Race placements
  // RECALCULATE RACE PLACEMENTS AT END OF RACE
  private static readonly float RACEPLACEMENTSTICK = 0.15f;

  [SerializeField] private PlayerVehicleController _player;
  public A_Ability[] Abilitieslist;

  private static GameStateManager instance;
  public static GameStateManager Instance { get { return instance; } }

  public bool UseDebug = true;
  [Header("Game Logic Values")]
  public int LapsToFinishRace = 3;
  public static float countdownTime = 3;

  [Header("Game Logic Objects")]
  [SerializeField] private CameraFollower3D cam;
  [SerializeField] public LapChecker _lapChecker;
  [SerializeField] private LapTimer _lapTimer;
  [SerializeField] private waypointGizmos[] NavigationTracks;
  [SerializeField] public UIController _uiController;
  [SerializeField] private VehicleAIController[] _aiControllers;
  [SerializeField] private GameObject[] _playerVehicles;
  [SerializeField] private Transform[] _startLocations;
  [SerializeField] private PostProcessing _postProcessing;
  [SerializeField] private MusicManager _musicManager;

  [Header("Cursor Sprites")]
  public Texture2D lightningCursor;

  [Header("Race Placements and debug")]
  [SerializeField] string[] vehicleDbgInfo;
  public int nextPlayerCheckpointPosition = 0;


  [Header("Misc")]
  [SerializeField] private VisualEffect SkidParticlesPrototype;
  private static readonly string skidParticlesVelocityID = "Velocity";
  public LayerMask CarPhysicsLayer;
  public static readonly float musicVolumeLevel = 0.5f;

  private InputManager inputManager;

  public static PlayerVehicleController Player; // Singleton var

  public static int _newCharacter = 0;


  // UI Stuff
  [SerializeField] private TextMeshProUGUI _lapTimesText;

  private List<A_VehicleController> vehicles = new List<A_VehicleController>();
  [HideInInspector] public Vector3[] levelCheckpointLocations;

  private void Awake()
  {
    instance = this;
    if (_player == null)
    {
      _player = Instantiate(_playerVehicles[_newCharacter]).GetComponent<PlayerVehicleController>();
    }
    Player = _player;

    inputManager = this.gameObject.AddComponent<InputManager>();
    inputManager.Init();

    _lapChecker?.Init(this);
    if (_lapChecker != null) levelCheckpointLocations = _lapChecker.checkPointLocations;
    _uiController?.init(_player);
    cam = Camera.main.gameObject.GetComponentInParent<CameraFollower3D>();
    cam?.Init(inputManager);

    // breaks at this one for some reason, had to move everything else up to stop them from not being called
    _player?.Init(inputManager);

    vehicles.Add(_player);

    int vehiclesToPosition = 1;

    _player.VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.FreezePosition;

    if (_aiControllers.Length == 0)
    {
      VehicleAIController[] ais = FindObjectsByType<VehicleAIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }
    for (int i = 0; i < _aiControllers.Length; i++)
    {
      if (_aiControllers[i].gameObject.activeSelf)
      {
        if (NavigationTracks.Length > 0)
        {
          _aiControllers[i].Init(NavigationTracks);
        }
        vehicles.Add(_aiControllers[i]);
        vehiclesToPosition++;
        _aiControllers[i].VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.FreezePosition;
      }
    }

    // Set checkpoints passed through.
    int checkpointsArrLength = levelCheckpointLocations.Length;
    for (int i = 0; i < vehicles.Count; i++)
    {
      vehicles[i].checkpointsPassedThrough = new bool[checkpointsArrLength];
      // Set all to false
      for (int k = 0; k < checkpointsArrLength; k++)
      {
        vehicles[i].checkpointsPassedThrough[k] = false;
      }
    }

    // Set Vehicles position
    for (int i = 0; i < vehiclesToPosition; i++)
    {
      vehicles[i].transform.SetPositionAndRotation(_startLocations[i].position, _startLocations[i].rotation);
      vehicles[i].setNewRespawnPosition(_startLocations[i]);
    }
    _postProcessing?.init();
    StartCoroutine(calculateVehiclePlacements());
    Debug.Log("GSM has Finished Intializing!");
  }

  IEnumerator calculateVehiclePlacements()
  {
    // Starting logic
    Dictionary<float, A_VehicleController> distPlayerDict = new();

    // Last index is distance of nth - 0th index
    float[] distancesBetweenCheckpoints = new float[levelCheckpointLocations.Length];
    // Calculate distanes between checkpoints
    for (int i = 0; i < levelCheckpointLocations.Length; i++)
    {
      int nextIndex = i + 1;
      if (nextIndex == levelCheckpointLocations.Length) nextIndex = 0;
      distancesBetweenCheckpoints[i] = Vector3.Distance(levelCheckpointLocations[i], levelCheckpointLocations[nextIndex]);
    }

    float checkpointFraction = 1f / levelCheckpointLocations.Length;
    float[] progressions = new float[vehicles.Count];
    vehicleDbgInfo = new string[vehicles.Count];

    while (true)
    {
      distPlayerDict.Clear();
      // Calculate Distances and progression
      for (int i = 0; i < vehicles.Count; i++)
      {

        float distToNextCheckpoint = Vector3.Distance(vehicles[i].transform.position, levelCheckpointLocations[vehicles[i].nextCheckpointIndex]);

        float distProgression = Mathf.Clamp01(1 - (distToNextCheckpoint / distancesBetweenCheckpoints[vehicles[i].nextCheckpointIndex]));

        float roundedProgression = (vehicles[i].nextCheckpointIndex - 1) * checkpointFraction;

        float particleProgression = distProgression * checkpointFraction;

        int amtOfLapsPassed = vehicles[i].lapsPassed;

        float progression = (roundedProgression + particleProgression) + (float)amtOfLapsPassed;


        if (UseDebug)
        {
          Vector3 dir = levelCheckpointLocations[vehicles[i].nextCheckpointIndex] - vehicles[i].transform.position;
          Debug.DrawRay(vehicles[i].transform.position, dir, Color.white, RACEPLACEMENTSTICK);
          vehicleDbgInfo[i] = $"{vehicles[i].name}: nxt chckpnt ind {vehicles[i].nextCheckpointIndex}: Place {vehicles[i].racePlacement} Laps {amtOfLapsPassed}\n"
            + $"prog {progression}: round prog {roundedProgression}: part prog {particleProgression}: dist prog {distProgression}:\n"
            + $"Next chkpnt dist {distToNextCheckpoint}";
        }

        distPlayerDict.Add(progression, vehicles[i]);
        progressions[i] = progression;
      }

      // Sort distances and input placements based on that
      //
      int zeroIndexLn = vehicles.Count - 1;
      SortingAlgorithms.QuickSort(progressions, 0, zeroIndexLn);

      int count;
      for (int i = 0; i < progressions.Length; i++)
      {
        count = zeroIndexLn - i + 1;

        A_VehicleController vRef = distPlayerDict[progressions[i]];

        vRef.racePlacement = count;
      }

      yield return new WaitForSeconds(RACEPLACEMENTSTICK);
    }

  }
  private void Update()
  {
    //Debug
    if (Input.GetKeyDown(KeyCode.L))
    {
      onPlayerWin();
    }
  }
  public void setVehicleNextCheckpoint(A_VehicleController vehicle, int index)
  {

    var pl = vehicle.GetComponent<PlayerVehicleController>();
    if (pl != null)
    {
      nextPlayerCheckpointPosition = index;
      pl.nextCheckpointIndex = index;
    }
    else
    {
      // Don't think this matters... But Set it in case!
      vehicle.nextCheckpointIndex = index;
    }
  }
  public void setVehicleLapCount(A_VehicleController vehicle)
  {
    vehicle.lapsPassed++;
    for (int i = 0; i < vehicle.checkpointsPassedThrough.Length; i++)
    {
      vehicle.checkpointsPassedThrough[i] = false;
    }
  }
  public void onPlayerWin()
  {
    Debug.Log("Player won!");

    _uiController.setPlayScreen(false);

    List<string> lapTimes = _lapTimer.lapTimes;
    string lapTimesStr = "";

    //Because List's don't have a defined size?
    int count = 0;

    foreach (var lapTime in lapTimes)
    {
      lapTimesStr += $"Lap {count + 1} Time: {lapTime}\n";
      count++;
    }

    print(lapTimesStr);

    _lapTimesText.text = lapTimesStr;

    bool isGP = GrandPrixManager.GameMode == 0 ? true : false;
    GrandPrixManager.SetRacePlacement(GrandPrixManager.CurrentLevelIndex, 1);
    GrandPrixManager.CurrentLevelIndex += isGP ? 1 : 0;
    _uiController.setWinScreen(true, isGP);

  }

  public void UnfreezeAIs()
  {
    foreach (VehicleAIController ai in _aiControllers)
    {
      ai.VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.None;
      // Just to reset velocity calculations
      ai.VehiclePhysics.RigidBody.velocity = Vector3.zero;
      ai.VehiclePhysics.setInputs(0, 0);

    }
  }
  public void spawnSkidParticles(Vector3 positionOfHit, Vector3 orientation, float velocityOfHit)
  {
    var skidparticles = Instantiate(SkidParticlesPrototype);

    Quaternion newRot = Quaternion.Euler(orientation);
    skidparticles.transform.SetPositionAndRotation(positionOfHit, newRot);

    skidparticles.SetFloat(skidParticlesVelocityID, velocityOfHit);
  }
}
