using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    private InputManager inputManager;
    [SerializeField] private PlayerVehicleController _player;
    public static PlayerVehicleController Player; // Singleton var
    [SerializeField] private LapChecker _lapChecker;
    [SerializeField] private LapTimer _lapTimer;
    [SerializeField] private waypointGizmos _waypointGizmosMiddle;
    [SerializeField] private waypointGizmos _waypointGizmosOptimal;
    [SerializeField] private waypointGizmos _waypointGizmosWide;
    [SerializeField] private UIController _uiController;
    [SerializeField] private VehicleAIController[] _aiControllers;

    // UI Stuff
    [SerializeField] private TextMeshProUGUI _lapTimesText;
    
    private void Awake()
    {
        Player = _player;
        inputManager = this.gameObject.AddComponent<InputManager>();
        inputManager.Init();
        
        _lapChecker?.Init(this);

        _player?.Init(inputManager);

        _uiController.init(_player);

        for (int i = 0; i < _aiControllers.Length; i++)
        {
            _aiControllers[i]?.Init(_waypointGizmosMiddle, _waypointGizmosOptimal, _waypointGizmosWide);
        }
        Debug.Log("Finished Intializing!");
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
            lapTimesStr += $"Lap {count} Time: {lapTime}\n";
            count++;
        }

        _lapTimesText.text = lapTimesStr;

        _uiController.setWinScreen(true);

    }
}
