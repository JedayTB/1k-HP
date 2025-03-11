using System.Collections;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
public delegate void ExtraFadeoutLogic();
public class ScoreController : MonoBehaviour
{
    public TextMeshProUGUI ScoreText;
    public TextMeshProUGUI AccumulatedScoreText;

    private static float TimeToFadeAway = 5f;
    public float CurrentScore = 0f;
    public float AccumulatedScore = 0f;

    private bool isAccumulating = false;
    // Coroutines
    Coroutine ScoreTextCoroutine = null;
    Coroutine AccumFadeOutCoroutine = null;
    Coroutine ContinuousShake = null;

    [SerializeField] private float MaxAccumulatedScore = 250000f;
    // Extra logic fn's

    ExtraFadeoutLogic accumFadeoutLn;
    [Header("Action Score Delta's")]
    [SerializeField] private float NitroScoreDelta = 1000f;
    [SerializeField] private float DriftingScoreDelta = 300f;

    [SerializeField] private float MaxSpeedScoreMultiplier = 5f;
    [SerializeField] float ScoreSpeedModifier;
    [SerializeField]float NitroScoreModifier;
    [Header("Gradients")]
    [SerializeField] private Gradient ScoreAdditionGradient;
    CustomCarPhysics playerPhysRef;

    [Header("Sizing")]
    [SerializeField] private float LowestSize = 0.5f;
    [SerializeField] private float HighestSize = 2.5f;

    float lowestFontSize;
    float highestFontSize;
    [Header("Text Shake")]

    [SerializeField] private float LowestIntensity = 0f;
    [SerializeField] private float HighestIntensity = 75f;

    [SerializeField] float intensity;

    [SerializeField] private float _xInfluence = 1f;
    [SerializeField] private float _yInfluence = 1f;
    [SerializeField] private float _zInfluence = 1f;


    Vector3 StartPosition;
    void Start()
    {
        playerPhysRef = GameStateManager.Player.VehiclePhysics;
        accumFadeoutLn = setAccumScoreZero;

        float curFontSize = AccumulatedScoreText.fontSize;
        lowestFontSize = curFontSize * LowestSize;
        highestFontSize = curFontSize * HighestSize;

        StartPosition = AccumulatedScoreText.rectTransform.localPosition;

        ContinuousShake = StartCoroutine(ShakeText());
    }
    void setAccumScoreZero()
    {
        AccumulatedScore = 0f;
    }
    // Update is called once per frame
    void Update()
    {

        if (AccumulatedScore != 0 && AccumulatedScoreText.color.a != 0)
        {

            float prog = Mathf.Clamp01(AccumulatedScore / MaxAccumulatedScore);
            float val = Mathf.Lerp(lowestFontSize, highestFontSize, prog);
            float inten = Mathf.Lerp(LowestIntensity, HighestIntensity, prog);

            AccumulatedScoreText.fontSize = LerpAndEasings.ExponentialDecay(AccumulatedScoreText.fontSize, val, 5f, Time.deltaTime);
            intensity = LerpAndEasings.ExponentialDecay(intensity, inten, 5f, Time.deltaTime);
        }
        else
        {
            AccumulatedScoreText.fontSize = 0f;
        }

    }
    private IEnumerator ShakeText()
    {
        while (true)
        {
            float xPos = Random.Range(-(_xInfluence * intensity), _xInfluence * intensity);
            float yPos = Random.Range(-(_yInfluence * intensity), _yInfluence * intensity);

            Vector3 _endLocation = new Vector3(xPos, yPos, 0);
            _endLocation += StartPosition;

            AccumulatedScoreText.rectTransform.localPosition = LerpAndEasings.VexExpoDecay(AccumulatedScoreText.rectTransform.localPosition, _endLocation, 5f, Time.deltaTime);

            yield return null;
        }
    }

    void FixedUpdate()
    {
         ScoreSpeedModifier = Mathf.Lerp(1f, MaxSpeedScoreMultiplier, playerPhysRef.normalizedSpeed);
         NitroScoreModifier = playerPhysRef.isUsingNitro == true ? 3f : 1f;

        if (playerPhysRef.isUsingNitro) AddScore((NitroScoreDelta * Time.fixedDeltaTime) * NitroScoreModifier * ScoreSpeedModifier);
        else if (playerPhysRef.isDrifting)  AddScore((DriftingScoreDelta * Time.fixedDeltaTime) * NitroScoreModifier * ScoreSpeedModifier);
    }
    private void AddScore(float delta)
    {
        CurrentScore += delta;
        AccumulatedScore += delta;
        ScoreText.text = $"Score: {CurrentScore:00000}";
        AccumulatedScoreText.text = $"+ {Mathf.FloorToInt(AccumulatedScore)}";

        float prog = Mathf.Clamp01(AccumulatedScore / MaxAccumulatedScore);
        AccumulatedScoreText.color = ScoreAdditionGradient.Evaluate(prog);

        if (ScoreTextCoroutine != null) StopCoroutine(ScoreTextCoroutine);
        ScoreTextCoroutine = StartCoroutine(FadeOutText(ScoreText, TimeToFadeAway, null));

        if (AccumFadeOutCoroutine != null) StopCoroutine(AccumFadeOutCoroutine);
        AccumFadeOutCoroutine = StartCoroutine(FadeOutText(AccumulatedScoreText, TimeToFadeAway * 0.5f, accumFadeoutLn));
    }
    IEnumerator FadeOutText(TextMeshProUGUI text, float timeToInvisible, ExtraFadeoutLogic exLog)
    {
        float count = 0f;

        Color CurrentTextColor = text.color;
        CurrentTextColor.a = 1f;

        text.color = CurrentTextColor;

        Color endAlphaCol = CurrentTextColor;
        endAlphaCol.a = 0f;

        float progress = 0f;
        while (count < timeToInvisible)
        {
            count += Time.deltaTime;

            progress = count / timeToInvisible;
            text.color = Color.Lerp(CurrentTextColor, endAlphaCol, progress);
            yield return null;
        }

        exLog?.Invoke();

    }
}
