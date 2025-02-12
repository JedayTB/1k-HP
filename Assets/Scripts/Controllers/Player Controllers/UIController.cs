using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
  public AudioSource countdownTimerSound;
  public string debugStr;
  private PlayerVehicleController _player;
  [SerializeField] private GameObject _pauseMenu;
  // Menu's
  [Header("Sub-Menu's")]
  [SerializeField] private CanvasGroup _playMenu;
  [SerializeField] private CanvasGroup _winMenuGP;
  [SerializeField] private CanvasGroup _winMenuLS;


  [Header("Rect Transforms")]
  [SerializeField] private RectTransform _spedometerLinePivot;

  //Abbility Specific
  [SerializeField] private RectTransform abilityCircle;
  //
  [SerializeField] private AnimationCurve circleAnimCurve;
  [SerializeField] private RectTransform chilliOilSprite;
  [SerializeField] private RectTransform hookshotSprite;
  [SerializeField] private RectTransform lightningSprite;
  [SerializeField] private RectTransform bubbleGumSprite;

  [Header("Sliders")]
  // Slider's 
  [SerializeField] private Slider _playerNitroSlider;

  [SerializeField] private float _sliderLerpSpeed = 2.5f;
  [SerializeField] private Slider _builtUpNitroSlider;
  // Text
  [Header("Text fields")]
  [SerializeField] private TextMeshProUGUI _nextMapName;
  [SerializeField] private TextMeshProUGUI _countdownText;
  [SerializeField] private TextMeshProUGUI GearText;
  [SerializeField] private TextMeshProUGUI playerPlacementText;


  // Misc
  [SerializeField] private KeyCode _pauseMenuKey = KeyCode.Escape;
  private Vector3 abilityGaugeMinScale = new Vector3(0.01f, 0.01f, 0.01f);
  private Vector3 abilityGaugeMaxScale;
  [SerializeField] private float abilityCircleAnimTime = 0.75f;

  private float nextCheckpointAngle;
  private bool _menuIsOpen = false;

  public Image lightningCrossHair;
  public Image HookshotCrosshair;

  private Vector3 cachedLocation;
  private static float maxSpeed = 200f;
  private static float rotationDif = 220;

  private float counter = 3f;

  public void init(PlayerVehicleController PLAYER)
  {
    _player = PLAYER;
    _playerNitroSlider.maxValue = _player.MaxNitroChargeAmounts;
    _builtUpNitroSlider.maxValue = 1f;
    cachedLocation = _player.transform.position;

    abilityGaugeMaxScale = abilityCircle.transform.localScale;

    StartCoroutine(CountDown(GameStateManager.countdownTime));
  }
  void Update()
  {
    if (Input.GetKeyUp(_pauseMenuKey))
    {
      menuOpenClose();
    }
    if (Input.GetKeyDown(KeyCode.Space) && GameStateManager.Instance.UseDebug)
    {
      counter = 0;
    }

    if (_player.isDrifting) _builtUpNitroSlider.value = _player._nitroIncrementThresholdValue;


    //if (GameStateManager.Instance.levelCheckpointLocations.Length != 0) AdjustAngleToCheckpoint();

    rotateSpeedometreLine();

    _builtUpNitroSlider.gameObject.SetActive(_player.isDrifting);
    _playerNitroSlider.value = LerpAndEasings.ExponentialDecay(_playerNitroSlider.value, _player._nitroChargeAmounts, _sliderLerpSpeed, Time.deltaTime);
    GearText.text = _player.VehiclePhysics.gearText;
    playerPlacementText.text = $"{_player.racePlacement}";
  }
  private void AdjustAngleToCheckpoint()
  {

    int index = GameStateManager.Instance.nextPlayerCheckpointPosition;
    // Effectively forward facing angle
    float playerYRot = _player.transform.rotation.eulerAngles.y;
    Vector3 delta = GameStateManager.Instance.levelCheckpointLocations[index] - _player.transform.position;

    nextCheckpointAngle = -Vector3.Angle(_player.transform.forward, delta);
    //math.degrees(math.acos(dot / _player.transform.position.magnitude * target.magnitude));

    //nextCheckpoint//Compas.transform.rotation = Quaternion.Euler(0, 0, nextCheckpointAngle);
    //
    /*
    if (GameStateManager.Instance.UseDebug)
    {
      Debug.DrawRay(_player.transform.position, delta, Color.white);
      debugStr = $"Dir to angle {nextCheckpointAngle} \nplayerYRot {playerYRot}";
    }
    */
  }

  private void rotateSpeedometreLine()
  {
    if (_spedometerLinePivot != null)
    {
      // Basically just find out how far along the spedometer we are as a percent from 0-1
      // Then multiply the degree difference from 0km to 320km (right now the total diff is 194 degrees) by the percent
      float spedometerPercent = GameStateManager.Player.VehiclePhysics.getSpeed() / maxSpeed;
      float spedometerZRot = -rotationDif * spedometerPercent;
      _spedometerLinePivot.rotation = Quaternion.Euler(0, 0, spedometerZRot);
    }
  }


  public void setPlayScreen(bool val)
  {
    _playMenu.gameObject.SetActive(val);
  }
  public void setWinScreen(bool val, bool isGP)
  {
    Cursor.lockState = CursorLockMode.None;
    Cursor.visible = val;

    if (isGP)
    {
      if (GrandPrixManager.CurrentLevelIndex + 1 > GrandPrixManager.GrandPrixLength)
      {
        _nextMapName.text = "Results";
      }
      else
      {
        _nextMapName.text = GrandPrixManager.LevelDisplayNames[GrandPrixManager.CurrentLevelIndex];
      }

      StartCoroutine(FadeInResults(1.5f, _winMenuGP));
    }
    else
    {
      StartCoroutine(FadeInResults(1.5f, _winMenuLS));
    }
  }

  private void menuOpenClose()
  {
    Debug.LogWarning("Configure Menu to use Input System instead of keycode");

    if (_menuIsOpen == true)
    {
      _pauseMenu.SetActive(false);
      Time.timeScale = 1f;
      Cursor.lockState = CursorLockMode.Locked;
      Cursor.visible = false;
    }
    else if (_menuIsOpen == false)
    {
      _pauseMenu.SetActive(true);
      Time.timeScale = 0f;

      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
    }

    _menuIsOpen = !_menuIsOpen;
  }

  public void resetPlayer()
  {
    _player.respawn();

    if (_menuIsOpen)
    {
      menuOpenClose();
    }
  }

  public void continueButton()
  {
    menuOpenClose(); // pretty sure this will always close and never cause problems
  }


  public IEnumerator FadeInResults(float time, CanvasGroup winScreen)
  {
    float count = 0;
    winScreen.gameObject.SetActive(true);
    winScreen.alpha = 0;


    while (count < time)
    {
      count += Time.deltaTime;
      float progress = count / time;
      winScreen.alpha = progress;

      yield return null;
    }

    winScreen.alpha = 1;
    winScreen.enabled = false;
  }

  public IEnumerator CountDown(float time)
  {
    _playMenu.alpha = 0;
    _countdownText.alpha = 0;
    //countdownTimerSound.Play();

    while (PreRaceCamera.cutSceneIsHappening) // the worst thing i've ever written what a bandaid fix im sorry im gonna kill myself
    {
      yield return null;
    }

    _countdownText.alpha = 1;

    while (counter > 0)
    {
      counter -= Time.deltaTime;
      _countdownText.text = (counter + 0.5f).ToString("0");

      if (counter < 2 && counter > 1) // highschool ass if statement man what am i doing
      {
        _countdownText.fontSize = 100;
      }
      else if (counter <= 1)
      {
        _countdownText.fontSize = 130;
      }
      yield return null;
    }

    _playMenu.alpha = 1;
    _countdownText.gameObject.SetActive(false);
    _player.VehiclePhysics.RigidBody.constraints = RigidbodyConstraints.None;
    GameStateManager.Instance.UnfreezeAIs();
  }

  public void playerGotAbility(addedAbility abilityAdded)
  {
    SpritesOnAbility(abilityAdded, true);
    Vector3 startScale = abilityGaugeMinScale;
    Vector3 endScale = abilityGaugeMaxScale;
    abilityCircle.gameObject.SetActive(true);
    StartCoroutine(abilityAnim(abilityAdded, startScale, endScale, true));
  }
  public void playerUsedAbility(addedAbility abilityUsed)
  {
    Vector3 startScale = abilityGaugeMaxScale;
    Vector3 endScale = abilityGaugeMinScale;

    StartCoroutine(abilityAnim(addedAbility.fucked, startScale, endScale, false));
  }


  IEnumerator abilityAnim(addedAbility abilityUsed, Vector3 startScale, Vector3 endScale, bool enableVal)
  {
    float timeCount = 0f;
    float progress;
    float scaleMultiplier;

    abilityCircle.transform.localScale = startScale;

    while (timeCount < abilityCircleAnimTime)
    {
      timeCount += Time.deltaTime;

      progress = timeCount / abilityCircleAnimTime;
      scaleMultiplier = circleAnimCurve.Evaluate(progress);
      abilityCircle.transform.localScale = Vector3.LerpUnclamped(startScale, endScale, scaleMultiplier);
      yield return null;
    }

    SpritesOnAbility(abilityUsed, enableVal);
    abilityCircle.gameObject.SetActive(enableVal);
  }
  private void SpritesOnAbility(addedAbility ability, bool val)
  {
    // Reset sprites
    bubbleGumSprite.gameObject.SetActive(false);
    lightningSprite.gameObject.SetActive(false);

    switch (ability)
    {
      case addedAbility.Bubblegum:
        bubbleGumSprite.gameObject.SetActive(val);
        break;
      case addedAbility.Lightning:
        lightningSprite.gameObject.SetActive(val);
        break;
    }
  }
}
