using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessing : MonoBehaviour
{
    public Volume volume;
    private Bloom bloom;
    private MotionBlur motionBlur;
    private LensDistortion lensDistortion; // we need to have a reference to each type of effect :(

    public float intensityChange;

    public CustomCarPhysics _physicsRef;

    public float maxDistortion = -.3f;

    void Start()
    {
        volume.profile.TryGet(out bloom); // gotta set it here since we can't do it from editor >:(
        volume.profile.TryGet(out motionBlur);
        volume.profile.TryGet(out lensDistortion);
        motionBlur.intensity.max = 5f;

        
    }

    void Update()
    {
        //bloom.intensity.value = intensityChange;
        //motionBlur.intensity.value = intensityChange;

        float vel = _physicsRef.getVelocity();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (lensDistortion.active)
            {
                lensDistortion.active = false;
            }
            else if (!lensDistortion.active) 
            {
                lensDistortion.active = true;
            }

            print("poopy");
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            lensDistortion.intensity.value -= 0.05f;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            lensDistortion.intensity.value += 0.05f;
        }


        //applyDistortion();
        //clampDistortion();
    }

    private void applyDistortion()
    {
        float carVel = _physicsRef.getVelocity();

        float distortionChange = -(carVel / 250);
        lensDistortion.intensity.value = distortionChange;
    }

    private void clampDistortion()
    {
        if (lensDistortion.intensity.value < maxDistortion)
        {
            lensDistortion.intensity.value = maxDistortion;
        }

        print(lensDistortion.intensity.value);
    }

    private void cameraFOVChange()
    {

    }
}
