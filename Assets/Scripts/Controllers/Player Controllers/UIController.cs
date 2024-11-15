using System.Collections.Specialized;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private PlayerVehicleController _player;
    [SerializeField] private GameObject _pauseMenu;

    [SerializeField] private CanvasGroup _playMenu;
    [SerializeField] private CanvasGroup _winMenu;
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private KeyCode _pauseMenuKey = KeyCode.Escape;
    [SerializeField] private Slider _playerNitroSlider;
    [SerializeField] private Slider _builtUpNitroSlider;
    [SerializeField] private bool _menuIsOpen = false;
    [SerializeField] private RectTransform _miniPlayerPosition;
    [SerializeField] private float _miniScaleDivide = 100f;

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

        speedText.text = $"{GameStateManager.Player._vehiclePhysics.getVelocity()} km/h" ;

        //miniMap();
    }

    public void setPlayScreen(bool val){
        _playMenu.gameObject.SetActive(val);
    }
    public void setWinScreen(bool val){
        _winMenu.gameObject.SetActive(val);
    }

    private void menuOpenClose()
    {
        if (_menuIsOpen == true)
        {
            _pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
        else if (_menuIsOpen == false)
        {
            _pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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

    private void FixedUpdate()
    {
        miniMap();
    }

    private void miniMap()
    {
        Vector3 playerVelocity = _player._vehiclePhysics.RigidBody.velocity;

        Vector3 minimapUpdate = new Vector3(_miniPlayerPosition.position.x + (playerVelocity.x / _miniScaleDivide * 1.7f), _miniPlayerPosition.position.y + (playerVelocity.z / _miniScaleDivide), _miniPlayerPosition.position.z);
        _miniPlayerPosition.position = minimapUpdate;
    }
}
