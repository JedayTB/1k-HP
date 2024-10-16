using UnityEngine;

[RequireComponent (typeof(BoxCollider))]
public class Collectables : MonoBehaviour
{
    void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }
    public virtual void onPickup(I_VehicleController vehicle)
    {
        Debug.Log($"{this.name} got picked up. Disabling object");
        this.gameObject.SetActive(false);
    }
    
}
