using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessing : MonoBehaviour
{
    public bool isForGamePlay;
    private MotionBlur motionBlur;
    private ChromaticAberration chromaticAberration;
    private Vignette vignette;

    private float playerTerminalVelocity;
    private float currentPlayerSpeed;
    private float normalizedPlayerSpeed;

    [Header("Basic setup")]
    public Volume volume;
    private static float thresholdForEffects = 50f;
    private static float decaySpeed = 4f;

    [Header("Chromatic Abberation Setup")]
    [SerializeField][Range(0, 1)] private float maxChromaticAberration = 1f;
    [SerializeField] private float chromaticAbberationSetAmount;


    [Header("Motion Blue Setup")]
    [SerializeField][Range(0, 1)] private float maxMotionBlur = 0.75f;
    [SerializeField][Range(0f, 0.2f)] private float motionBlurClamp = 0.05f;

    [Header("Vignette Setup")]
    [SerializeField][Range(0, 0.5f)] private float maxVignette = 0.5f;
    [SerializeField] private float vignetteSetAmount;

    [SerializeField] private float motionBlurSetAmount;
    public void init()
    {
        playerTerminalVelocity = GameStateManager.Player.VehiclePhysics.GearTwo.MaxSpeed - 20f;

        volume.profile.TryGet(out motionBlur);
        volume.profile.TryGet(out chromaticAberration);
        volume.profile.TryGet(out vignette);

        if (motionBlur == null) Debug.LogError("Motion Blur not Obtained");
        if (chromaticAberration == null) Debug.LogError("Chromatic Aberration not obtained");
        if (vignette == null) Debug.LogError("Vignette not obtainted");

        motionBlur.intensity.max = 5f;
        vignette.intensity.max = 0.5f;

    }

    void Update()
    {
        if (isForGamePlay == true)
        {
            currentPlayerSpeed = GameStateManager.Player.VehiclePhysics.getSpeed();


            if (currentPlayerSpeed > thresholdForEffects)
            {
                // Extra bit at the end is cuz...
                // past threshold (lets say 80), the value is already above 0. 
                // So, to normalize the value, just add the minimum to get a lower value 
                normalizedPlayerSpeed = Mathf.Clamp01((thresholdForEffects - currentPlayerSpeed) / (thresholdForEffects - playerTerminalVelocity));
                //normalizedPlayerSpeed = currentPlayerSpeed / 50f;
                //        Debug.Log($"Post processing progress {normalizedPlayerSpeed}");
                chromaticAbberationSetAmount = LerpAndEasings.ExponentialDecay(chromaticAbberationSetAmount, maxChromaticAberration * normalizedPlayerSpeed, decaySpeed, Time.deltaTime);
                motionBlurSetAmount = LerpAndEasings.ExponentialDecay(motionBlurSetAmount, maxMotionBlur * normalizedPlayerSpeed, decaySpeed, Time.deltaTime);


                float setVignetteAmount = Mathf.Lerp(0f, maxVignette, normalizedPlayerSpeed);
                vignetteSetAmount = LerpAndEasings.ExponentialDecay(vignetteSetAmount, setVignetteAmount, decaySpeed, Time.deltaTime);

                chromaticAberration.intensity.value = chromaticAbberationSetAmount;
                motionBlur.intensity.value = motionBlurSetAmount;
                vignette.intensity.value = vignetteSetAmount;
            }
            else
            {
                chromaticAberration.intensity.value = 0f;
                motionBlur.intensity.value = 0f;
                vignette.intensity.value = 0f;
            }
        }
    }
    void OnDisable()
    {
        if (isForGamePlay == true)
        {
            chromaticAberration.intensity.value = 0f;
            motionBlur.intensity.value = 0f;
            vignette.intensity.value = 0f;
        }

    }
}
