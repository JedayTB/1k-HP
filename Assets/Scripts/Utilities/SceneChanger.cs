using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static void ChangeSceneTo(string sceneName){
        SceneManager.LoadScene(sceneName);
    }
    public static void QuiteGame(){
        Application.Quit();
    }
}
