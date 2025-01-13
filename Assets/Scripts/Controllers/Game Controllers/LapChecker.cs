using UnityEngine;

public class LapChecker : MonoBehaviour
{
  [SerializeField] private lapCheckpoint[] _checkpoints;

  [SerializeField] private int lapsToWin = 3;
  [SerializeField] private ParticleSystem[] raceFinishParticles;


  private LapTimer _lapTimer;

  private GameStateManager _gsm;
  public int lapCount = 0;


  public void Init(GameStateManager gsm)
  {
    _gsm = gsm;

    _checkpoints = GetComponentsInChildren<lapCheckpoint>();
    _lapTimer = FindObjectOfType<LapTimer>();

    for (int i = 0; i < _checkpoints.Length; i++)
    {
      _checkpoints[i].Init(i);
    }

  }

  public void checkIfVehicleFinishedlap(A_VehicleController vehicle)
  {
    int nextPos = 0;

    for (int i = 0; i < _checkpoints.Length; i++)
    {
      nextPos = i;
      _gsm.setVehicleNextCheckpoint(vehicle, nextPos++);
      if (vehicle.checkpointsPassedThrough[i] != true) return;
    }
    // If here is reached, lap was finished
    _gsm.setVehicleNextCheckpoint(vehicle, 0);
    // Change to see if its 1st place
    // need further logic if player isn't first place 
    // and still wants to drive
    if (vehicle is PlayerVehicleController) onLapFinished();
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
