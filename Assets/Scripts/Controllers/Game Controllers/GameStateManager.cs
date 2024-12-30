using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
  public bool UseDebug = true;
  public int nextPlayerCheckpointPosition = 0;
  private static GameStateManager instance;

  public static GameStateManager Instance { get { return instance; } }
  private InputManager inputManager;
  private PlayerVehicleController _player;
  public static PlayerVehicleController Player; // Singleton var

  [SerializeField] private CameraFollower3D cam;
  [SerializeField] public LapChecker _lapChecker;
  [SerializeField] private LapTimer _lapTimer;
  [SerializeField] private waypointGizmos[] NavigationTracks;
  [SerializeField] private UIController _uiController;
  [SerializeField] private VehicleAIController[] _aiControllers;
  [SerializeField] private GameObject[] _playerVehicles;
  [SerializeField] private Transform[] _startLocations;
  [SerializeField] private PostProcessing _postProcessing;
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
    levelCheckpointLocations = _lapChecker.checkPointLocations;
    _uiController?.init(_player);
    cam = Camera.main.gameObject.GetComponentInParent<CameraFollower3D>();
    cam?.Init(inputManager);
    _postProcessing?.Init();

    // breaks at this one for some reason, had to move everything else up to stop them from not being called
    _player?.Init(inputManager);

    vehicles.Add(_player);

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

    // Only doing one, MP Server will handle multiple players
    int vehiclesToPosition = 1 + _aiControllers.Length;

    for (int i = 0; i < vehiclesToPosition; i++)
    {
      vehicles[i].transform.SetPositionAndRotation(_startLocations[i].position, _startLocations[i].rotation);
      vehicles[i].setNewRespawnPosition(_startLocations[i]);
    }


    Debug.Log("GSM has Finished Intializing!");
  }

  private void Update()
  {
    if (Input.GetKeyDown(KeyCode.L))
    {
      onPlayerWin();
    }
  }
  public void setNextPlayerCheckpoint(int index)
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
