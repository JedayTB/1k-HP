using System.Collections;
using UnityEngine;

public class ChilliOilPuddle : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private BoxCollider BC;
    [SerializeField] private BoxCollider BCTrigger;
    [SerializeField] private I_VehicleController Owner;
    [SerializeField] private float speedMultiplier = 1.25f;
    [SerializeField] private float _speedBoostTime = 0.75f;
    [SerializeField] public Rigidbody rb;
    I_VehicleController vhc;


    void Awake(){
        BC.enabled = false;
        BCTrigger.enabled = false;
    }
    public void Init(PlayerVehicleController owner)
    {
        Owner = owner;
        rb = GetComponent<Rigidbody>();
    }
    private void Update(){
        if(!BC.enabled && !BCTrigger.enabled){
            bool isGrounded = Physics.Raycast(transform.position, -transform.up, 1f, groundLayers);
            BC.enabled = isGrounded;
            BCTrigger.enabled = isGrounded;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning($"Getting Component off of {other.gameObject.name}. Inefficient");
        //More fuck ugly dogshit stupid code
        //            physics model  actual object with I_VehicleController attached
        vhc = other.transform.parent.parent.gameObject.GetComponent<I_VehicleController>();
        if(vhc != null && vhc == Owner){
            print("Boosting Owner speed!");
            StartCoroutine(increaseOwnerSpeed(_speedBoostTime, speedMultiplier, vhc));
            vhc = null;
        }
        print($"{other.gameObject.name}");
    }
    IEnumerator increaseOwnerSpeed(float time,float speedMultiplier, I_VehicleController owner)
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
