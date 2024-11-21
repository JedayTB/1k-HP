using System.Collections;
using UnityEngine;

public class ChilliOilPuddle : MonoBehaviour
{
    [SerializeField] private int OwnerHashCode;
    [SerializeField] private float speedMultiplier = 1.25f;
    [SerializeField] private float _speedBoostTime = 0.75f;
    [SerializeField] public Rigidbody rb;

    I_VehicleController vhc;
    public void Init(PlayerVehicleController owner)
    {
        OwnerHashCode = owner.GetInstanceID();
        rb = GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        
    }
    IEnumerator increaseOwnerSpeed(float time, PlayerVehicleController owner)
    {
        float count = 0f;
        float baseAccel = owner.VehiclePhysics.Acceleration;

        owner.VehiclePhysics.Acceleration *= speedMultiplier;
        while (count < time)
        {
            count += Time.deltaTime;

            yield return null;
        }
        owner.VehiclePhysics.Acceleration = baseAccel;


    }
}
