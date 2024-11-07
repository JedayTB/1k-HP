using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityCell : Collectables
{

    [SerializeField] private int _abilityAmount = 100;
    [SerializeField] private BoxCollider _boxCollider;

    void Start()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.isTrigger = true;
    }

    public override void onPickup(I_VehicleController vehicle)
    {
        vehicle.addAbilityGauge(_abilityAmount);
        base.onPickup(vehicle);

    }
}
