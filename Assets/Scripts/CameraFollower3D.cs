using UnityEngine;

public class CameraFollower3D : MonoBehaviour
{
    private Transform _transform;
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _desiredLocation;
    [SerializeField] private float smoothSpeed;
    [SerializeField] private KeyCode _rearViewKey = KeyCode.Tab;
    
    private Vector3 _currentVelocity = Vector3.zero;

    void Awake()
    {
        _transform = transform;
    }

    private void Update()
    {
        checkForInput();
    }

    private void checkForInput()
    {
        float horizontalInput = Input.GetAxisRaw("Mouse X");
        float verticalInput = Input.GetAxisRaw("Mouse Y");

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

    void FixedUpdate(){
        Vector3 targetPosition = _desiredLocation.position;

        _transform.position = Vector3.SmoothDamp(_transform.position, targetPosition, ref _currentVelocity, smoothSpeed);

        _transform.LookAt(_target);

        // We need to change the rotation of the Z to the car's Z after the LookAt, or else it gets overridden
        Quaternion newRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, _target.rotation.eulerAngles.z);
        _transform.rotation = newRotation;

    }
}
