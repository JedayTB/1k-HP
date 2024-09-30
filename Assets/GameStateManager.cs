using UnityEngine;


public class GameStateManager : MonoBehaviour
{
    [SerializeField] public PlayerVehicleController _player;
    [SerializeField] private LapChecker _lapChecker;
    [SerializeField] private waypointGizmos _waypointGizmos;

    [SerializeField] private VehicleAIController[] _aiControllers;

    // UI Stuff

    [SerializeField] private CanvasGroup _winScreenUI;
    private void Awake()
    {
        _lapChecker?.Init(this);
        _player?.Init();

        for (int i = 0; i < _aiControllers.Length; i++)
        {
            _aiControllers[i]?.Init(_waypointGizmos);
        }

    }

    public void onPlayerWin()
    {
        Debug.Log("Player won!");
        _winScreenUI.gameObject.SetActive(true);
    }
}
