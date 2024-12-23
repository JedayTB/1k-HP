using UnityEngine;

public delegate void CheckFinishedLap();
public class LapChecker : MonoBehaviour
{
  [SerializeField] private lapCheckpoint[] _checkpoints;

  [SerializeField] private int lapsToWin = 3;
  [SerializeField] private ParticleSystem[] raceFinishParticles;


  private LapTimer _lapTimer;

  private GameStateManager _gsm;
  public CheckFinishedLap checkFinishedLap;
  public int lapCount = 0;


  public void Init(GameStateManager gsm)
  {
    _gsm = gsm;

    _checkpoints = GetComponentsInChildren<lapCheckpoint>();
    print(_checkpoints.Length);
    _lapTimer = FindObjectOfType<LapTimer>();
    checkFinishedLap = checkIfLapsPassed;

    for (int i = 0; i < _checkpoints.Length; i++)
    {
      _checkpoints[i].Init(i, checkFinishedLap);
    }

  }

  void checkIfLapsPassed()
  {
    int nextPos = 0;

    for (int i = 0; i < _checkpoints.Length; i++)
    {
      nextPos = i;

      _gsm.setNextPlayerCheckpoint(nextPos++);
      if (_checkpoints[i].passedCheckpoint != true) return;
    }
    // If here is reached, lap was finished
    _gsm.setNextPlayerCheckpoint(0);
    onLapFinished();
  }
  void onLapFinished()
  {
    lapCount++;

    foreach (var checkpoint in _checkpoints)
    {
      checkpoint.passedCheckpoint = false;
      checkpoint.resetHashset();
    }
    _lapTimer?.endLap();
    Debug.Log($"Finished lap: {lapCount}");
    if (lapCount >= lapsToWin)
    {
      onRaceFinish();
    }
  }
  void onRaceFinish()
  {
    foreach (var ps in raceFinishParticles)
    {
      ps.Play();
    }
    _gsm.onPlayerWin();
  }

  void OnDrawGizmos()
  {
    _checkpoints = GetComponentsInChildren<lapCheckpoint>();
    for (int i = 0; i < _checkpoints.Length; i++)
    {
      _checkpoints[i].name = $"Checkpoint {i + 1}";
    }
  }
  public Vector3[] checkPointLocations
  {
    get
    {
      Vector3[] ret = new Vector3[_checkpoints.Length];
      for (int i = 0; i < _checkpoints.Length; i++)
      {
        ret[i] = _checkpoints[i].transform.position;
      }
      return ret;
    }
  }
}
