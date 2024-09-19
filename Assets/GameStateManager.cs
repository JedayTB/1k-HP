using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [SerializeField] private PlayerVehicleController _player;
    [SerializeField] private LapChecker _lapChecker;
    [SerializeField] private waypointGizmos _waypointGizmos;

    [SerializeField] private VehicleAIController[] _aiControllers;
    private void Awake()
    {
        _lapChecker.Init();
        _player.Init();

        for(int i = 0; i < _aiControllers.Length; i++)
        {
            _aiControllers[i].Init(_waypointGizmos);
        }

    }
}
