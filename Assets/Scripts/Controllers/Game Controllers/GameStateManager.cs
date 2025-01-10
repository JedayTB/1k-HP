using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
  // Every 0.25 seconds, calculate Race placements
  // RECALCULATE RACE PLACEMENTS AT END OF RACE
  private static readonly float RACEPLACEMENTSTICK = 0.25f;
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

  private Dictionary<float, A_VehicleController> distPlayerDict = new();

  private void Awake()
  {
    instance = this;
    var tempPlayer = Instantiate(_playerVehicles[_newCharacter], _startLocations[0].transform.position, _startLocations[0].transform.rotation);

    _player = tempPlayer.GetComponent<PlayerVehicleController>();
    Player = _player;

    inputManager = this.gameObject.AddComponent<InputManager>();
    inputManager.Init();

    _lapChecker?.Init(this);
    if (_lapChecker) levelCheckpointLocations = _lapChecker.checkPointLocations;
    _uiController?.init(_player);
    cam = Camera.main.gameObject.GetComponentInParent<CameraFollower3D>();
    cam?.Init(inputManager);
    _postProcessing?.Init();

    // breaks at this one for some reason, had to move everything else up to stop them from not being called
    _player?.Init(inputManager);

    vehicles.Add(_player);

    //VehicleAIController[] ais = FindObjectsByType<VehicleAIController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    for (int i = 0; i < _aiControllers.Length; i++)
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
    }
    // Set checkpoints passed through.

    for (int i = 0; i < levelCheckpointLocations.Length; i++)
    {
      vehicles[i].checkpointsPassedThrough = new bool[levelCheckpointLocations.Length];
      // Set all to false
      for (int k = 0; i < levelCheckpointLocations.Length; i++)
      {
        vehicles[i].checkpointsPassedThrough[k] = false;
      }
    }

    // Only doing one, MP Server will handle multiple players
    int vehiclesToPosition = 1 + _aiControllers.Length;

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
    while (true)
    {
      distPlayerDict.Clear();
      float[] distances = new float[vehicles.Count];
      for (int i = 0; i < vehicles.Count; i++)
      {
        float distToNextCheckpoint = Vector3.Distance(vehicles[i].transform.position, levelCheckpointLocations[vehicles[i].nextCheckpointIndex]);
        distances[i] = distToNextCheckpoint;
        distPlayerDict.Add(distToNextCheckpoint, vehicles[i]);
      }
      //distances.sort();
      for (int i = 0; i < vehicles.Count; i++)
      {
        A_VehicleController v = distPlayerDict[distances[i]];
        v.racePlacement = i;
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
