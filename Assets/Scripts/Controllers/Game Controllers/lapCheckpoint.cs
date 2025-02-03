using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class lapCheckpoint : MonoBehaviour
{

  public int checkPointNumber;
  private BoxCollider _BC;
  [SerializeField] private Transform respawnPoint;
  public HashSet<A_VehicleController> _vehiclesPassedThroughCheckpoint;

  public void Init(int checkPointNumber)
  {
    _BC = GetComponent<BoxCollider>();
    _BC.isTrigger = true;
    this.checkPointNumber = checkPointNumber;

    _vehiclesPassedThroughCheckpoint = new HashSet<A_VehicleController>();
  }
  public void resetHashset()
  {
    _vehiclesPassedThroughCheckpoint.Clear();
  }
  void OnTriggerEnter(Collider other)
  {

    var vehicle = other.gameObject.transform.parent.parent.gameObject.GetComponent<A_VehicleController>();
    //var vehicle = other.GetComponentInParent<A_VehicleController>();

    if (vehicle != null)
    {
      if (_vehiclesPassedThroughCheckpoint.Contains(vehicle) == false)
      {
        _vehiclesPassedThroughCheckpoint.Add(vehicle);

        vehicle.checkpointsPassedThrough[checkPointNumber] = true;

        GameStateManager.Instance._lapChecker.checkIfVehicleFinishedlap(vehicle);
        setVehicleRespawn(vehicle);

      }


    }
  }
  void setVehicleRespawn(A_VehicleController vehicle)
  {
    vehicle?.setNewRespawnPosition(respawnPoint);
  }
}

