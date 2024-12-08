using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.VFX;

public class SpeedLinesController : MonoBehaviour
{
    [SerializeField] VisualEffect speedLinesOBJ;

    public string dbgString;

    private readonly string RadiusID = "Radius";
    private readonly string XScaleRangeID = "XScaleRange";
    private readonly string YScaleRangeID = "YScaleRange";
    private readonly string ParticleSpawnRateID = "SpawnRate";

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

    [SerializeField] private float _minRadiusSize = 1f;
    [SerializeField] private float _maxRadiusSize = 2.5f;

    private float _currentRadiusSize;
    private float _currentSpawnRate;

    private float playerSpeed = 0f;

    private float lerpByValue = 0f;
    private void Awake()
    { 
        _Radius = speedLinesOBJ.GetFloat(RadiusID);
        _ParticleSpawnRate = speedLinesOBJ.GetFloat(ParticleSpawnRateID);

        _XScaleRange = speedLinesOBJ.GetVector2(XScaleRangeID);
        _YScaleRange = speedLinesOBJ.GetVector2(YScaleRangeID);
    }

    private void OnDisable()
    {
        speedLinesOBJ.SetFloat(RadiusID, 0f);
        speedLinesOBJ.SetFloat(ParticleSpawnRateID, 0f);
    }

    void Update()
    {
        playerSpeed = GameStateManager.Player.VehiclePhysics.getSpeed();

        if(playerSpeed > _minSpeedThreshold)
        {

            lerpByValue = Mathf.Clamp01(playerSpeed / _maxSpeedThreshold + _minSpeedThreshold);

            _currentRadiusSize = Mathf.Lerp(_maxRadiusSize, _minRadiusSize, lerpByValue);
            _currentSpawnRate = Mathf.Lerp(_minSpawnRate, _maxSpawnRate, lerpByValue);

            speedLinesOBJ.SetFloat(RadiusID, _currentRadiusSize);
            speedLinesOBJ.SetFloat(ParticleSpawnRateID, _currentSpawnRate);

            dbgString = $"LerpBy value {lerpByValue}\nRadius set size {_currentRadiusSize}\nSpawn rate {_currentSpawnRate}";

        }
        else
        {

            speedLinesOBJ.SetFloat(RadiusID, 0f);
            speedLinesOBJ.SetFloat(ParticleSpawnRateID, 0f);

        }

    }
}
