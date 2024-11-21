using UnityEngine;

public class CindyController : PlayerVehicleController
{
    [SerializeField] private ChilliOilPuddle prototypeChilli;
    [SerializeField] private float throwPower = 30f;

    [SerializeField] private float throwSpread = 5f;
    [SerializeField] private int AmountOfThrowingProjectiles = 3;


    protected override void Update()
    {
        base.Update();
    }

    private void throwChilliOil()
    {
        int modif = 1;
        float throwAngl = 0f;

        Vector3 setRotation = transform.localRotation.eulerAngles;
        Vector3 rbVelocity = _vehiclePhysics.RigidBody.velocity;
        Vector3 throwDir = new(45, 0,0);

        for (int i = 0; i < AmountOfThrowingProjectiles; i++)
        {
            throwAngl = i * throwSpread * modif;
            modif *= -1;
            
            var tempOil = Instantiate(prototypeChilli);
            tempOil.Init(this);
            tempOil.transform.position = transform.position + Vector3.up;
            
            setRotation.y = throwAngl;

            tempOil.transform.rotation = Quaternion.Euler(setRotation);
            throwDir.y = throwAngl;
            print(throwDir);
            tempOil.rb.AddForce(throwPower * throwDir);
        }
    }

    protected override void onAbilityFull()
    {
        base.onAbilityFull();
    }
    public override void useCharacterAbility()
    {
        if(_abilityGauge >= 100)
        {
            throwChilliOil();
            _abilityGauge = 0;
        }
        
    }

}
