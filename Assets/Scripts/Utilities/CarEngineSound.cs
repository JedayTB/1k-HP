using UnityEngine;

public class CarEngineSound : MonoBehaviour
{
  public AudioSource engineAudioSource;
  static float minPitch = 0.3f;
  static float maxPitch = 2.25f;
  static float maxSpeed = 200f;

  private CustomCarPhysics carPhys;

  void Start()
  {
    carPhys = GetComponent<CustomCarPhysics>();
    maxSpeed = carPhys.GearTwo.MaxSpeed;
  }

  void Update()
  {
    float carSpeed = carPhys.getSpeed();

    float pitch = Mathf.Lerp(minPitch, maxPitch, carSpeed / maxSpeed);

    engineAudioSource.pitch = LerpAndEasings.ExponentialDecay(engineAudioSource.pitch, pitch, 5f, Time.deltaTime);
  }
}

