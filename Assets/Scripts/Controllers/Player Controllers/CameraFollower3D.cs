using UnityEngine;

public class CameraFollower3D : MonoBehaviour
{
    private Transform _transform;
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _desiredLocation;
    [SerializeField] private Transform _pivot;
    [SerializeField] private float smoothSpeed = 0.25f;
    [SerializeField] private float _sensitivity = 1f;
    [SerializeField] private KeyCode _rearViewKey = KeyCode.Tab;

    private Vector3 _currentVelocity = Vector3.zero;
    private float _horizontalInput;
    private float _verticalInput;

    public float rotated;

    void Awake()
    {
        _transform = transform;
        _pivot.transform.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        checkForInput();
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

    void FixedUpdate(){
        ///
        /// vector3 currentEulerrotation = camera.rot
        /// rottate code here 
        /// currentEulerrotation.y = Mathf.clamp(y, -35, 35);
        /// camer.rot = currentEul
        ///

        Vector3 currentEulerRotation = _pivot.transform.localRotation.eulerAngles;
        print(currentEulerRotation);

        currentEulerRotation.y += -_horizontalInput * _sensitivity;

        _pivot.transform.localRotation = Quaternion.Euler(currentEulerRotation);


        Vector3 targetPosition = _desiredLocation.position;

        _transform.position = Vector3.SmoothDamp(_transform.position, targetPosition, ref _currentVelocity, smoothSpeed);

        _transform.LookAt(_target);



        // We need to change the rotation of the Z to the car's Z after the LookAt, or else it gets overridden
        Quaternion newRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, _target.rotation.eulerAngles.z);
        _transform.rotation = newRotation;

    }
}
