using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // Single instance shakes are ones that happen once (e.g. car being hit, being struck by something, explosion, etc.)
    [Header("Single Instance Settings")]
    [SerializeField][Tooltip("Length of the shake")] private float _shakeDuration;
    [SerializeField][Tooltip("How much the camera shakes")] private float _shakeIntensity;
    [SerializeField][Tooltip("How often a new shake happens \nLower is more often")] private float _shakeHarshness;
    [SerializeField][Tooltip("Determines if the shake should become more gentle near the end")] private bool _hasFalloff;
    [SerializeField][Tooltip("How long should the falloff take")] private float _falloffDuration;

    [Space(15)]
    [SerializeField] private float _xInfluence = 1f;
    [SerializeField] private float _yInfluence = 1f;
    [SerializeField] private float _zInfluence = 1f;

    [Space(15)]
    [SerializeField] private float _progress;
    public bool doShake = false;

    // Continuous shake is the slight shaking from going fast
    [Header("Contiuous Settings")]
    [SerializeField][Tooltip("Basically controls how intense the shake is \nLower is more intense")] private float _intensityDivision = 20f;
    [SerializeField][Tooltip("How often a new shake happens \nLower is more often")] private float _harshnessDivision = 1f;
    [SerializeField][Tooltip("How many KM/H until the next shake itensity increase")] private float _intensityIncreaseRate = 10f;

    private float _currentLerpDuration;
    private float _startTime;
    private Vector3 _startLocation;
    private Vector3 _endLocation;
    private Vector3 _originalPosition;
    private bool _right; // trust;
    private float _timeWaitingToShake = 999f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J) || doShake)
        {
            StartCoroutine(Shake(_shakeDuration, _shakeIntensity, _shakeHarshness, _hasFalloff, _falloffDuration));
            doShake = false;
        }

        Vector3 velocity = GameStateManager.Player.VehiclePhysics.getVelocity();
        float velocitymag = velocity.magnitude;
        float amountToShake = velocitymag - 80; // only do it after they reach 80kmh
        amountToShake = (int)amountToShake / 10; // only increase shake intensity every 10 kmph
        amountToShake = (float)amountToShake / _intensityDivision; // this just scales the shake way down so it's not insanely aggresive

        if (amountToShake > 0)
        {
            ContinuousShake(amountToShake, 1/_harshnessDivision);
        }
        else
        {
            transform.localPosition = Vector3.zero;
        }
    }

    private void ContinuousShake(float intensity, float harshness)
    {
        Vector3 originalPosition = transform.localPosition;
        _originalPosition = originalPosition;

        if (_timeWaitingToShake > harshness / 10)
        {
            float xPos = Random.Range(0, _xInfluence * intensity);
            float yPos = Random.Range(0, _yInfluence * intensity);
            float zPos = Random.Range(0, _zInfluence * intensity);

            _endLocation = new Vector3(xPos, yPos, zPos);
            _endLocation.x *= _right ? 1 : -1;

            // Just alternate between right and left lmao
            _right = !_right;

            LerpReset();

            _timeWaitingToShake = 0;
        }

        ShakeLerp(_endLocation, harshness / 10);

        _timeWaitingToShake += Time.deltaTime;
    }

    IEnumerator Shake(float duration, float intensity, float harshness, bool hasFalloff, float falloffDuration)
    {
        Vector3 originalPosition = transform.localPosition;
        _originalPosition = originalPosition;
        float timeShaking = 0;
        float timeWaitingToShake = 999f;

        // This is just so we can scale the intensity later if falloff is enabled
        float originalIntensity = intensity;
        bool right = true; // trust
        
        // Adds the falloff time to the total duration if enabled
        // Needed so the loop doesn't end before the falloff
        duration = hasFalloff ? duration + falloffDuration : duration;

        while (timeShaking < duration)
        {
            // duration - falloffDuration is done to check if it is time for the falloff to start
            if (hasFalloff && timeShaking >= duration - falloffDuration)
            {
                //print("the falloff is crazy");

                // Terrible formula...
                // duration - falloffDuration is so we are scaling based off the duration of the main shake
                // timeShaking - duration / 
                // lowkey mid commenting i already forget it but it works so idk
                float amountToReduceBy = (timeShaking - (duration - falloffDuration)) / falloffDuration;
                intensity = originalIntensity * (1 - amountToReduceBy);
            }

            if (timeWaitingToShake > harshness/10)
            {
                float xPos = Random.Range(0, _xInfluence * intensity);
                float yPos = Random.Range(0, _yInfluence * intensity);
                float zPos = Random.Range(0, _zInfluence * intensity);

                _endLocation = new Vector3(xPos, yPos, zPos);
                _endLocation.x *= right ? 1 : -1;

                // Just alternate between right and left lmao
                right = !right;

                LerpReset();

                timeWaitingToShake = 0;
            }

            ShakeLerp(_endLocation, harshness / 10);

            timeShaking += Time.deltaTime;
            timeWaitingToShake += Time.deltaTime;

            yield return null;
        }

        StartCoroutine(FinishShake(originalPosition, harshness / 10));
    }
    private void LerpReset()
    {
        _startTime = Time.time;
        _startLocation = transform.localPosition;
        _currentLerpDuration = 0.00001f;
        _progress = 0;
    }

    private void ShakeLerp(Vector3 endPosition, float duration)
    {
        float progress = _currentLerpDuration / duration;

        progress = 1 - Mathf.Pow(1 - _currentLerpDuration / duration, 2);

       //if (progress < 0.5f)
        {
            //progress = 2 * progress * progress;
        }
        //else
        {
            //progress = 1 - Mathf.Pow(-2 * progress + 2, 2) / 2;
        }

        transform.localPosition = Vector3.Lerp(_startLocation, endPosition, progress);
        _currentLerpDuration += Time.deltaTime;
    }

    IEnumerator FinishShake(Vector3 originalPosition, float duration)
    {
        LerpReset();
        print("poo");
        _currentLerpDuration = 0.0001f;

        //float progress = 1 - Mathf.Pow(1 - (Time.time - _startTime - duration), 0.5f);

        while (_currentLerpDuration < duration)
        {
            float progress = _currentLerpDuration / duration;
            if (progress < 0.5f)
            {
                progress = 2 * progress * progress;
            }
            else
            {
                progress = 1 - Mathf.Pow(-2 * progress + 2, 2) / 2;
            }

            _progress = progress;
            transform.localPosition = Vector3.Lerp(_startLocation, _originalPosition, progress);
            _currentLerpDuration += Time.deltaTime;
            yield return null;
        }
        
        LerpReset();
    }
}
