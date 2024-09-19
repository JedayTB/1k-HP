using UnityEngine;

//Used as an interface. Not going to bother with using MonoBehaviour with interfaces.
public class I_VehicleController : MonoBehaviour
{
    protected CarVisualController _vehicleVisualController;
    protected CustomCarPhysics _vehiclePhysics;

    protected Vector3 _respawnPosition;
    protected Quaternion _respawnRotation;

    protected float _throttleInput;
    protected float _turningInput;

    protected Rigidbody _vehiclePhysicsRB;

    public virtual void Init()
    {
        _vehiclePhysics = GetComponent<CustomCarPhysics>();
        _vehicleVisualController = GetComponent<CarVisualController>();

        _vehiclePhysics.Init();
        _vehicleVisualController.Init();

        _respawnPosition = transform.position;
        _respawnRotation = transform.rotation;

        Debug.Log("Car finished Initialization");
    }
    
    public virtual void setNewRespawnPosition()
    {
        _respawnPosition = transform.position;
    }
    public virtual void setNewRespawnPosition(Vector3 newPos)
    {
        _respawnPosition = newPos;
    }
    public virtual void respawn()
    {
        transform.position = _respawnPosition;
        transform.rotation = _respawnRotation;

        _vehiclePhysics.setRigidBodyVelocity(Vector3.zero);
    }
    
}
