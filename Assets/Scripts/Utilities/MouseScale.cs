using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseScale : MonoBehaviour
{
    public void PointerEnter()
    {
        transform.localScale = new Vector2(5.626861f, 19.78189f);
    }
    public void PointerExit() 
    {
        transform.localScale = new Vector2(6f, 20f);
    }
    public void PointerClick()
    {
        transform.localScale = new Vector2(5.8f, 19.8f);
    }
}
