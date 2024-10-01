using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NitroCell : Collectables
{
    private float _nitroCellAmount = 20f;

    public override void onPickup(I_VehicleController vehicle)
    {
        base.onPickup(vehicle);
        vehicle._nitroAmount += _nitroCellAmount;
    }
}
