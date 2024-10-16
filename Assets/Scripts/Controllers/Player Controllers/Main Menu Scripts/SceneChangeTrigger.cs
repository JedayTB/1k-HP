using UnityEngine;

public class SceneChangeTrigger : MonoBehaviour
{
    [SerializeField] SceneChanger _sceneChanger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PLAYER"))
        {
            Debug.Log("Start scene transition");
            _sceneChanger.LoadLevelWithTransition("StageOneDemo");
        }
    }
}
