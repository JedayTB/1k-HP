using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // idk if there was a good reason this was a single instance but it caused things to be broken and making it not a single instance doesn't break anything soo
    /*public static SceneChanger Instance {
        get { return instance; }
    }*/


    public static void ChangeSceneTo(string sceneName){
        Time.timeScale = 1.0f;
        Debug.Log("we be clicking!!");
        SceneManager.LoadScene(sceneName);
    }
    public static void QuiteGame(){
        Application.Quit();
    }


    private static SceneChanger instance;
    [SerializeField] private Animator _anim;
    [SerializeField] private float _transitionTime;
    [SerializeField] private GameObject[] _canvasElements;
    private Canvas _canvas;
    void Awake()
    {
        _anim = GetComponent<Animator>();
        if(instance != null)
        {
            //Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            _canvas = GetComponentInParent<Canvas>();

            DontDestroyOnLoad(this.gameObject);
            DontDestroyOnLoad(_canvas.gameObject);
        }
        
    }

    public void LoadLevelWithTransition(string sceneName){
        if (sceneName == "MainMenu")
        {
            GrandPrixManager.CurrentLevelIndex = 0; // just making sure the grand prix progress is reset when main menu is picked
        }

        StartCoroutine(StartSceneTransition(sceneName));
    }

    IEnumerator StartSceneTransition(string sceneName){

        _anim.SetBool("Start", true);

        yield return new WaitForSeconds(_transitionTime);

        foreach (var element in _canvasElements) { 
            element.gameObject.SetActive(false);
        }

        ChangeSceneTo(sceneName);

        StartCoroutine(EndSceneTransition());
    }
    IEnumerator EndSceneTransition()
    {
        _anim.SetBool("Start", false);

        yield return new WaitForSeconds(_transitionTime);

        _canvas.gameObject.SetActive(false);

    }
}
