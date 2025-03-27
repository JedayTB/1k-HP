using UnityEngine;
using TMPro;

public enum CharacterNames
{
  None,
  AzureAce,
  Chopper,
  SunScorch,
  Cindy
}

public class CharacterUI : MonoBehaviour
{
  [SerializeField] private CharacterNames _selectedCharacter = CharacterNames.AzureAce;
  [SerializeField] private Animator _animator;
  [SerializeField] private KeyCode _confirmKey;
  [SerializeField] private KeyCode _cancelKey;
  [SerializeField] private TextMeshProUGUI _confirmCharText;
  [SerializeField] private TextMeshProUGUI _selectedCharacterText;

  [Space(15)]
  [SerializeField] private GameObject[] _characterButtons;
  [SerializeField] private AnimationClip[] _animations;
  [SerializeField] private SceneChanger _sceneChanger;

  InputManager inputManager;

  // Start is called before the first frame update
  void Awake()
  {
    string yuh = _selectedCharacter.ToString();
    setSelectedCharacterText("Azure Ace");
    CursorController.setDefaultCursorConfined();
    inputManager = GetComponent<InputManager>();

    //_selectedCharacterText.text = "Null";
    //_selectedCharacterText.gameObject.SetActive(false);
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(_confirmKey) && _selectedCharacter != CharacterNames.None)
    {
      ConfirmButton();
    }

    if (inputManager.pressedDeny)
    {
      _sceneChanger.LoadLevelWithTransition("MainMenu");
    }

  }
  public void setSelectedCharacterText(string characterName)
  {
    _selectedCharacterText.gameObject.SetActive(true);

    _selectedCharacterText.text = $"Select {characterName}?";
  }
  public void SelectCharacter(string characterName)
  {

    switch (characterName)
    {
      case "Mimi":
        _selectedCharacter = CharacterNames.AzureAce;
        break;
      case "Hoku":
        _selectedCharacter = CharacterNames.Chopper;
        break;
      case "Ethan":
        _selectedCharacter = CharacterNames.SunScorch;
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

  public void ConfirmButton()
  {
    GameStateManager._newCharacter = ((int)_selectedCharacter) - 1;

    if (GrandPrixManager.GameMode == 0)
    {
      _sceneChanger.LoadLevelWithTransition(GrandPrixManager.LevelOrder[GrandPrixManager.CurrentLevelIndex]);
    }
    if (GrandPrixManager.GameMode == 1)
    {
      _sceneChanger.LoadLevelWithTransition("CityLevel");
    }

  }

  public void BackButton(string text)
  {

  }
}

