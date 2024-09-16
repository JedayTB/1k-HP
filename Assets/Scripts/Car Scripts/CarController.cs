using UnityEngine;

public class CarController : MonoBehaviour
{
    private CarVisualController _carVisualController;
    private CustomCarPhysics _customCarPhysics;
    private Vector3 _respawnPosition;
    private Quaternion _respawnRotation;
    void Start()
    {
        _customCarPhysics = GetComponent<CustomCarPhysics>();
        _carVisualController = GetComponent<CarVisualController>();

        _customCarPhysics.Init();
        _carVisualController.Init();

        _respawnPosition = transform.position;

        Debug.Log("Car finished Initialization");
    }
    //Run by LapChecker 
    public void setNewRespawnPosition()
    {
        _respawnPosition = transform.position;
        _respawnRotation = transform.rotation;
    }
    //Run by KillZone. Does not destroy game object.
    public void respawn()
    {
        transform.position = _respawnPosition;
        transform.rotation = _respawnRotation;

        Debug.Log($"Respawned {this.name} to {_respawnPosition}");
    }
}
