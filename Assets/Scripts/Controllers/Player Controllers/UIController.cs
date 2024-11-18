using System.Collections;
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
    [SerializeField][Tooltip("Changes how fast the minimap icon moves in relation to the players speed. \nHigher value = slower speed.")] private float _miniScaleDivide = 100f;
    [SerializeField] private float _resetFreezeDuration = 1.5f;

    private Vector3 cachedLocation;

    public void init(PlayerVehicleController PLAYER){
        _player = PLAYER; 
        _playerNitroSlider.maxValue = _player.MaxNitroChargeAmounts;
        _builtUpNitroSlider.maxValue = 1f;
        cachedLocation = _player.transform.position;
    }
    void Update()
    {
        if (Input.GetKeyUp(_pauseMenuKey))
        {
            menuOpenClose();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            resetPlayer();
        }
        _builtUpNitroSlider.gameObject.SetActive(_player.isDrifting);
        if(_player.isDrifting){
            _builtUpNitroSlider.value = _player._nitroIncrementThresholdValue;
        }
        _playerNitroSlider.value = _player._nitroChargeAmounts;

        speedText.text = $"{GameStateManager.Player._vehiclePhysics.getVelocity().ToString("00.00")} km/h" ;

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
        _player._vehiclePhysics.RigidBody.velocity = Vector3.zero;

        Vector3 newPosition = new Vector3(_player.transform.position.x, _player.transform.position.y + 10f, _player.transform.position.z);
        _player.transform.position = newPosition;

        Quaternion newRotation = Quaternion.Euler(0, _player.transform.rotation.eulerAngles.y, 0);
        _player.transform.rotation = newRotation;

        StartCoroutine(FreezeRotation(1.5f));

        //GameStateManager.Player.respawn();
        if (_menuIsOpen)
        {
            menuOpenClose();
        }
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
        // Essentially we are just taking the difference between the current location of the car and the location last frame, and adding that to the location of the minimap icon
        // Vehicle's z rotation is in the y spot for the minimap icon rotation becuase 2D to 3D silliness
        Vector3 newLocation = new Vector3((_player.transform.position.x - cachedLocation.x) / _miniScaleDivide, (_player.transform.position.z - cachedLocation.z) / _miniScaleDivide, 0);
        _miniPlayerPosition.position += newLocation;
        cachedLocation = _player.transform.position;

        // We change the z rotation of the minimap icon according to the vehicles y rotation (because of 2D to 3D shenanigans)
        Quaternion minimapRotation = Quaternion.Euler(0, 0, -_player.transform.rotation.eulerAngles.y);
        _miniPlayerPosition.rotation = minimapRotation;
    }

    public IEnumerator FreezeRotation(float time) // my first coroutine omg are you proud of me :3
    {
        float count = 0;
        _player._vehiclePhysics.RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        while (count < time)
        {
            count += Time.deltaTime;
            yield return null;
        }
        _player._vehiclePhysics.RigidBody.constraints = RigidbodyConstraints.None;
    }
}
