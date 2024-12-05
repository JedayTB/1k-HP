using System.Collections;
using UnityEngine;

[RequireComponent (typeof(BoxCollider))]
public class Collectables : MonoBehaviour
{   
    [SerializeField] protected float _timeToRespawn = 5f;
    [SerializeField] protected Collider _collider;
    [SerializeField] protected Renderer _renderer;
    void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _collider.isTrigger = true;
        _renderer = GetComponentInChildren<Renderer>();
    }
    public virtual void onPickup(A_VehicleController vehicle)
    {
        Debug.Log($"{this.name} got picked up. Disabling object");
        StartCoroutine(respawnClock());
    }
    protected virtual IEnumerator respawnClock(){
        float count = 0f;

        _renderer.enabled = false;
        _collider.enabled = false;
        while(count < _timeToRespawn){
            count += Time.deltaTime;
            yield return null;
        }
        _collider.enabled = true;
        _renderer.enabled = true;
    }
    
}
