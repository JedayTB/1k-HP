using UnityEngine;

public class CindyController : PlayerVehicleController
{
    [SerializeField] private ChilliOilPuddle prototypeChilli;
    [SerializeField] private float throwPower = 500f;

    [SerializeField] private Transform[] ChilliOilLaunchLocations;

    private void Awake()
    {

        if(ChilliOilLaunchLocations.Length == 0)Debug.LogError($"{this.gameObject.name} Is missing Chilli Launch Locations! Ability will not function!");
    }

    protected override void Update()
    {
        base.Update();
    }

    private void throwChilliOil()
    {
        for (int i = 0; i < ChilliOilLaunchLocations.Length; i++)
        {
            ChilliOilPuddle tempChilli = Instantiate(prototypeChilli);
            Transform launchLoc = ChilliOilLaunchLocations[i];

            tempChilli.Init(this);

            tempChilli.transform.position = launchLoc.position;
            tempChilli.rb.velocity = _vehiclePhysics.RigidBody.velocity;

            tempChilli.rb.AddForce(throwPower * launchLoc.forward);
        }
    }

    public override void useCharacterAbility()
    {
        if (_abilityGauge >= 100)
        {
            throwChilliOil();
            _abilityGauge = 0;
        }
    }
}
