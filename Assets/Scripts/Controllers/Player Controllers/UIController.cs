using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private PlayerVehicleController _player;
    [SerializeField] private GameObject _pauseMenu;

    [SerializeField] private CanvasGroup _playMenu;
    [SerializeField] private CanvasGroup _winMenu;

    [SerializeField] private KeyCode _pauseMenuKey = KeyCode.Escape;
    [SerializeField] private Slider _playerNitroSlider;
    [SerializeField] private Slider _builtUpNitroSlider;
    private bool _menuIsOpen = false;

    public void init(PlayerVehicleController PLAYER){
        _player = PLAYER; 
        _playerNitroSlider.maxValue = _player.MaxNitroChargeAmounts;
        _builtUpNitroSlider.maxValue = 1f;
    }
    void Update()
    {
        if (Input.GetKeyUp(_pauseMenuKey))
        {
            menuOpenClose();
        }
        _builtUpNitroSlider.gameObject.SetActive(_player.isDrifting);
        if(_player.isDrifting){
            _builtUpNitroSlider.value = _player._nitroIncrementThresholdValue;
        }
        _playerNitroSlider.value = _player._nitroChargeAmounts;
    }

    public void setPlayScreen(bool val){
        _playMenu.gameObject.SetActive(val);
    }
    public void setWinScreen(bool val){
        _winMenu.gameObject.SetActive(val);
    }

    private void menuOpenClose()
    {
        if (_menuIsOpen)
        {
            _pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (!_menuIsOpen)
        {
            _pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        _menuIsOpen = !_menuIsOpen;
    }

    public void resetPlayer()
    {
        GameStateManager.Player.respawn();
        menuOpenClose();
    }

    public void continueButton()
    {
        menuOpenClose(); // pretty sure this will always close and never cause problems
    }
}
