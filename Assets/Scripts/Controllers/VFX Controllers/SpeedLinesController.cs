using UnityEngine;
using UnityEngine.VFX;

public class SpeedLinesController : MonoBehaviour
{

  [SerializeField] VisualEffect speedLinesOBJ;

  private static readonly string RadiusID = "Radius";
  private static readonly string XScaleRangeID = "XScaleRange";
  private static readonly string YScaleRangeID = "YScaleRange";
  private static readonly string ParticleSpawnRateID = "SpawnRate";
  private static readonly string ParticleTextureID = "Sprite";

  private float _Radius;
  private float _ParticleSpawnRate;

  private Vector2 _XScaleRange;
  private Vector2 _YScaleRange;

  private static float _minSpeedThreshold = 45f;
  private static float _maxSpeedThreshold = 100f;

  [Header("Speed Line Spawn Rate")]

  [SerializeField] private float _minSpawnRate = 25f;
  [SerializeField] private float _maxSpawnRate = 90;

  [Header("Speed Line Radius")]

  [SerializeField] private float _minRadiusSize = 1.8f;
  [SerializeField] private float _maxRadiusSize = 2.5f;

  [Header("Nitro lines colours")]
  [SerializeField] private Texture particleTexture;
  [SerializeField] private Texture NormalTexture;
  [SerializeField] private Texture NitroTexture;
  [SerializeField] private float nitroSpawnRate = 50f;
  [SerializeField] private float nitroRadiusSize = 2f;


  private float _currentRadiusSize;
  private float _currentSpawnRate;

  private float playerSpeed = 0f;
  public bool playerIsInNitro = false;
  private float lerpByValue = 0f;

  private void Start()
  {
    _Radius = speedLinesOBJ.GetFloat(RadiusID);
    _ParticleSpawnRate = speedLinesOBJ.GetFloat(ParticleSpawnRateID);

    _XScaleRange = speedLinesOBJ.GetVector2(XScaleRangeID);
    _YScaleRange = speedLinesOBJ.GetVector2(YScaleRangeID);

    particleTexture = NormalTexture;
    particleTexture = speedLinesOBJ.GetTexture(ParticleTextureID);
    _maxSpeedThreshold = GameStateManager.Player.VehiclePhysics.GearTwo.MaxSpeed - 20f;
  }


  private void OnDisable()
  {
    speedLinesOBJ.SetFloat(RadiusID, 0f);
    speedLinesOBJ.SetFloat(ParticleSpawnRateID, 0f);
    speedLinesOBJ.SetTexture(ParticleTextureID, NormalTexture);
  }

  void Update()
  {
    playerSpeed = GameStateManager.Player.VehiclePhysics.getSpeed();
    playerIsInNitro = GameStateManager.Player.VehiclePhysics.isUsingNitro;

    // Lines colour
    if (playerIsInNitro)
    {
      particleTexture = NitroTexture;
    }
    else
    {
      particleTexture = NormalTexture;
    }

    speedLinesOBJ.SetTexture(ParticleTextureID, particleTexture);

    // Normal for speed
    if (playerSpeed > _minSpeedThreshold)
    {
      lerpByValue = Mathf.Clamp01((_minSpeedThreshold - playerSpeed) / (_minSpeedThreshold - _maxSpeedThreshold));
      //Debug.Log($"Spd Ln progress {lerpByValue}");

      _currentRadiusSize = Mathf.Lerp(_maxRadiusSize, _minRadiusSize, lerpByValue);
      _currentSpawnRate = Mathf.Lerp(_minSpawnRate, _maxSpawnRate, lerpByValue);

      speedLinesOBJ.SetFloat(RadiusID, _currentRadiusSize);
      speedLinesOBJ.SetFloat(ParticleSpawnRateID, _currentSpawnRate);
    }
    else
    {
      // No speed lines
      speedLinesOBJ.SetFloat(RadiusID, 0f);
      speedLinesOBJ.SetFloat(ParticleSpawnRateID, 0f);


      if (playerIsInNitro)
      {
        speedLinesOBJ.SetFloat(RadiusID, nitroRadiusSize);
        speedLinesOBJ.SetFloat(ParticleSpawnRateID, nitroSpawnRate);
      }
    }

  }
}
