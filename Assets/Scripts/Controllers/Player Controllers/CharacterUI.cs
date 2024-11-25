using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CharacterNames
{
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            _animator.SetTrigger("OpenMenu");
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
                _selectedCharacter = CharacterNames.Mimi;
                break;
        }
    }
}
