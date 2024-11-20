using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// shit ass script i'm sorry ethan
public class CameraRotaterThing : MonoBehaviour
{
    [SerializeField] private float _rotationSpeed = 0.15f;

    private void FixedUpdate()
    {
        Quaternion newRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + _rotationSpeed, transform.rotation.eulerAngles.z);
        transform.rotation = newRotation;
    }
}
