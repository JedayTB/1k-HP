using UnityEngine;

public class ConstantLevelHolder : MonoBehaviour
{
  private static ConstantLevelHolder instance;
  public static ConstantLevelHolder Instance { get { return instance; } }

  public string NextScenename = "";

  private void Start()
  {
    if (instance != null)
    {
      Destroy(this.gameObject);
    }
    else
    {
      instance = this;
      DontDestroyOnLoad(this.gameObject);
    }
  }
}
