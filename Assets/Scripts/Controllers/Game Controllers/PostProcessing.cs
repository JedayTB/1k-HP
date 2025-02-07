using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessing : MonoBehaviour
{
  private MotionBlur motionBlur;
  private ChromaticAberration chromaticAberration;

  private float playerTerminalVelocity;
  private float currentPlayerSpeed;
  private float normalizedPlayerSpeed;

  [Header("Basic setup")]
  public Volume volume;
  [SerializeField] private bool setThresholdInStart = true;
  [SerializeField] private float thresholdForEffects;
  [SerializeField] private float decaySpeed = 4f;

  [Header("Chromatic Abberation Setup")]
  [SerializeField][Range(0, 1)] private float maxChromaticAberration = 1f;
  private float chromaticAbberationSetAmount;


  [Header("Motion Blue Setup")]
  [SerializeField][Range(0, 1)] private float maxMotionBlur = 0.75f;
  [SerializeField][Range(0f, 0.2f)] private float motionBlurClamp = 0.05f;

  private float motionBlurSetAmount;
  public void init()
  {
    playerTerminalVelocity = GameStateManager.Player.VehiclePhysics.TerminalVelocity;
    if (setThresholdInStart == true) thresholdForEffects = GameStateManager.Player.VehiclePhysics.GearOne.MaxSpeed;


    volume.profile.TryGet<MotionBlur>(out motionBlur);
    volume.profile.TryGet<ChromaticAberration>(out chromaticAberration);
    motionBlur.intensity.max = 5f;

  }
  private void Start()
  {
    init();
  }

  void Update()
  {
    currentPlayerSpeed = GameStateManager.Player.VehiclePhysics.getSpeed();
    if (motionBlur != null || chromaticAberration != null)
    {
      if (currentPlayerSpeed > thresholdForEffects)
      {
        // Extra bit at the end is cuz...
        // past threshold (lets say 80), the value is already above 0. 
        // So, to normalize the value, just add the minimum to get a lower value 
        normalizedPlayerSpeed = Mathf.Clamp01(currentPlayerSpeed / playerTerminalVelocity + thresholdForEffects);

        chromaticAbberationSetAmount = LerpAndEasings.ExponentialDecay(chromaticAbberationSetAmount, maxChromaticAberration * normalizedPlayerSpeed, decaySpeed, Time.deltaTime);
        motionBlurSetAmount = LerpAndEasings.ExponentialDecay(motionBlurSetAmount, maxMotionBlur * normalizedPlayerSpeed, decaySpeed, Time.deltaTime);

        chromaticAberration.intensity.value = chromaticAbberationSetAmount;
        motionBlur.intensity.value = motionBlurSetAmount;
      }
      else
      {
        chromaticAberration.intensity.value = 0f;
        motionBlur.intensity.value = 0f;
      }
    }
  }
  void OnDisable()
  {
    chromaticAberration.intensity.value = 0f;
    motionBlur.intensity.value = 0f;
  }
}
