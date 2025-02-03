using UnityEngine;

public class LapChecker : MonoBehaviour
{
  [SerializeField] private lapCheckpoint[] _checkpoints;

  [SerializeField] private int lapsToWin = 3;
  [SerializeField] private ParticleSystem[] raceFinishParticles;


  private LapTimer _lapTimer;

  private GameStateManager _gsm;
  public int lapsCompleted = 0;


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
      if (vehicle.checkpointsPassedThrough[i] == false)
      {
        return;
      }
      nextPos = i + 1;

      if (nextPos == _checkpoints.Length || nextPos > _checkpoints.Length)
      {
        nextPos = 0;
      }
      _gsm.setVehicleNextCheckpoint(vehicle, nextPos);
    }
    // If here is reached, lap was finished
    _gsm.setVehicleNextCheckpoint(vehicle, 0);

    onLapFinished(vehicle);

    _gsm.setVehicleLapCount(vehicle);

  }
  void onLapFinished(A_VehicleController vehicleFinished)
  {
    // Need to make sure this doesnt get triggered
    // everytime a vehicle finishes a lap, only while (true)

    //if(vehicleFinished.lapsPassed == lapsCompleted++;

    foreach (var checkpoint in _checkpoints)
    {
      checkpoint._vehiclesPassedThroughCheckpoint.Remove(vehicleFinished);
    }
    if (vehicleFinished is PlayerVehicleController) _lapTimer?.endLap();

    Debug.Log($"Finished lap: {lapsCompleted}");

    if (lapsCompleted >= GameStateManager.Instance.LapsToFinishRace)
    {
      onRaceFinish();
    }
  }
  void onRaceFinish()
  {
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
