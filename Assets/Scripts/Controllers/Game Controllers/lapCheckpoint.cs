using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class lapCheckpoint : MonoBehaviour
{

  public int checkPointNumber;
  public bool passedCheckpoint = false;
  private BoxCollider _BC;
  private CheckFinishedLap lapPassed;
  [SerializeField] private Transform respawnPoint;
  HashSet<A_VehicleController> _vehiclesPassedThroughCheckpoint;

  

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
    vehicle?.setNewRespawnPosition(respawnPoint);


  }
}

