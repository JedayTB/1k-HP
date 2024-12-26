using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
  public string debugStr;
  private PlayerVehicleController _player;
  [SerializeField] private GameObject _pauseMenu;
  // Menu's
  [SerializeField] private CanvasGroup _playMenu;
  [SerializeField] private CanvasGroup _winMenuGP;
  [SerializeField] private CanvasGroup _winMenuLS;

  // Text
  [SerializeField] private TextMeshProUGUI _nextMapName;
  [SerializeField] private TextMeshProUGUI _countdownText;
  [SerializeField] private TextMeshProUGUI GearText;
  // Rect transforms 
  [SerializeField] private RectTransform _spedometerLinePivot;
  [SerializeField] private RectTransform nextCheckpointCompas;
  // Slider's 
  [SerializeField] private Slider _playerNitroSlider;
  [SerializeField] private Slider _builtUpNitroSlider;
  [SerializeField] private Slider _AbilityGaugeSlider;
  // Misc
  [SerializeField] private KeyCode _pauseMenuKey = KeyCode.Escape;

  private float nextCheckpointAngle;
  private bool _menuIsOpen = false;
  [SerializeField] private float _resetFreezeDuration = 1.5f;

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
    if (Input.GetKeyUp(_pauseMenuKey))
    {
      menuOpenClose();
    }

    if (_player.isDrifting)
    {
      _builtUpNitroSlider.value = _player._nitroIncrementThresholdValue;
    }
    if (_player._abilityGauge > 0)
    {
      _AbilityGaugeSlider.gameObject.SetActive(true);
      _AbilityGaugeSlider.value = _player._abilityGauge;
    }
    else
    {
      _AbilityGaugeSlider.gameObject.SetActive(false);
    }
    if (GameStateManager.Instance.levelCheckpointLocations != null)
    {

      AdjustAngleToCheckpoint();
    }
    rotateSpeedometreLine();

    _builtUpNitroSlider.gameObject.SetActive(_player.isDrifting);
    _playerNitroSlider.value = _player._nitroChargeAmounts;
    GearText.text = _player.VehiclePhysics.gearText;
  }
  private void AdjustAngleToCheckpoint()
  {

    int index = GameStateManager.Instance.nextPlayerCheckpointPosition;
    // Effectively forward facing angle
    float playerYRot = _player.transform.rotation.eulerAngles.y;

    Vector3 playerToNextCheckpointDir = GameStateManager.Instance.levelCheckpointLocations[index] - _player.transform.position;
    playerToNextCheckpointDir.Normalize();

    // Use X and Z values because we're in 3d!;
    nextCheckpointAngle = Mathf.Rad2Deg * Mathf.Atan2(playerToNextCheckpointDir.z, playerToNextCheckpointDir.x) - 90f;

    nextCheckpointAngle = Mathf.DeltaAngle(nextCheckpointAngle, playerYRot);

    nextCheckpointCompas.transform.rotation = Quaternion.Euler(0, 0, nextCheckpointAngle);
    debugStr = $"Dir to angle {nextCheckpointAngle} \nplayerYRot {playerYRot}";
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

    print("what is happenijng");

    while (count < time)
    {
      print("please");
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
    float count = time;

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
  }
}
