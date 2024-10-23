using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameStateManager _gameStateManager;
    [SerializeField] private GameObject _pauseMenu;

    [SerializeField] private KeyCode _pauseMenuKey = KeyCode.Escape;
    [SerializeField] private Slider _playerNitroSlider;
    private bool _menuIsOpen = false;

    void Start()
    {
        init();
    }
    public void init(){
        _playerNitroSlider.maxValue = _gameStateManager.player.MaxNitroChargeAmounts;
    }
    void Update()
    {
        if (Input.GetKeyUp(_pauseMenuKey))
        {
            menuOpenClose();
        }
        _playerNitroSlider.value = _gameStateManager.player._builtUpNitroAmount;
    }

    private void menuOpenClose()
    {
        if (_menuIsOpen)
        {
            _pauseMenu.SetActive(false);
            Time.timeScale = 1f;
        }
        else if (!_menuIsOpen)
        {
            _pauseMenu.SetActive(true);
            Time.timeScale = 0f;
        }

        _menuIsOpen = !_menuIsOpen;
    }

    public void resetPlayer()
    {
        _gameStateManager.player.respawn();
        menuOpenClose();
    }

    public void continueButton()
    {
        menuOpenClose(); // pretty sure this will always close and never cause problems
    }
}
