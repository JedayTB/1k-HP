using UnityEngine;

public class CursorController : MonoBehaviour
{
  public static void setNewCursor(Texture2D newCursor)
  {
    Cursor.lockState = CursorLockMode.Confined;
    Cursor.visible = true;
    Cursor.SetCursor(newCursor, Vector2.zero, CursorMode.ForceSoftware);
  }
  public static void setDefaultCursor()
  {
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
  }
    public static void setDefaultCursorConfined()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
