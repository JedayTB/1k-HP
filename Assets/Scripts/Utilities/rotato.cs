using UnityEngine;

public class rotato : MonoBehaviour
{
    [SerializeField] private Vector3 _rotationAxis;

    void Update()
    {
        transform.Rotate(_rotationAxis * Time.deltaTime);
    }
}
