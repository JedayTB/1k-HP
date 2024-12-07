using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour
{
    [SerializeField] SceneChanger _sceneChanger;

    public void LoadScene(string sceneName)
    {
        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
        }

        GrandPrixManager.GameMode = 1;

        _sceneChanger.LoadLevelWithTransition(sceneName);
    }


    public void LoadGrandPrix()
    {
        GrandPrixManager.GameMode = 0;

        _sceneChanger.LoadLevelWithTransition("CharacterSelect");
    }

    public void LoadNextGPRace()
    {
        if (GrandPrixManager.CurrentLevelIndex < GrandPrixManager.GrandPrixLength)
        {
            _sceneChanger.LoadLevelWithTransition(GrandPrixManager.LevelOrder[GrandPrixManager.CurrentLevelIndex]);
        }
        else
        {
            _sceneChanger.LoadLevelWithTransition("ResultsScreen");
        }
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
