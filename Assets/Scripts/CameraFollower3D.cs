using UnityEngine;

public class CameraFollower3D : MonoBehaviour
{
    private Transform _transform;
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _desiredLocation;
    [SerializeField] private float smoothSpeed;
    
    private Vector3 _currentVelocity = Vector3.zero;
    

    void Awake()
    {
        _transform = transform;
    }

    void FixedUpdate(){
        Vector3 targetPosition = _desiredLocation.position;
        _transform.position = Vector3.SmoothDamp(_transform.position, targetPosition, ref _currentVelocity, smoothSpeed);
        _transform.LookAt(_target);
    }
}
