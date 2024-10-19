using UnityEngine;

public class CarEngineSound : MonoBehaviour
{
    public AudioSource engineAudioSource; 
    public float minPitch = 0.8f;         
    public float maxPitch = 2.0f;         
    public float maxSpeed = 200f;         

    private Rigidbody carRigidbody;

    void Start()
    {
        carRigidbody = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float carSpeed = carRigidbody.velocity.magnitude;

        float pitch = Mathf.Lerp(minPitch, maxPitch, carSpeed / maxSpeed);

        engineAudioSource.pitch = pitch;
    }
}

