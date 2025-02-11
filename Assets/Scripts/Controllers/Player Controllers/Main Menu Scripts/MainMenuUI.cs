using TMPro;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    [Header("Video Settings Dropdowns")]
    [SerializeField]
    private TMP_Dropdown presetDropdown;

    [SerializeField]
    private TMP_Dropdown textureDropdown,
        aaDropdown,
        fpsDropdown;

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

    #region Video Settings

    public void ChangePreset() { }

    public void ChangeTexture() { }

    public void ChangeAA()
    {
        int[] aaLevels = { 0, 2, 4, 8 };
        QualitySettings.SetQualityLevel(aaLevels[aaDropdown.value], true);
    }

    public void ChangeFPS()
    {
        int[] fpsLevels = { 30, 60, 120, 144, 240, 0 };
        Application.targetFrameRate = fpsLevels[fpsDropdown.value];
    }

    #endregion

    #region Audio Settings

    #endregion
}
