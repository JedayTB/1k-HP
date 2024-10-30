using UnityEngine;

public class NitroCell : Collectables
{

    [SerializeField] private int _nitroCellAmount = 1;
    [SerializeField] private BoxCollider _boxCollider;
    
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.isTrigger = true;
    }
    public override void onPickup(I_VehicleController vehicle)
    {
        vehicle.addNitro(_nitroCellAmount);
        base.onPickup(vehicle);
        
    }
}
