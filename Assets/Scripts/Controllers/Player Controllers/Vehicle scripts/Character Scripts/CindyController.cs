using UnityEngine;

public class CindyController : PlayerVehicleController
{
    [SerializeField] private ChilliOilPuddle prototypeChilli;
    [SerializeField] private float throwPower = 30f;

    [SerializeField] private float ThrowSpread = 5f;
    [SerializeField] private int Projectiles = 3;


    protected override void Update()
    {
        base.Update();
    }

    private void throwChilliOil()
    {

    }

    protected override void onAbilityFull()
    {
        base.onAbilityFull();
    }
    public override void useCharacterAbility()
    {
        base.useCharacterAbility();
    }


}
