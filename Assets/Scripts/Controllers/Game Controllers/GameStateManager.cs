using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
  // Every 0.25 seconds, calculate Race placements
  // RECALCULATE RACE PLACEMENTS AT END OF RACE
  private static readonly float RACEPLACEMENTSTICK = 0.1f;
  private PlayerVehicleController _player;
  public static GameStateManager Instance { get { return instance; } }

  [SerializeField] private CameraFollower3D cam;
  [SerializeField] public LapChecker _lapChecker;
  [SerializeField] private LapTimer _lapTimer;
  [SerializeField] private waypointGizmos[] NavigationTracks;
  [SerializeField] private UIController _uiController;
  [SerializeField] private VehicleAIController[] _aiControllers;
  [SerializeField] private GameObject[] _playerVehicles;
  [SerializeField] private Transform[] _startLocations;
  [SerializeField] private PostProcessing _postProcessing;


  public bool UseDebug = true;
  public int nextPlayerCheckpointPosition = 0;
  private static GameStateManager instance;

  public A_Ability[] Abilitieslist;

  private InputManager inputManager;

  public static PlayerVehicleController Player; // Singleton var

  public static int _newCharacter = 2;

  // UI Stuff
  [SerializeField] private TextMeshProUGUI _lapTimesText;

  private List<A_VehicleController> vehicles = new List<A_VehicleController>();
  public Vector3[] levelCheckpointLocations;


  private void Awake()
  {
    instance = this;
    var tempPlayer = Instantiate(_playerVehicles[_newCharacter], _startLocations[0].transform.position, _startLocations[0].transform.rotation);

    _player = tempPlayer.GetComponent<PlayerVehicleController>();
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


    //VehicleAIController[] ais = FindObjectsByType<VehicleAIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    for (int i = 0; i < _aiControllers.Length; i++)
    {
      if (_aiControllers[i].enabled)
      {
        if (NavigationTracks.Length > 0)
        {
          _aiControllers[i].Init(NavigationTracks);
        }
        else
        {
          _aiControllers[i].Init();
        }
        vehicles.Add(_aiControllers[i]);
        vehiclesToPosition++;
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

    for (int i = 0; i < vehiclesToPosition; i++)
    {
      vehicles[i].transform.SetPositionAndRotation(_startLocations[i].position, _startLocations[i].rotation);
      vehicles[i].setNewRespawnPosition(_startLocations[i]);
    }

    StartCoroutine(calculateVehiclePlacements());
    Debug.Log("GSM has Finished Intializing!");
  }

  IEnumerator calculateVehiclePlacements()
  {
    // Starting logic
    Dictionary<float, A_VehicleController> distPlayerDict = new();
    float[] vehicleRaceProgressionCalc = new float[vehicles.Count];

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

        print($"i {i}, val {progressions[i]}");
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
    nextPlayerCheckpointPosition = index;
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
}
