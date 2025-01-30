using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
  // Every 0.25 seconds, calculate Race placements
  // RECALCULATE RACE PLACEMENTS AT END OF RACE
  private static readonly float RACEPLACEMENTSTICK = 0.15f;

  [SerializeField] private PlayerVehicleController _player;
  public A_Ability[] Abilitieslist;

  public static GameStateManager Instance { get { return instance; } }

  public bool UseDebug = true;

  [
    Header("Game Logic Objects")]
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
  public Texture2D hookshotCursor;

  [Header("Misc")]
  public static readonly float musicVolumeLevel = 0.5f;

  public int nextPlayerCheckpointPosition = 0;
  private static GameStateManager instance;

  private InputManager inputManager;

  public static PlayerVehicleController Player; // Singleton var

  public static int _newCharacter = 2;

  // UI Stuff
  [SerializeField] private TextMeshProUGUI _lapTimesText;

  private List<A_VehicleController> vehicles = new List<A_VehicleController>();
  [HideInInspector] public Vector3[] levelCheckpointLocations;

  private void Awake()
  {
    instance = this;
    if (_player == null)
    {
      var tempPlayer = Instantiate(_playerVehicles[_newCharacter]);
      _player = tempPlayer.GetComponent<PlayerVehicleController>();
    }
    Player = _player;

    inputManager = this.gameObject.AddComponent<InputManager>();
    inputManager.Init();

    _lapChecker?.Init(this);
    if (_lapChecker != null) levelCheckpointLocations = _lapChecker.checkPointLocations;
    _uiController?.init(_player);
    cam = Camera.main.gameObject.GetComponentInParent<CameraFollower3D>();
    cam?.Init(inputManager);
    _postProcessing?.Init();

    // breaks at this one for some reason, had to move everything else up to stop them from not being called
    _player?.Init(inputManager);

    vehicles.Add(_player);

    int vehiclesToPosition = 1;

    _player.VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.FreezePosition;

    //VehicleAIController[] ais = FindObjectsByType<VehicleAIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
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

    StartCoroutine(calculateVehiclePlacements());

    _musicManager?.startMusic();
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

    while (true)
    {
      distPlayerDict.Clear();
      // Calculate Distances and progression
      for (int i = 0; i < vehicles.Count; i++)
      {
        float distToNextCheckpoint = Vector3.Distance(vehicles[i].transform.position, levelCheckpointLocations[vehicles[i].nextCheckpointIndex]);

        float distProgression = (distToNextCheckpoint / distancesBetweenCheckpoints[vehicles[i].nextCheckpointIndex]);

        float roundedProgression = (vehicles[i].nextCheckpointIndex - 1) * checkpointFraction;

        float particleProgression = distProgression * checkpointFraction;

        float progression = roundedProgression + particleProgression;

        distPlayerDict.Add(progression, vehicles[i]);
        progressions[i] = progression;
      }

      // Sort distances and input placements based on that
      //
      int zeroIndexLn = vehicles.Count - 1;
      SortingAlgorithms.QuickSort(progressions, 0, zeroIndexLn);
      // ^ sorts lowest to highest, but 1st place should be highest progression number
      // Traverse array backwards

      int count = 0;
      for (int i = progressions.Length - 1; i >= 0; i--)
      {
        count++;

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
      pl.nextCheckpointIndex++;
    }
    else
    {
      // Don't think this matters... But Set it in case!
      vehicle.nextCheckpointIndex++;
    }
  }
  public void onPlayerWin()
  {
    Debug.Log("Player won!");

    _uiController.setPlayScreen(false);

    List<float> lapTimes = _lapTimer.lapTimes;
    string lapTimesStr = "";

    //Because List's don't have a defined size?
    int count = 0;

    foreach (float lapTime in lapTimes)
    {
      lapTimesStr += $"Lap {count + 1} Time: {lapTime}\n";
      count++;
    }

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
}
