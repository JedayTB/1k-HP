using System.Collections;
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

    
    [SerializeField] private Animator _anim;
    [SerializeField] private float _transitionTime;
    void Awake()
    {
        _anim = GetComponent<Animator>();
    }
    public void LoadLevelWithTransition(string sceneName){
        StartCoroutine(StartSceneTransition(sceneName));
    }

    IEnumerator StartSceneTransition(string sceneName){
        _anim.SetTrigger("Start");

        yield return new WaitForSeconds(_transitionTime);

        ChangeSceneTo(sceneName);
    }
}
