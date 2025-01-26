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
  [SerializeField] private RectTransform nextCheckpointCompas;

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
  // Rect transforms 



  // Misc
  [SerializeField] private KeyCode _pauseMenuKey = KeyCode.Escape;
  [SerializeField] private float abilityCircleAnimTime = 0.75f;

  private float nextCheckpointAngle;
  private bool _menuIsOpen = false;

  public Image lightningCrossHair;
  public Image HookshotCrosshair;

  private Vector3 cachedLocation;
  private float maxSpeed = 320f;
  private float rotationDif = 194;

  public void init(PlayerVehicleController PLAYER)
  {
    _player = PLAYER;
    _playerNitroSlider.maxValue = _player.MaxNitroChargeAmounts;
    _builtUpNitroSlider.maxValue = 1f;
    cachedLocation = _player.transform.position;

    StartCoroutine(CountDown(3));
  }
  void Update()
  {
    if (Input.GetKey(KeyCode.O))
    {
      Time.timeScale = 5f;
    }
    else if (Input.GetKey(KeyCode.P))
    {
      Time.timeScale = 1f;
    }
    if (Input.GetKeyUp(_pauseMenuKey))
    {
      menuOpenClose();
    }

    if (_player.isDrifting)
    {
      _builtUpNitroSlider.value = _player._nitroIncrementThresholdValue;
    }

    if (GameStateManager.Instance.levelCheckpointLocations.Length != 0)
    {
      AdjustAngleToCheckpoint();
    }
    rotateSpeedometreLine();

    _builtUpNitroSlider.gameObject.SetActive(_player.isDrifting);
    _playerNitroSlider.value = LerpAndEasings.ExponentialDecay(_playerNitroSlider.value, _player._nitroChargeAmounts, _sliderLerpSpeed, Time.deltaTime);
    GearText.text = _player.VehiclePhysics.gearText;
  }
  private void AdjustAngleToCheckpoint()
  {

    int index = GameStateManager.Instance.nextPlayerCheckpointPosition;
    // Effectively forward facing angle
    float playerYRot = _player.transform.rotation.eulerAngles.y;
    Vector3 target = GameStateManager.Instance.levelCheckpointLocations[index];

    Vector3 playerToNextCheckpointDir = target - _player.transform.forward;
    playerToNextCheckpointDir.Normalize();

    // Use X and Z values because we're in 3d!;
    nextCheckpointAngle = Mathf.Rad2Deg * Mathf.Atan2(playerToNextCheckpointDir.x, playerToNextCheckpointDir.z) - 90f;

    nextCheckpointAngle = Mathf.DeltaAngle(nextCheckpointAngle, playerYRot);

    nextCheckpointCompas.transform.rotation = Quaternion.Euler(0, 0, nextCheckpointAngle);

    debugStr = $"Dir to angle {nextCheckpointAngle} \nplayerYRot {playerYRot}";
        print(target);
    /*
       int index = GameStateManager.Instance.nextPlayerCheckpointPosition;
      // Effectively forward facing angle
      Vector3 plForward = _player.transform.forward;
      Vector3 target = GameStateManager.Instance.levelCheckpointLocations[index];

      nextCheckpointAngle = Vector3.SignedAngle(plForward, target, Vector3.up);

      nextCheckpointCompas.transform.rotation = Quaternion.Euler(0, 0, nextCheckpointAngle);
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
    Debug.LogWarning("Configure to use Input System instead of keycode");

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
    float count = time;
    //countdownTimerSound.Play();

    while (PreRaceCamera.cutSceneIsHappening) // the worst thing i've ever written what a bandaid fix im sorry im gonna kill myself
        {
            yield return null;
        }

        _countdownText.alpha = 1;

    while (count > 0)
    {
      count -= Time.deltaTime;
      _countdownText.text = (count + 0.5f).ToString("0");

      if (count < 2 && count > 1) // highschool ass if statement man what am i doing
      {
        _countdownText.fontSize = 100;
      }
      else if (count <= 1)
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
    Vector3 startScale = new(0.25f, 0.25f, 0.25f);
    Vector3 endScale = Vector3.one;
    StartCoroutine(abilityGainedAnim(startScale, Vector3.one));
  }
  public void playerUsedAbility(addedAbility abilityUsed)
  {
    Vector3 startScale = Vector3.one;
    Vector3 endScale = new(0.25f, 0.25f, 0.25f);
    StartCoroutine(abilityUsedAnim(abilityUsed, startScale, endScale));
  }
  IEnumerator abilityGainedAnim(Vector3 startScale, Vector3 endScale)
  {
    float timeCount = 0f;
    float progress;

    abilityCircle.gameObject.SetActive(true);
    abilityCircle.transform.localScale = startScale;

    while (timeCount < abilityCircleAnimTime)
    {
      timeCount += Time.deltaTime;
      progress = circleAnimCurve.Evaluate(timeCount / abilityCircleAnimTime);

      abilityCircle.transform.localScale = Vector3.LerpUnclamped(startScale, endScale, progress);
      yield return null;
    }
  }

  IEnumerator abilityUsedAnim(addedAbility abilityUsed, Vector3 startScale, Vector3 endScale)
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

    SpritesOnAbility(abilityUsed, false);
    abilityCircle.gameObject.SetActive(true);
  }
  private void SpritesOnAbility(addedAbility ability, bool val)
  {
    // Reset sprites
    bubbleGumSprite.gameObject.SetActive(false);
    chilliOilSprite.gameObject.SetActive(false);
    lightningSprite.gameObject.SetActive(false);
    hookshotSprite.gameObject.SetActive(false);

    switch (ability)
    {
      case addedAbility.Bubblegum:
        bubbleGumSprite.gameObject.SetActive(val);
        break;
      case addedAbility.ChilliOil:
        chilliOilSprite.gameObject.SetActive(val);
        break;
      case addedAbility.Hookshot:
        hookshotSprite.gameObject.SetActive(val);
        break;
      case addedAbility.Lightning:
        lightningSprite.gameObject.SetActive(val);
        break;
    }
  }
}
