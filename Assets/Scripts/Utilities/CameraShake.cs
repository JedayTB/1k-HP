using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{

    [SerializeField][Tooltip("Length of the shake")] private float _shakeDuration;
    [SerializeField][Tooltip("How much the camera shakes")] private float _shakeIntensity;
    [SerializeField][Tooltip("How often a new shake happens \nLower is more often")] private float _shakeHarshness;

    [Space(15)]
    [SerializeField] private float _xInfluence = 1f;
    [SerializeField] private float _yInfluence = 1f;
    [SerializeField] private float _zInfluence = 1f;

    [Space(15)]
    [SerializeField] private float _progress;

    private float _currentLerpDuration;
    private float _startTime;
    private Vector3 _startLocation;
    private Vector3 _endLocation;
    private Vector3 _originalPosition;
   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(Shake(_shakeDuration, _shakeIntensity, _shakeHarshness));
        }
    }

    IEnumerator Shake(float duration, float intensity, float harshness)
    {
        Vector3 originalPosition = transform.localPosition;
        _originalPosition = originalPosition;
        float timeShaking = 0;
        float timeWaitingToShake = 999f;

        while (timeShaking < duration)
        {

            print("we are shaking");

            if (timeWaitingToShake > harshness/10)
            {
                float xPos = Random.Range(-_xInfluence * intensity, _xInfluence * intensity);
                float yPos = Random.Range(-_yInfluence * intensity, _yInfluence * intensity);
                float zPos = Random.Range(-_zInfluence * intensity, _zInfluence * intensity);

                _endLocation = new Vector3(xPos, yPos, zPos);
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
    }

    private void ShakeLerp(Vector3 endPosition, float duration)
    {
        print("are we actually lerping here");

        float progress = _currentLerpDuration / duration;
        if (progress < 0.5f)
        {
            progress = 2 * progress * progress;
        }
        else
        {
            progress = 1 - Mathf.Pow(-2 * progress + 2, 2) / 2;
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
            transform.localPosition = Vector3.Lerp(_startLocation, originalPosition, progress);
            _currentLerpDuration += Time.deltaTime;
            yield return null;
        }
        
        LerpReset();
    }
}
