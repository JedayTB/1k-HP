using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
  public static readonly string mainMenuSceneName = "MainMenu";
  public static readonly string characterSelectSceneName = "CharacterSelect";
  public static readonly string levelSelectScenename = "LevelSelect";

  [SerializeField] private AudioSource musicSource;

  public float fadeInTime = 5f;
  public static float minVolumeForFadeIn = 0.0f;
  public bool startMusicInAwake = false;
  public bool isForMenuMusic = false;

  public void startMusic()
  {
    if (musicSource == null)
    {
      Debug.LogWarning("Forgot to attach Audio Source");
      musicSource = GetComponent<AudioSource>();
      if (musicSource == null) Debug.LogError($"{this.name} is fucked. Can't grab Audio Manager");
      return;
    }

    if (isForMenuMusic)
    {
      DontDestroyOnLoad(this.gameObject);
    }
    SceneManager.activeSceneChanged += sceneChangedLogic;
    StartCoroutine(fadeInVolume(fadeInTime, GameStateManager.musicVolumeLevel));
  }

  private void sceneChangedLogic(Scene replaced, Scene next)
  {
    // If need to debug.. Replaced scene never has a name (idfk either)
    if (isForMenuMusic)
    {

      if (isForMenuMusic)
      {

        bool dontDestroySelf = next.name == mainMenuSceneName || next.name == characterSelectSceneName || next.name == levelSelectScenename;

        if (dontDestroySelf == true)
        {
        }
        else
        {

          Destroy(this.gameObject);
        }
      }
    }
  }

  private void Awake()
  {
    if (startMusicInAwake) startMusic();
  }
  IEnumerator fadeInVolume(float fadeInTime, float maxFadeInVolume)
  {
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
  }
  private void OnDestroy()
  {
    SceneManager.activeSceneChanged -= sceneChangedLogic;
  }

}
