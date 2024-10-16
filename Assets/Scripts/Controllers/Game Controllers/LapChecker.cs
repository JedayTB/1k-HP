using UnityEngine;

public delegate void CheckFinishedLap();
public class LapChecker : MonoBehaviour
{
    [SerializeField] private lapCheckpoint[] _checkpoints;
    private LapTimer _lapTimer;

    private GameStateManager _gsm;
    public CheckFinishedLap checkFinishedLap;
    public int lapCount = 0;
    [SerializeField] private int lapsToWin = 3;

    
    public void Init(GameStateManager gsm)
    {
        _gsm = gsm;
        
        _checkpoints = GetComponentsInChildren<lapCheckpoint>();
        _lapTimer = FindObjectOfType<LapTimer>();
        checkFinishedLap = checkIfLapsPassed;

        for (int i = 0; i < _checkpoints.Length; i++)
        {
            _checkpoints[i].Init(i, checkFinishedLap);
        }

    }

    void checkIfLapsPassed()
    {
        foreach (var checkpoint in _checkpoints)
        {
            if (checkpoint.passedCheckpoint != true) return;
        }

        lapCount++;

        foreach (var checkpoint in _checkpoints)
        {
            checkpoint.passedCheckpoint = false;
            checkpoint.resetHashset();
        }
        _lapTimer.endLap();
        Debug.Log($"Finished lap: {lapCount}");
        if (lapCount >= lapsToWin)
        {
            _gsm.onPlayerWin();
        }
    }

    void OnDrawGizmos()
    {
        _checkpoints = GetComponentsInChildren<lapCheckpoint>();
        for (int i = 0; i < _checkpoints.Length; i++)
        {
            _checkpoints[i].name = $"Checkpoint {i + 1}";
        }
    }
}
