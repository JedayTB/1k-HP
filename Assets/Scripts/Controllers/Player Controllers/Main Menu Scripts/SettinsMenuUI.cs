using System.Runtime.Serialization.Formatters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
  #region Variables


  public TMP_Text gameVersion;

  [Header("General Settings Config")]
  [SerializeField]
  private GameObject pagePreMenu;

  [SerializeField]
  private GameObject pageMainMenu,
      pageVideoSettings,
      pageAudioSettings;

  [Header("Video Settings")]
  [SerializeField]
  private TMP_Dropdown presetDropdown;

  [SerializeField]
  private TMP_Dropdown aaDropdown,
      fpsDropdown,
      vfxDropdown;

  [Header("Audio Settings")]
  [SerializeField]
  private Slider masterVolumeSlider;

  [SerializeField]
  private Slider musicVolumeSlider,
      sfxVolumeSlider;
  #endregion 

  private void Start()
  {
    if(gameVersion != null) gameVersion.text = "ver " + Application.version;
    CursorController.setDefaultCursorConfined();
    LoadAllSettings();
  }

  public void ExitButton()
  {
    Application.Quit();
  }

  private void SaveAllSettings()
  {
    // Video
    PlayerPrefs.SetInt("S-V-Preset", presetDropdown.value);
    PlayerPrefs.SetInt("S-V-AA", aaDropdown.value);
    PlayerPrefs.SetInt("S-V-FPS", fpsDropdown.value);
    PlayerPrefs.SetInt("S-V-VFX", vfxDropdown.value);

    // Audio
    PlayerPrefs.SetFloat("S-A-Master", masterVolumeSlider.value);
    PlayerPrefs.SetFloat("S-A-Music", musicVolumeSlider.value);
    PlayerPrefs.SetFloat("S-A-SFX", sfxVolumeSlider.value);

    ApplyAllSettings();
  }

  private void LoadAllSettings()
  {
    if (
        !PlayerPrefs.HasKey("S-V-Preset")
        || !PlayerPrefs.HasKey("S-V-AA")
        || !PlayerPrefs.HasKey("S-V-FPS")
        || !PlayerPrefs.HasKey("S-V-VFX")
        || !PlayerPrefs.HasKey("S-A-Master")
        || !PlayerPrefs.HasKey("S-A-Music")
        || !PlayerPrefs.HasKey("S-A-SFX")
    )
    {
      // Video
      PlayerPrefs.SetInt("S-V-Preset", 3);
      PlayerPrefs.SetInt("S-V-AA", 1);
      PlayerPrefs.SetInt("S-V-FPS", 6);
      PlayerPrefs.SetInt("S-V-VFX", 0);

      // Audio
      PlayerPrefs.SetFloat("S-A-Master", 1f);
      PlayerPrefs.SetFloat("S-A-Music", 1f);
      PlayerPrefs.SetFloat("S-A-SFX", 1f);
    }

    var preset = PlayerPrefs.GetInt("S-V-Preset");
    var aa = PlayerPrefs.GetInt("S-V-AA");
    var fps = PlayerPrefs.GetInt("S-V-FPS");
    var vfx = PlayerPrefs.GetInt("S-V-VFX");

    var masterVolume = PlayerPrefs.GetFloat("S-A-Master");
    var musicVolume = PlayerPrefs.GetFloat("S-A-Music");
    var sfxVolume = PlayerPrefs.GetFloat("S-A-SFX");

    // Video
    presetDropdown.value = preset;
    aaDropdown.value = aa;
    fpsDropdown.value = fps;
    vfxDropdown.value = vfx;

    // Audio
    masterVolumeSlider.value = masterVolume;
    musicVolumeSlider.value = musicVolume;
    sfxVolumeSlider.value = sfxVolume;

    int[] aaLevels = { 0, 2, 4, 8 };
    int[] fpsLevels = { 30, 60, 120, 144, 240, 0 };

    QualitySettings.antiAliasing = aaLevels[aa];

    QualitySettings.SetQualityLevel(preset, true);

    Application.targetFrameRate = fpsLevels[fps];
  }

  public void ApplyAllSettings()
  {
    int[] aaLevels = { 0, 2, 4, 8 };
    int[] fpsLevels = { 30, 60, 120, 144, 240, 0 };

    QualitySettings.antiAliasing = aaLevels[aaDropdown.value];

    QualitySettings.SetQualityLevel(presetDropdown.value, true);

    Application.targetFrameRate = fpsLevels[fpsDropdown.value];

        GameStateManager.musicVolumeLevel = masterVolumeSlider.value;
        GameStateManager.Instance.UpdateMusicVolume(); // only kind of works but i'm too lazy rn to make it correct
  }

  #region General Menu
  public void openSettingsPrepage()
  {
    pageMainMenu.SetActive(false);
    pagePreMenu.SetActive(true);
    pageVideoSettings.SetActive(false);
    pageAudioSettings.SetActive(false);
    ApplyAllSettings();
    SaveAllSettings();
  }
  public void openVideoSettings()
  {
    pagePreMenu.SetActive(false);
    pageVideoSettings.SetActive(true);
    pageAudioSettings.SetActive(false);
    ApplyAllSettings();
    SaveAllSettings();
  }
  public void openAudioSettings()
  {
    pagePreMenu.SetActive(false);
    pageVideoSettings.SetActive(false);
    pageAudioSettings.SetActive(true);

    ApplyAllSettings();
    
    SaveAllSettings();
  }
  public void backToMain()
  {
    pageMainMenu.SetActive(true);
    pagePreMenu.SetActive(false);
    pageVideoSettings.SetActive(false);
    pageAudioSettings.SetActive(false);
    ApplyAllSettings();
    SaveAllSettings();
  }

  #endregion

  #region Video Settings

  public void ChangeDropdown()
  {
    SaveAllSettings();
  }

  public void ChangeVFX() { }

  #endregion

  #region Audio Settings

  public void ChangeMainVolume() { }

  public void ChangeMusicVolume() { }

  public void ChangeSFXVolume() { }

  #endregion
}
