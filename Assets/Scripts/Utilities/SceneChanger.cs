using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger Instance {
        get { return instance; }
    }


    public static void ChangeSceneTo(string sceneName){
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
            Destroy(this.gameObject);
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
