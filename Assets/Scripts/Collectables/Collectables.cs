using UnityEngine;

[RequireComponent (typeof(BoxCollider))]
public class Collectables : MonoBehaviour
{

    public virtual void onPickup(I_VehicleController vehicle)
    {
        Debug.Log($"{this.name} got picked up");        
    }
    
}
