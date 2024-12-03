using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Animator animator;

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
}
