using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LensDistortionController : MonoBehaviour
{
    private static readonly int _lensSize = Shader.PropertyToID("_CIrcleMaskSize");
    private static readonly int _distortionIntensity = Shader.PropertyToID("_LensDistortionStrength");

    [SerializeField] private float _threshold = 150f;

    [SerializeField] private Material lensDistortionMat;

    private void Awake()
    {    
        lensDistortionMat.SetFloat(_lensSize, 2f);
        lensDistortionMat.SetFloat(_distortionIntensity, 0f);
    }

    private void Update()
    {
        if(GameStateManager.Player.VehiclePhysics.getVelocity() > _threshold)
        { 
            lensDistortionMat.SetFloat(_distortionIntensity, -0.15f);
        }
        else
        {
            lensDistortionMat.SetFloat(_distortionIntensity, 0f);
        }
    }

}
