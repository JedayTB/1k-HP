using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseScale : MonoBehaviour
{
    public void PointerEnter()
    {
        transform.localScale = new Vector2(1.2f, 1.2f);
    }
    public void PointerExit() 
    {
        transform.localScale = new Vector2(1f, 1f);
    }
    public void PointerClick()
    {
        transform.localScale = new Vector2(0.8f, 0.8f);
    }
}
