using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Renderer))]
public class Dissolve : MonoBehaviour
{
    /// <summary>Material used for this model.</summary>
    private Material material;

    /// <summary>Integer hash for the shader property '_Clip'</summary>
    private static readonly int _Clip = Shader.PropertyToID("_Clip");

    [Tooltip("Time needed to dissolve the model.")]
    [SerializeField] private float dissolveTime = 3f;

    private void Awake()
    {
        material = GetComponent<Renderer>().material;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.M))
        {
            StartCoroutine(FadeOut(duration: dissolveTime));
        }
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
}
