using UnityEngine;

public class NitroCell : Collectables
{

    [SerializeField] private int _nitroCellAmount = 1;
    [SerializeField] private BoxCollider _boxCollider;
    public Renderer otherRend;
    public AudioSource _nitroPickup;
    
    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider>();
        _boxCollider.isTrigger = true;
    }
    public override void onPickup(A_VehicleController vehicle)
    {
        // lol
        for(int i = 0; i < _nitroCellAmount; i++){
            vehicle.addNitro();
        }
        base.onPickup(vehicle);
    }
}
