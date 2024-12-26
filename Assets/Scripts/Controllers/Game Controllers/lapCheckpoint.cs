using System.Collections.Generic;
using UnityEngine;

public enum LapSetType
{
  useVehicleTransform,
  useRespawnCubePos,
  useRespawnCubeTransform
}
[RequireComponent(typeof(BoxCollider))]
public class lapCheckpoint : MonoBehaviour
{

  public int checkPointNumber;
  public bool passedCheckpoint = false;
  private BoxCollider _BC;
  private CheckFinishedLap lapPassed;
  [SerializeField] private Transform respawnPoint;
  HashSet<A_VehicleController> _vehiclesPassedThroughCheckpoint;

  [SerializeField] private LapSetType _respawnPointSetType = LapSetType.useRespawnCubePos;

  public void Init(int checkPointNumber, CheckFinishedLap lapLogic)
  {
    _BC = GetComponent<BoxCollider>();
    _BC.isTrigger = true;
    this.checkPointNumber = checkPointNumber;
    lapPassed = lapLogic;

    _vehiclesPassedThroughCheckpoint = new HashSet<A_VehicleController>();
  }
  public void resetHashset()
  {
    _vehiclesPassedThroughCheckpoint.Clear();
  }
  void OnTriggerEnter(Collider other)
  {
    var vehicle = other.GetComponentInParent<A_VehicleController>();

    if (vehicle != null)
    {
      if (_vehiclesPassedThroughCheckpoint.Contains(vehicle) == false)
      {
        _vehiclesPassedThroughCheckpoint.Add(vehicle);

        passedCheckpoint = true;


        lapPassed?.Invoke();
        setVehicleRespawn(vehicle);

      }


    }
  }
  void setVehicleRespawn(A_VehicleController vehicle)
  {
    switch (_respawnPointSetType)
    {
      case LapSetType.useVehicleTransform:
        vehicle?.setNewRespawnPosition();
        break;

      case LapSetType.useRespawnCubePos:
        vehicle?.setNewRespawnPosition(respawnPoint.position);
        break;

      case LapSetType.useRespawnCubeTransform:
        vehicle?.setNewRespawnPosition(respawnPoint);
        break;

      default:
        Debug.LogError("We're fucked");
        break;
    }


  }
}

