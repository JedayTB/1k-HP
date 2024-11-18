using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour
{
    [SerializeField] SceneChanger _sceneChanger;

    public void LoadScene(string sceneName)
    {
        _sceneChanger.LoadLevelWithTransition(sceneName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PLAYER"))
        {
            Debug.Log("Start scene transition");
            _sceneChanger.LoadLevelWithTransition("StageOneDemo");
        }
    }
}
