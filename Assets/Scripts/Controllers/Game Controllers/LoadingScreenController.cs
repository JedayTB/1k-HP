using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
  public Image loadingWheel;
  public Slider LoadingBar;
  public float wheelTurnSpeed = 10f;

  private Vector3 rotateDir = new(0f, 0f, 1f);
  public string levelToAsyncLoad;
  void Start()
  {
    levelToAsyncLoad = ConstantLevelHolder.Instance.NextScenename;
    StartCoroutine(LoadAsync());
  }
  IEnumerator LoadAsync()
  {
    AsyncOperation op = SceneManager.LoadSceneAsync(levelToAsyncLoad);

    while (op.isDone == false)
    {
      LoadingBar.value = Mathf.Clamp01(op.progress / .9f);
      yield return null;
    }
  }
  // Update is called once per frame
  void Update()
  {
    loadingWheel.transform.Rotate(wheelTurnSpeed * rotateDir);
  }
}
