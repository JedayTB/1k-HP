using System.Collections;
using UnityEngine;

public class BoostPannels : MonoBehaviour
{
    [SerializeField] private float boostStrength = 750;
    [SerializeField] private float pannelBoostTime = 1f;

    void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.GetComponentInParent<A_VehicleController>().VehiclePhysics.RigidBody;
        if(rb != null) {
            Debug.Log($"Got RB: {rb}");
            StartCoroutine(AddBoostToVehicle(pannelBoostTime, rb));
        }
    }
    IEnumerator AddBoostToVehicle(float time, Rigidbody vehicleRB){
        float count = 0f;

        while(count < time){
            count += Time.deltaTime;
            vehicleRB.AddForce(boostStrength * transform.forward);
            yield return null;
        }
    }
}
