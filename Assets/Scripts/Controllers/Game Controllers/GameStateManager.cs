using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    private InputManager inputManager;
    private PlayerVehicleController _player;
    public static PlayerVehicleController Player; // Singleton var
    [SerializeField] private CameraFollower3D cam;
    [SerializeField] private LapChecker _lapChecker;
    [SerializeField] private LapTimer _lapTimer;
    [SerializeField] private waypointGizmos[] NavigationTracks;
    [SerializeField] private UIController _uiController;
    [SerializeField] private VehicleAIController[] _aiControllers;
    [SerializeField] private GameObject[] _playerVehicles;
    [SerializeField] private Transform _startLocation;
    [SerializeField] private PostProcessing _postProcessing;
    public static int _newCharacter = 2;

    // UI Stuff
    [SerializeField] private TextMeshProUGUI _lapTimesText;

    public bool HasThreeTracks = false;

    private void Awake()
    {
        var tempPlayer = Instantiate(_playerVehicles[_newCharacter], _startLocation.transform.position, _startLocation.transform.rotation);

        _player = tempPlayer.GetComponent<PlayerVehicleController>();
        Player = _player;

        inputManager = this.gameObject.AddComponent<InputManager>();
        inputManager.Init();

        _lapChecker?.Init(this);
        _uiController?.init(_player);
        cam = Camera.main.gameObject.GetComponentInParent<CameraFollower3D>();
        cam?.Init(inputManager);
        _postProcessing?.Init();

        // breaks at this one for some reason, had to move everything else up to stop them from not being called
        _player?.Init(inputManager);

        for (int i = 0; i < _aiControllers.Length; i++)
        {
            if (HasThreeTracks) {
                _aiControllers[i]?.Init(NavigationTracks);
            }
            else
            {
                _aiControllers[i]?.Init();
            }
            
        }
        Debug.Log("GSM has Finished Intializing! - No Issues! (hopefully)");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            onPlayerWin();
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
}
