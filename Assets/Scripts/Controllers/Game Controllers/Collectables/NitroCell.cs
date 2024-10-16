using UnityEngine;

public class NitroCell : Collectables
{

    [SerializeField] private float _nitroCellAmount = 20f;
    [SerializeField] private BoxCollider _boxCollider;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.isTrigger = true;
    }
    public override void onPickup(I_VehicleController vehicle)
    {
        vehicle._nitroAmount += _nitroCellAmount;
        base.onPickup(vehicle);
    }
}
