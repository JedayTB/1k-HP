using UnityEngine;

public class LensDistortionController : MonoBehaviour
{
  #region  readonly
  private static readonly int _lensSize = Shader.PropertyToID("_CIrcleMaskSize");
  private static readonly int _distortionIntensity = Shader.PropertyToID("_LensDistortionStrength");
  private readonly float MaxCircleClipSize = 2f;
  private readonly float MinCircleClipSize = 2f;
  [SerializeField][Range(0, -.15f)] private float MinLensDistortionStrength = 0f;
  [SerializeField][Range(0, -.15f)] private float MaxLensDistortionStrength = -0.9f;
  #endregion

  private static float _maxDistortionSpeed = 350f;
  private static float _minSpeedForDistortion = 80f;

  [SerializeField] private Material lensDistortionMat;

  float distortionMultiplier;
  float playerVelocity;
  float effectiveLensDistortion;

  private void Awake()
  {
    lensDistortionMat.SetFloat(_lensSize, 2f);
    lensDistortionMat.SetFloat(_distortionIntensity, 0f);
  }
  private void OnDisable()
  {
    lensDistortionMat.SetFloat(_lensSize, 2f);
    lensDistortionMat.SetFloat(_distortionIntensity, 0f);
  }
  private void Update()
  {
    playerVelocity = GameStateManager.Player.VehiclePhysics.getSpeed();

    if (playerVelocity > _minSpeedForDistortion)
    {
      distortionMultiplier = Mathf.Clamp01((_minSpeedForDistortion - playerVelocity) / (_minSpeedForDistortion - _maxDistortionSpeed));
      //Debug.Log($"Lens distortion progress {distortionMultiplier}");

      effectiveLensDistortion = MaxLensDistortionStrength * distortionMultiplier;

      lensDistortionMat.SetFloat(_distortionIntensity, effectiveLensDistortion);

    }
    else
    {
      lensDistortionMat.SetFloat(_distortionIntensity, 0f);
    }
  }


}
