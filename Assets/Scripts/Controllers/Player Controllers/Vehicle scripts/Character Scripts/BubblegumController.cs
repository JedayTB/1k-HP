using System.Collections;
using UnityEngine;

public class BubblegumController : A_Ability
{
    [Header("Mimi Bubble Specifics")]
    [SerializeField] private float _BubbleActiveTime = 5f;
    [SerializeField] private float _bounceStrength = 40;
    [SerializeField] private float _speedIncreaseAmt = 1.5f;
    [SerializeField] private float _driftChargeIncreaseAmt = 1.4f;
    [SerializeField] GameObject _bubble;

    public override void AbilityUsed()
    {
        print("Used bubblegum!");
        StartCoroutine(bubbleActive(_BubbleActiveTime));
        vehicle.delistAbilityAction(onAbility);
    }
    IEnumerator bubbleActive(float bubbleActiveTime)
    {
        print("bubble func being run");
        float count = 0;

        _bubble.SetActive(true);
        print($"HP before {vehicle.VehiclePhysics.horsePower}");
        vehicle._builtUpNitroAmount *= _driftChargeIncreaseAmt;
        vehicle.VehiclePhysics.horsePower *= _speedIncreaseAmt;
        print($"HP after {vehicle.VehiclePhysics.horsePower}");
        while (count < bubbleActiveTime)
        {
            count += Time.deltaTime;

            yield return null;
        }
        _bubble.SetActive(false);
        print("bubble func ended");

        vehicle._builtUpNitroAmount /= _driftChargeIncreaseAmt;
        vehicle.VehiclePhysics.horsePower /= _speedIncreaseAmt;

        gameObject.SetActive(false);
    }

    //Should be from the capsule
    void OnTriggerEnter(Collider other)
    {

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
