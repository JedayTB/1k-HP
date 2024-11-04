using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITargetFollower : MonoBehaviour
{
    public Transform target;
    public GameObject UIFollower;
    private Camera cam;
    private void Start()
    {
        cam = FindAnyObjectByType<Camera>();
    }
    void Update()
    {
        Vector2 UIMove = cam.WorldToScreenPoint(target.position);   
        UIFollower.transform.position = UIMove;
    }
}
