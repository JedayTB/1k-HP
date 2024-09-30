using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameStateManager _gameStateManager;
    [SerializeField] private GameObject _pauseMenu;

    private bool _menuIsOpen = false;

    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape)) 
        {
            menuOpenClose();
        }
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
