using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreRaceCamera : MonoBehaviour
{
    [SerializeField] private List<Transform> initialLocations = new List<Transform>();
    [SerializeField] private List<Transform> endLocations = new List<Transform>();
    [SerializeField] private List<float> lerpSpeeds = new List<float>(); // these set how long each shot should take
    [SerializeField] private float lerpDuration = 2f;
    [SerializeField] private CameraFollower3D cam;
    [SerializeField] private bool isLevelSelect = false;

    private int transformIndex = 0;
    public static bool cutSceneIsHappening = true;
     
    void Start()
    {
        cutSceneIsHappening = true;
        if (!isLevelSelect)
        {
            endLocations[endLocations.Count - 1] = cam._desiredLocation;
        }
        StartCoroutine(CameraLerp(false));
    }

    private void Update()
    {
        if (!isLevelSelect)
        {
            if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && cutSceneIsHappening)
            {
                StopAllCoroutines();
                cutSceneIsHappening = false;
                cam.enabled = true;
            }
        }
    }

    private IEnumerator CameraLerp(bool ease)
    {
        float currentDuration = 0.0001f;
        transform.position = initialLocations[transformIndex].position;
        transform.rotation = initialLocations[transformIndex].rotation;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float progress = 0;
        float cachedProgress = 0;
        //cam.setTarget(lookAtLocations[transformIndex], false);

        if (!ease)
        {
            while (progress < 1)
            {
                progress = currentDuration / lerpSpeeds[transformIndex];
                currentDuration += Time.deltaTime;

                transform.position = Vector3.Lerp(startPosition, endLocations[transformIndex].position, progress);
                transform.rotation = Quaternion.Lerp(startRotation, endLocations[transformIndex].rotation, progress);

                yield return null;
            }
        }
        else
        {
            while (progress < 1)
            {
                progress = 1 - Mathf.Pow(1 - currentDuration / lerpSpeeds[transformIndex], 2);

                if (progress < cachedProgress)
                {
                    progress = 1;
                }

                currentDuration += Time.deltaTime;

                transform.position = Vector3.Lerp(startPosition, endLocations[transformIndex].position, progress);
                transform.rotation = Quaternion.Lerp(startRotation, endLocations[transformIndex].rotation, progress);

                cachedProgress = progress;
                yield return null;
            }
        }

        transformIndex++;
        if (isLevelSelect)
        {
            if (transformIndex == endLocations.Count)
            {
                transformIndex = 0;
            }

            StartCoroutine(CameraLerp(false));

        }
        else
        {
            if (transformIndex == endLocations.Count - 1)
            {
                StartCoroutine(CameraLerp(true));
            }
            else if (transformIndex < endLocations.Count)
            {
                StartCoroutine(CameraLerp(false));
            }
            else
            {
                cam.enabled = true;
                cutSceneIsHappening = false;
            }
        }
    }
}
