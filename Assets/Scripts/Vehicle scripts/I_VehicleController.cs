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

    public float _maxNitroAmount = 100f;
    public float _nitroAmount = 50f;

    public float _nitroSpeedBoost = 2f;

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

    public virtual void setAsAutoDriveAI()
    {
        Debug.LogError("Not implemented yet");
    }

    public virtual void unflipSelf()
    {
        Debug.LogError("Not Implemented yet");
    }
    public virtual void respawn()
    {
        transform.position = _respawnPosition;
        transform.rotation = _respawnRotation;
        _vehiclePhysics._rigidBody.freezeRotation = true;
        _vehiclePhysics.setRigidBodyVelocity(Vector3.zero);
    }
    public virtual void setNewRespawnPosition()
    {
        _respawnPosition = transform.position;
        _respawnRotation = transform.rotation;
    }
    public virtual void setNewRespawnPosition(Vector3 newPos)
    {
        _respawnPosition = newPos;
    }
    public virtual void setNewRespawnPosition(Transform newRespawn)
    {
        _respawnPosition = newRespawn.position;
        _respawnRotation = newRespawn.rotation;
    }
    void OnTriggerEnter(Collider other)
    {
        var collectable = other.GetComponent<Collectables>();
        // Unity object's should not use propogation
        // Well your mother soon to be full of lacerations
        // Sure, it's uncool to me the emancipation of a life 
        // Gunned - hope it not be in this nasty nation
        // - Ethan arrazola
        collectable?.onPickup(this);
    }
}
