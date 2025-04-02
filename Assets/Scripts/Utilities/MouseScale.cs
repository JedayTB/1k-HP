using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseScale : MonoBehaviour
{
    Vector3 baseScale = Vector3.one; 

    private void Start()
    {
        baseScale = transform.localScale;
    }
    public void PointerEnter()
    {
        transform.localScale = baseScale * 1.2f;
    }
    public void PointerExit() 
    {
        transform.localScale = baseScale;
    }
    public void PointerClick()
    {
        transform.localScale = baseScale * 0.8f;
    }
}
