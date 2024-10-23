using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class GameStateManager : MonoBehaviour
{
    private InputManager inputManager;
    [SerializeField] public PlayerVehicleController player;
    [SerializeField] private LapChecker _lapChecker;
    [SerializeField] private LapTimer _lapTimer;
    [SerializeField] private waypointGizmos _waypointGizmos;

    [SerializeField] private VehicleAIController[] _aiControllers;

    // UI Stuff
    [SerializeField] private TextMeshProUGUI _lapTimesText;
    [SerializeField] private CanvasGroup _winScreenUI;
    [SerializeField] private CanvasGroup _playingHud;
    private void Awake()
    {
        inputManager = this.gameObject.AddComponent<InputManager>();
        inputManager.Init();
        
        _lapChecker?.Init(this);

        player?.Init(inputManager);

        for (int i = 0; i < _aiControllers.Length; i++)
        {
            _aiControllers[i]?.Init(_waypointGizmos);
        }

    }
    
    public void onPlayerWin()
    {
        Debug.Log("Player won!");

        _playingHud.gameObject.SetActive(false);

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

        _winScreenUI.gameObject.SetActive(true);

    }
}
