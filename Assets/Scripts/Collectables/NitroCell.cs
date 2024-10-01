using UnityEngine;

public class NitroCell : Collectables
{
    [SerializeField] private float _nitroCellAmount = 20f;

    public override void onPickup(I_VehicleController vehicle)
    {
        vehicle._nitroAmount += _nitroCellAmount;
        base.onPickup(vehicle);
    }
}
