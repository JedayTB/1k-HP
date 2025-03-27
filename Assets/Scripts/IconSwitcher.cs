using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum IconState
{
    playMenu, pauseMenu, charSelect
}
public class IconSwitcher : MonoBehaviour
{
    public GameObject parentWithInputManager;




    [Header("Icons")]
    public Image NitroIcon;
    public Image gearShiftIcon;
    public Image acceptIcon;
    public Image denyIcon;
    public Image pauseMenuIcon;

    [Header("Keyboard and mouse Sprites")]
    public Sprite nitroKeyboardSprite;
    public Sprite gearShiftKeyboardSprite;
    public Sprite denyKeyboardSprite;
    public Sprite acceptKeyboardSprite;

    [Header("Controller sprites")]
    public Sprite controllerAcceptSprite;
    public Sprite controllerDenySprite;
    public Sprite controllerNitroSprite;
    public Sprite controllerGearShiftSprite;
    public Sprite controllerStartSprite;

    private void Start()
    {
        var inp = parentWithInputManager.GetComponent<InputManager>();  
        inp.onInputSchemeChanged += switchIcons;
    }
    private void switchIcons(ControlSchemes scheme)
    {
        Sprite nitroSprite = null;
        Sprite gearShiftSprite = null;
        Sprite acceptSprite = null;
        Sprite denySprite = null;
        Sprite pauseSprite = null;

        switch (scheme)
        {
            case ControlSchemes.KeyboardMouse:
                nitroSprite = nitroKeyboardSprite;
                gearShiftSprite = gearShiftKeyboardSprite;
                acceptSprite = acceptKeyboardSprite;
                denySprite = denyKeyboardSprite;
                break;
            case ControlSchemes.Gamepad:
                nitroSprite = controllerNitroSprite;
                gearShiftSprite = controllerGearShiftSprite;
                acceptSprite = controllerAcceptSprite;
                denySprite = controllerDenySprite;
                pauseSprite = controllerStartSprite;
                break;
        }
        if(NitroIcon != null) NitroIcon.sprite = nitroSprite; 
        if(gearShiftIcon != null) gearShiftIcon.sprite = gearShiftSprite;
        if(acceptIcon != null) acceptIcon.sprite = acceptSprite;
        if(denyIcon != null) denyIcon.sprite = denySprite;
        if(pauseMenuIcon != null) pauseMenuIcon.sprite = pauseSprite;
    }
}
