using UnityEditor.Build;
using UnityEngine;

public class MimiController : PlayerVehicleController
{
    [SerializeField] private float _bounceStrength = 40;
    [SerializeField] GameObject _bubble;
    public override void useCharacterAbility()
    {
        if(_abilityGauge >= 100)
        {
            _bubble.SetActive(true);
            //Debug.Log("ability used!");
        }
        
    }
    protected override void Update()
    {
        //playerControlsLogic();
        base.Update();

        if (_bubble.gameObject.activeSelf) {
            _abilityElapsedTime += Time.deltaTime;

            if (_abilityElapsedTime >= _abilityUseTimer) {

                _bubble.SetActive(false);
                _abilityElapsedTime = 0f;

            }

        }
    }
    //Should be from the capsule
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other); //Just does checking for collectables

        I_VehicleController vehicle = other.GetComponentInParent<I_VehicleController>();

        if (vehicle != null) {
            Debug.Log("got vehicle");
            bounceOff(vehicle);
        } 
    }
    private void bounceOff(I_VehicleController vehicle)
    {
        Vector3 dirToBounce = vehicle.transform.position - transform.position;
        float vehicleWeight = vehicle._vehiclePhysics.RigidBody.mass;
        vehicle._vehiclePhysics.RigidBody.AddForce(_bounceStrength * vehicleWeight * dirToBounce);
    }
}
