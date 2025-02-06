using UnityEngine;
using UnityEngine.Rendering.Universal;
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
    [Header("Speed Thresholds")]

    [SerializeField] private float _minSpeedThreshold = 100f;
    [SerializeField] private float _maxSpeedThreshold = 250f;

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
    [SerializeField] private float nitroSpawnRate;
    [SerializeField] private float nitroRadiusSize;


    private float _currentRadiusSize;
    private float _currentSpawnRate;

    private float playerSpeed = 0f;
    private bool playerIsInNitro = false;
    private float lerpByValue = 0f;
    private void Awake()
    { 
        _Radius = speedLinesOBJ.GetFloat(RadiusID);
        _ParticleSpawnRate = speedLinesOBJ.GetFloat(ParticleSpawnRateID);

        _XScaleRange = speedLinesOBJ.GetVector2(XScaleRangeID);
        _YScaleRange = speedLinesOBJ.GetVector2(YScaleRangeID);

        particleTexture = NormalTexture;
        particleTexture = speedLinesOBJ.GetTexture(ParticleTextureID);
        

        nitroSpawnRate = Mathf.Lerp(_minSpawnRate, _maxSpawnRate, 0.5f);
        nitroRadiusSize = Mathf.Lerp(_minRadiusSize, _maxRadiusSize, 0.5f);
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
        if(playerSpeed > _minSpeedThreshold)
        {
            lerpByValue = Mathf.Clamp01(playerSpeed / _maxSpeedThreshold + _minSpeedThreshold);
            _currentRadiusSize = Mathf.Lerp(_maxRadiusSize, _minRadiusSize, lerpByValue);
            _currentSpawnRate = Mathf.Lerp(_minSpawnRate, _maxSpawnRate, lerpByValue);

            speedLinesOBJ.SetFloat(RadiusID, _currentRadiusSize);
            speedLinesOBJ.SetFloat(ParticleSpawnRateID, _currentSpawnRate);
        }
        else
        {
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
