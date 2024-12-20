using System.Collections;
using UnityEngine;

public class MimiController : PlayerVehicleController
{
    [Header("Mimi Bubble Specifics")]
    [SerializeField] private float _BubbleActiveTime = 5f;
    [SerializeField] private float _bounceStrength = 40;
    [SerializeField] private float _speedIncreaseAmt = 1.5f;
    [SerializeField] private float _driftChargeIncreaseAmt = 1.4f;
    [SerializeField] GameObject _bubble;
    public override void useCharacterAbility()
    {
        if (_abilityGauge >= 100)
        {
            _bubble.SetActive(true);
            StartCoroutine(bubbleActive(_BubbleActiveTime));
            _abilityGauge = 0;
            //Debug.Log("ability used!");
        }
    }
    IEnumerator bubbleActive(float bubbleActiveTime)
    {   
        print("bubble func being run");
        float count = 0;
        
        _builtUpNitroAmount *= _driftChargeIncreaseAmt;
        _vehiclePhysics.horsePower *= _speedIncreaseAmt;
        
        while (count < bubbleActiveTime)
        {
            count += Time.deltaTime;

            yield return null;
        }
        _bubble.SetActive(false);
        print("bubble func ended");
        _builtUpNitroAmount /= _driftChargeIncreaseAmt;
        _vehiclePhysics.horsePower /= _speedIncreaseAmt;

    }
    protected override void Update()
    {
        //playerControlsLogic();
        base.Update();
    }
    //Should be from the capsule
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other); //Just does checking for collectables

        A_VehicleController vehicle = other.GetComponentInParent<A_VehicleController>();

        if (vehicle != null)
        {
            bounceOff(vehicle);
        }
    }
    private void bounceOff(A_VehicleController vehicle)
    {
        Vector3 dirToBounce = vehicle.transform.position - transform.position;
        float vehicleWeight = vehicle.VehiclePhysics.RigidBody.mass;
        vehicle.VehiclePhysics.RigidBody.AddForce(_bounceStrength * vehicleWeight * dirToBounce);
    }
}
