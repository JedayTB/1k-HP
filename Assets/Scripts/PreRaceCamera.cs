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

        StartCoroutine(CameraLerp());
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

    private IEnumerator CameraLerp()
    {
        float currentDuration = 0.0001f;
        transform.position = initialLocations[transformIndex].position;
        transform.rotation = initialLocations[transformIndex].rotation;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float progress = 0;
        //cam.setTarget(lookAtLocations[transformIndex], false);

        while (progress < 1)
        {
            progress = currentDuration / lerpDuration;
            currentDuration += Time.deltaTime;

            transform.position = Vector3.Lerp(startPosition, endLocations[transformIndex].position, progress);
            transform.rotation = Quaternion.Lerp(startRotation, endLocations[transformIndex].rotation, progress);

            yield return null;
        }

        transformIndex++;

        if (transformIndex < endLocations.Count)
        {
            StartCoroutine(CameraLerp());
        }
        else
        {
            cam.enabled = true;
            cutSceneIsHappening = false;
        }
    }
}
