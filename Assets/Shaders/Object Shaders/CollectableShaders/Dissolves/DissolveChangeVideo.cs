using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
[RequireComponent(typeof(VideoPlayer))]
public class DissolveChangeVideo : MonoBehaviour
{
    /// <summary>Material used to display the videos.</summary>
    private Material material;

    /// <summary>Video Player that plays each video clip.</summary>
    private VideoPlayer videoPlayer;

    /// <summary>Integer hash for the shader property '_Clip'</summary>
    private static readonly int _Clip = Shader.PropertyToID("_Clip");

    [Tooltip("Time needed to dissolve the model.")]
    [SerializeField] private float dissolveTime = 3f;

    [Tooltip("Second Video Clip to show after dissolving away.")]
    [SerializeField] private VideoClip nextClip;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
        videoPlayer = GetComponent<VideoPlayer>();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            StartCoroutine(ChangeVideoClip(duration: dissolveTime));
        }
    }

    private IEnumerator ChangeVideoClip(float duration)
    {
        yield return FadeOut(duration);

        videoPlayer.clip = nextClip;

        yield return FadeIn(duration);
    }

    private IEnumerator FadeOut(float duration)
    {
        for (float t = 0f; t <= 1f; t += Time.deltaTime / duration)
        {
            material.SetFloat(_Clip, t);
            yield return null;
        }

        material.SetFloat(_Clip, 1f);
    }

    private IEnumerator FadeIn(float duration)
    {
        for (float t = 0f; t <= 1f; t += Time.deltaTime / duration)
        {
            material.SetFloat(_Clip, 1 - t);
            yield return null;
        }

        material.SetFloat(_Clip, 0f);
    }
}
