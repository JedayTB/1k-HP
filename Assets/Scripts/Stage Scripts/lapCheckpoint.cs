using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class lapCheckpoint : MonoBehaviour
{
    public int checkPointNumber;
    public bool passedCheckpoint = false;

    private BoxCollider _BC;
    private CheckFinishedLap lapPassed;

    [SerializeField] private Transform respawnPoint;

    public void Init(int checkPointNumber, CheckFinishedLap lapLogic){
        _BC = GetComponent<BoxCollider>();
        _BC.isTrigger = true;
        this.checkPointNumber = checkPointNumber;
        lapPassed = lapLogic;
    }
    void OnTriggerEnter(Collider other)
    {
        var vehicle = other.GetComponentInParent<I_VehicleController>();

        if(other.CompareTag("PLAYER")){
            //print($"Player Passes Through Checkpoint {checkPointNumber}");
            passedCheckpoint = true;
            lapPassed?.Invoke();
            vehicle?.setNewRespawnPosition();
        }
    }
}
