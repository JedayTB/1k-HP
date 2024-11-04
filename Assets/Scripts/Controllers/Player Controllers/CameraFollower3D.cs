using TMPro.EditorUtilities;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class CameraFollower3D : MonoBehaviour
{
    private Transform _transform;
    private Camera _camera;
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _desiredLocation;
    [SerializeField] private Transform _pivot;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private PlayerVehicleController _vehicleController;
    [SerializeField] private float smoothSpeed = 0.25f;
    [SerializeField] private float _sensitivity = 1f;
    [SerializeField] private float _maximumRotationX = 35f;
    [SerializeField] private float _maximumRotationY = 15f;
    [SerializeField] private float _timeUntilReset = 1f;
    [SerializeField] private KeyCode _rearViewKey = KeyCode.Tab;
    [SerializeField] private bool _inverseCameraX = false;
    [SerializeField] private bool _inverseCameraY = false;

    private Vector3 _currentVelocity = Vector3.zero;
    private float _horizontalInput;
    private float _verticalInput;
    private float _timeSinceInput;
    private bool _lerping = false;
    private bool _lerpingFloat = false;
    private float _startTime;
    private float _startTimeFloat;
    private Vector3 _startRotation;
    private float _startFloat;

    public float rbVelocity;

    void Awake()
    {
        _transform = transform;
        _camera = GetComponent<Camera>();
        _pivot.transform.localRotation = Quaternion.identity;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        checkForInput();
        ChangeFOV();
    }

    private void checkForInput()
    {
        _horizontalInput = Input.GetAxisRaw("Mouse X");
        _verticalInput = Input.GetAxisRaw("Mouse Y");

        if (Input.GetKeyDown(_rearViewKey))
        {
            _desiredLocation.localPosition = new Vector3(_desiredLocation.localPosition.x, _desiredLocation.localPosition.y, 15f);
            _transform.position = _desiredLocation.position;
            smoothSpeed = 0f;
        }

        // it gets a little messy when resetting the position, since it sets it to the target, then falls behind since the car is fast
        // could keep track of it's non-rear position, then switch to that but idk
        if (Input.GetKeyUp(_rearViewKey))
        {
            _desiredLocation.localPosition = new Vector3(_desiredLocation.localPosition.x, _desiredLocation.localPosition.y, -7.12f);
            _transform.position = _desiredLocation.position;
            smoothSpeed = 0.2f;
        }
    }

    private void ChangeFOV()
    {
        if (_vehicleController._vehiclePhysics.isUsingNitro && _camera.fieldOfView != 80)
        {
            _camera.fieldOfView = lerpFloat(_camera.fieldOfView, 80f);
        }
        else if (!_vehicleController._vehiclePhysics.isUsingNitro && _camera.fieldOfView != 60)
        {
            _camera.fieldOfView = lerpFloat(_camera.fieldOfView, 60f);
        }
    }

    private float lerpFloat(float currentNum, float targetFOV)
    {
        if (!_lerpingFloat)
        {
            _startTime = Time.time;
            _lerpingFloat = true;
            _startFloat = currentNum;
        }

        //float progress = 1 - Mathf.Pow(1 - ((Time.time - _startTimeFloat) - 0.5f), 0.5f);
        float progress = _startTime / Time.time;
        progress = easeInOutQuad(progress);

        currentNum = Mathf.Lerp(_startFloat, targetFOV, progress);

        if (progress >= 0.99f)
        {
            currentNum = targetFOV;
            _lerpingFloat = false;
        }
        print(currentNum);
        return currentNum;
    }

    private Vector3 lerpRotation(Vector3 currentEulerRotation)
    {
        if (!_lerping) // i feel like this should be out but it broke when i did that sooo
        {
            _startTime = Time.time;
            _startRotation = currentEulerRotation;
            _lerping = true;
        }

        float progress = 1 - Mathf.Pow(1 - ((Time.time - _startTime) - 0.5f), 0.5f);
        currentEulerRotation.y = Mathf.Lerp(currentEulerRotation.y, 0, progress);
        currentEulerRotation.x = Mathf.Lerp(currentEulerRotation.x, 0, progress);

        if (progress == 1f)
        {
            _lerping = false;
        }

        return currentEulerRotation;
    }

    float easeInOutQuad(float x){
        return x< 0.5 ? 2 * x* x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2;
    }

void FixedUpdate(){
        ///
        /// vector3 currentEulerrotation = camera.rot
        /// rottate code here 
        /// currentEulerrotation.y = Mathf.clamp(y, -35, 35);
        /// camer.rot = currentEul
        ///

        // If there has been input, make the time since 0, otherwise add to it
        _timeSinceInput = _horizontalInput != 0 ? 0 : _timeSinceInput += Time.deltaTime;
        
        // It gets real angry if you don't do this and try to move while it's lerping
        if (_timeSinceInput < _timeUntilReset)
        {
            _lerping = false;
        }

        Vector3 currentEulerRotation = _pivot.transform.localRotation.eulerAngles;
        _horizontalInput *= _inverseCameraX ? -1 : 1; // oooh my i feel so cool.....
        currentEulerRotation.y += -_horizontalInput * _sensitivity;
        currentEulerRotation.x += _verticalInput * _sensitivity;

        // Stupid work around because rotation resets to 359 degrees instead of -1 when going below 0
        if (currentEulerRotation.y > 180f)
        {
            currentEulerRotation.y -= 360f; // i am SUCH a smart cookie
        }

        if (currentEulerRotation.x > 180f)
        {
            currentEulerRotation.x -= 360f;
        }

        // Lerps as long as there hasn't been enough input and the rotation isn't already 0
        if (_timeSinceInput > _timeUntilReset && currentEulerRotation.y != 0 || currentEulerRotation.x != 0)
        {
            currentEulerRotation = lerpRotation(currentEulerRotation);
        }

        currentEulerRotation.y = Mathf.Clamp(currentEulerRotation.y, -_maximumRotationX, _maximumRotationX);
        currentEulerRotation.x = Mathf.Clamp(currentEulerRotation.x, -_maximumRotationY, _maximumRotationY);

        // Sometimes we get a NaN error here, I did my best to get rid of it but it still shows up sometimes
        // I did manage to make it never (I think) affect gameplay, though
        _pivot.transform.localRotation = Quaternion.Euler(currentEulerRotation);

        Vector3 targetPosition = _desiredLocation.position;

        _transform.position = Vector3.SmoothDamp(_transform.position, targetPosition, ref _currentVelocity, smoothSpeed);
        _transform.LookAt(_target);

        // We need to change the rotation of the Z to the car's Z after the LookAt, or else it gets overridden
        Quaternion newRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, _target.rotation.eulerAngles.z);
        _transform.rotation = newRotation;

        //rbVelocity = _rb.velocity.x + _rb.velocity.z;
        
    }
}
