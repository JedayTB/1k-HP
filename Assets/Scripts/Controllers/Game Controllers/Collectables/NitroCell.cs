using UnityEngine;

public class NitroCell : Collectables
{

    [SerializeField] private int _nitroCellAmount = 1;
    [SerializeField] private BoxCollider _boxCollider;

    public AudioSource _nitroPickup;
    
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.isTrigger = true;
    }
    public override void onPickup(A_VehicleController vehicle)
    {
        _nitroPickup.Play();
        vehicle.addNitro(_nitroCellAmount);
        base.onPickup(vehicle);
    }
}
