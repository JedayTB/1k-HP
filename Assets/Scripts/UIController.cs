using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameStateManager _gameStateManager;
    [SerializeField] private GameObject _pauseMenu;

    [SerializeField] private KeyCode _pauseMenuKey = KeyCode.Escape;
    [SerializeField] private Slider _playerNitroSlider;
    private bool _menuIsOpen = false;

    void Update()
    {
        if (Input.GetKeyUp(_pauseMenuKey))
        {
            menuOpenClose();
        }
        
        _playerNitroSlider.value = _gameStateManager._player._nitroAmount;
    }

    private void menuOpenClose()
    {
        if (_menuIsOpen)
        {
            _pauseMenu.SetActive(false);
        }
        else if (!_menuIsOpen)
        {
            _pauseMenu.SetActive(true);
        }

        _menuIsOpen = !_menuIsOpen;
    }

    public void resetPlayer()
    {
        _gameStateManager._player.respawn();
        menuOpenClose();
    }
}
