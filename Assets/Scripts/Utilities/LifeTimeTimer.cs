using UnityEngine;

public class LifeTimeTimer : MonoBehaviour
{
    [SerializeField] float lifetime = 2f;

    void Start()
    {
        GameObject.Destroy(this.gameObject,lifetime);
    }

}
