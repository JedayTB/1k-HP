using UnityEngine;

[RequireComponent(typeof(BoxCollider))]

public class KillZone : MonoBehaviour
{
    private Collider _killZone;
    void Start()
    {
        _killZone = GetComponent<Collider>();
        _killZone.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var vehicle = other.GetComponentInParent<A_VehicleController>();

        if (vehicle != null) vehicle.respawn();
        //Debug.Log($"{other.name}");
        Debug.Log($"{vehicle} hit killzone.");
    }
}
