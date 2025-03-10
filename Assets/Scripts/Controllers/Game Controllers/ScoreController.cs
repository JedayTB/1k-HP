using System.Collections;
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
    Coroutine ScoreTextCoroutine;

    Coroutine AccumFadeOutCoroutine;
    private static float MaxAccumulatedScore = 5000f;
    // Extra logic fn's

    ExtraFadeoutLogic accumFadeoutLn;
    [Header("Action Score Delta's")]
    [SerializeField] private float NitroScoreDelta = 1000f;
    [SerializeField] private float DriftingScoreDelta = 300f;


    [Header("Gradients")]
    [SerializeField] private Gradient ScoreAdditionGradient;
    CustomCarPhysics playerPhysRef;

    [Header("Sizing")]
    [SerializeField] private float LowestSize = 0.5f;
    [SerializeField] private float HighestSize = 3f;

    [Header("Text Shake")]
    [SerializeField] private float lowestIntensity = 1f;
    void Start()
    {
        playerPhysRef = GameStateManager.Player.VehiclePhysics;
        accumFadeoutLn = setAccumScoreZero;
    }
    void setAccumScoreZero()
    {
        AccumulatedScore = 0f;
    }
    // Update is called once per frame
    void Update()
    {
    }
    void FixedUpdate()
    {
        if (playerPhysRef.isUsingNitro) AddScore(NitroScoreDelta * Time.fixedDeltaTime);
        else if (playerPhysRef.isDrifting) { AddScore(DriftingScoreDelta * Time.fixedDeltaTime); } 
    }
    private void AddScore(float delta)
    {
        CurrentScore += delta;
        AccumulatedScore += delta;
        ScoreText.text = $"Score: {CurrentScore:00000}";

        AccumulatedScoreText.text = $"+ {AccumulatedScore}";
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
