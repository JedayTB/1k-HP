using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

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

    public void StartButton()
    {
        animator.SetBool("MenuIsOpen", true);
    }

    public void OptionsButton()
    {
        // no options menu implemented yet
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void CancelButton()
    {
        animator.SetBool("MenuIsOpen", false);
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
        // Video
        presetDropdown.value = PlayerPrefs.GetInt("S-V-Preset");
        aaDropdown.value = PlayerPrefs.GetInt("S-V-AA");
        fpsDropdown.value = PlayerPrefs.GetInt("S-V-FPS");
        vfxDropdown.value = PlayerPrefs.GetInt("S-V-VFX");

        // Audio
        masterVolumeSlider.value = PlayerPrefs.GetFloat("S-A-Master");
        musicVolumeSlider.value = PlayerPrefs.GetFloat("S-A-Music");
        sfxVolumeSlider.value = PlayerPrefs.GetFloat("S-A-SFX");

        ApplyAllSettings();
    }

    public void ApplyAllSettings()
    {
        int[] aaLevels = { 0, 2, 4, 8 };
        int[] fpsLevels = { 30, 60, 120, 144, 240, 0 };

        QualitySettings.antiAliasing = aaLevels[aaDropdown.value];

        QualitySettings.SetQualityLevel(presetDropdown.value, true);

        Application.targetFrameRate = fpsLevels[fpsDropdown.value];
    }

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
