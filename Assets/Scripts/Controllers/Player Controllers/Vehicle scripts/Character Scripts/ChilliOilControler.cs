using UnityEngine;

public class ChilliOilController : A_Ability
{
  [SerializeField] private ChilliOilPuddle prototypeChilli;
  [SerializeField] private float throwPower = 500f;

  [SerializeField] private Transform[] ChilliOilLaunchLocations;

  protected override void Awake()
  {
        base.Awake();
    if (ChilliOilLaunchLocations.Length == 0) Debug.LogError($"{this.gameObject.name} Is missing Chilli Launch Locations! Ability will not function!");
  }

   private void throwChilliOil()
   {
       for (int i = 0; i < ChilliOilLaunchLocations.Length; i++)
       {
           ChilliOilPuddle tempChilli = Instantiate(prototypeChilli);
           Transform launchLoc = ChilliOilLaunchLocations[i];

           tempChilli.Init(vehicle);

           tempChilli.transform.position = launchLoc.position;
           tempChilli.rb.velocity = vehicle.VehiclePhysics.RigidBody.velocity;

           tempChilli.rb.AddForce(throwPower * launchLoc.forward);
       }
   }

   public override void AbilityUsed()
   {
        throwChilliOil();
        gameObject.SetActive(false);
    }
  
}
