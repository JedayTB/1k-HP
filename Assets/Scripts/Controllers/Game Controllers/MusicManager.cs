using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
  public static readonly string mainMenuSceneName = "MainMenu";
  public static readonly string characterSelectSceneName = "CharacterSelect";
  public static readonly string levelSelectScenename = "LevelSelect";
  public static readonly string loadingScreenName = "LoadingScreen";

  private static MusicManager instance;
    public static MusicManager Instance { get { return instance; } }

  public bool startMusicInAwake = false;
  public bool startMusicInStartFunction = false;
  public bool isForMenuMusic = false;


  [SerializeField] public AudioSource musicSource;
  public float fadeInTime = 5f;
  public static float minVolumeForFadeIn = 0.0f;
    public static float musicVolume = 0.5f;
    public static float sfxVolume = 0.5f;
    public bool doingTheFadeIn = true;


  public void startMusic()
  {
    if (instance == null)
    {
      instance = this;
      if (isForMenuMusic)
      {
        DontDestroyOnLoad(this.gameObject);
      }

      SceneManager.activeSceneChanged += sceneChangedLogic;
      musicSource.loop = true;
      StartCoroutine(fadeInVolume(fadeInTime, musicVolume));
    }
    else
    {
      Debug.Log($"{this.name} killed self because instance isnt null");
      Destroy(this.gameObject);
    }
  }

  private void sceneChangedLogic(Scene replaced, Scene next)
  {
    // If need to debug.. Replaced scene never has a name (idfk either)
    if (isForMenuMusic)
    {
      bool dontDestroySelf = next.name == mainMenuSceneName || next.name == characterSelectSceneName
        || next.name == levelSelectScenename || next.name == loadingScreenName;

      if (dontDestroySelf == true)
      {
      }
      else
      {
        instance = null;
        Debug.Log($"{this.name} killed self, because it's not a menu.");
        Destroy(this.gameObject);
      }
    }
  }
  private void OnDisable()
  {
    instance = null;
  }
  private void Start()
  {
    if (startMusicInStartFunction) startMusic();
  }
  private void Awake()
  {
    if (startMusicInAwake) startMusic();
  }
  private void OnDestroy()
  {
    SceneManager.activeSceneChanged -= sceneChangedLogic;
  }

  IEnumerator fadeInVolume(float fadeInTime, float maxFadeInVolume)
  {
        doingTheFadeIn = true;
    float count = 0f;
    float progress = 0f;

    musicSource.volume = minVolumeForFadeIn;
    musicSource.Play();
        while (musicSource.volume < maxFadeInVolume)
        {
            count += Time.deltaTime;
            progress = count / fadeInTime;
            musicSource.volume = Mathf.Lerp(minVolumeForFadeIn, maxFadeInVolume, progress);
            yield return null;
        }
        doingTheFadeIn = false;
    }

    public void UpdateMusicVolume(float newVolume)
    {
        musicSource.volume = newVolume;
        musicVolume = newVolume;
    }
}
