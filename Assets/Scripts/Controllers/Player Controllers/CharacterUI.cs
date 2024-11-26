using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum CharacterNames
{
    None,
    Mimi,
    Hoku,
    Ethan,
    Cindy
}

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private GameObject[] _characterButtons;
    [SerializeField] private AnimationClip[] _animations;

    [Space(15)]
    [SerializeField] private CharacterNames _selectedCharacter;
    [SerializeField] private Animator _animator; 

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.KeypadEnter) && _selectedCharacter != CharacterNames.None)
        {
            _animator.SetBool("MenuIsOpen", true);
        }
    }

    public void SelectCharacter(string characterName)
    {

        switch (characterName)
        {
            case "Mimi":
                _selectedCharacter = CharacterNames.Mimi;
                break;
            case "Hoku":
                _selectedCharacter = CharacterNames.Hoku;
                break;
            case "Ethan":
                _selectedCharacter = CharacterNames.Ethan;
                break;
            case "Cindy":
                _selectedCharacter = CharacterNames.Cindy;
                break;
            case null:
                _selectedCharacter = CharacterNames.None;
                break;
        }
    }

    public void CloseMenu()
    {
        _animator.SetBool("MenuIsOpen", false);
    }
}
