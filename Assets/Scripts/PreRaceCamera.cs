using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PreRaceCamera : MonoBehaviour
{
    [SerializeField] private List<Transform> initialLocations = new List<Transform>();
    [SerializeField] private List<Transform> endLocations = new List<Transform>();
    [SerializeField] private List<Transform> lookAtLocations = new List<Transform>();
    [SerializeField] private float lerpDuration = 2f;
    [SerializeField] private CameraFollower3D cam;

    private int transformIndex = 0;
    public static bool cutSceneIsHappening = true;
     
    void Start()
    {
        endLocations[0] = cam._desiredLocation;
        StartCoroutine(CameraLerp(true)); // temp for just this build
    }

    private void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && cutSceneIsHappening)
        {
            StopAllCoroutines();
            cutSceneIsHappening = false;
            cam.enabled = true;
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
                progress = currentDuration / lerpDuration;
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
                progress = 1 - Mathf.Pow(1 - currentDuration / lerpDuration, 2);

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
