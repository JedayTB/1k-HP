using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessing : MonoBehaviour
{
  public bool isForGamePlay;
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
    if (setThresholdInStart == true) thresholdForEffects = GameStateManager.Player.VehiclePhysics.GearOne.MaxSpeed - 15f;


    volume.profile.TryGet(out motionBlur);
    volume.profile.TryGet(out chromaticAberration);

    motionBlur.intensity.max = 5f;

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
