using UnityEngine;

public class LapChecker : MonoBehaviour
{
    [SerializeField] private lapCheckpoint[] _checkpoints;
    public Vector3[] chkPointLoc;
    private LapTimer _lapTimer;

    private GameStateManager _gsm;
    public int lapsCompleted = 0;
    private void OnValidate()
    {
        chkPointLoc = new Vector3[_checkpoints.Length];
        for(int i = 0;  i < _checkpoints.Length; i++)
        {
            chkPointLoc[i] = _checkpoints[i].transform.position;
        }
    }

    public void Init(GameStateManager gsm)
    {
        _gsm = gsm;

        _checkpoints = GetComponentsInChildren<lapCheckpoint>();
        _lapTimer = FindObjectOfType<LapTimer>();
        if (_lapTimer != null)
        {
            Debug.Log("Found Lap timer");
        }
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

        // if got here and didn't return, needs to pass first checkpoint one more time!
        if (vehicle.needsToPassFirstTwice == false)
        {
            vehicle.needsToPassFirstTwice = true;
            // index 0 is always first checkpoint
            vehicle.checkpointsPassedThrough[0] = false;
            _checkpoints[0]._vehiclesPassedThroughCheckpoint.Remove(vehicle);
            return;
        }

        onLapFinished(vehicle);
        vehicle.needsToPassFirstTwice = false;

    }
    void onLapFinished(A_VehicleController vehicleFinished)
    {
        if (vehicleFinished.lapsPassed == lapsCompleted)
        {
            lapsCompleted++;
        }

        _gsm.setVehicleLapCount(vehicleFinished);
        _gsm.setVehicleNextCheckpoint(vehicleFinished, 0);

        // So that only 1 vehicle can increment lapsCompleted
        // That vehile will always be the one that finishes a lap,
        // and has a lap count thats the same as the completed amount.


        foreach (var checkpoint in _checkpoints)
        {
            checkpoint._vehiclesPassedThroughCheckpoint.Remove(vehicleFinished);
        }

        if (vehicleFinished is PlayerVehicleController)
        {
            Debug.Log("Player finished a lap");
            _gsm._uiController.currentLapText.text = $"Lap: {lapsCompleted + 1}/{_gsm.LapsToFinishRace}";
            _gsm._uiController.newLapAnimText.text = $"Lap: {lapsCompleted + 1}/{_gsm.LapsToFinishRace}";
            _gsm._uiController.newLapAnimator.SetTrigger("NewLapPassed");
            _lapTimer?.endLap();
        }

        Debug.Log($"Finished lap: {lapsCompleted}");
        // Because, vehicle needs to pass 1st checkpoint twice to technically end the lap
        // and when a lap is finished, it resets all checkpoints.
        // Meaning player passes through 1st checkpoint, finishes lap, then 1st checlkpoint is set as not passed through.
        // Shitty code. by yours truly, Ethan Arrazola
        vehicleFinished.checkpointsPassedThrough[0] = true;
        _checkpoints[0]._vehiclesPassedThroughCheckpoint.Add(vehicleFinished);
        if (lapsCompleted >= GameStateManager.Instance.LapsToFinishRace)
        {
            _gsm.onPlayerWin();
        }
    }


    // Helper methods
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
