using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (musicSource == null) {
            Debug.LogError("Forgot to attach Audioscouce");
            return;
        }

        if(isForMenuMusic)
        {
            DontDestroyOnLoad(this.gameObject);
        }
        SceneManager.activeSceneChanged += sceneChangedLogic;
        StartCoroutine(fadeInVolume(fadeInTime, GameStateManager.musicVolumeLevel));
    }

    private void sceneChangedLogic(Scene replaced, Scene next)
    {
        if (isForMenuMusic)
        {
            //bool checkIfReplacedIsMenu = replaced.name == mainMenuSceneName || replaced.name == characterSelectSceneName || replaced.name == levelSelectScenename;

            //if (checkIfReplacedIsMenu) {
            print(next.name);
            print($"{next.name}, MainMenustr: {mainMenuSceneName}, equals {next.name == mainMenuSceneName}");

                bool destroySelf = next.name != mainMenuSceneName || next.name != characterSelectSceneName || next.name != levelSelectScenename;
                
                if (destroySelf == false)
                {
                    print("Music killed self");
                    Destroy(this.gameObject);
                }
                else
                {
                    print("Dont destroy");
                }
            //}
        }
        print($"func run, {this.name}, {replaced.name}, {next.name}");
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
