using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/* plays background themes*/
// regular sound effects called from respective scripts

public class MusicPlayer : MonoBehaviour
{
    Scene currentScene;
    string sceneName;

    float playTime;
    bool playing;
    float timer;


    // Start is called before the first frame update
    void Start()
    {
        currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;
        CheckScene();
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (!playing)
            CheckScene();
        else
            EndSoundTrack();
    }

    void MenuMusic()
    {
        int rand = Random.Range(1, 10);
        if (rand < 5)
        {
            AudioManager.instance.Play("SkyrimTheme");
            playTime = AudioManager.instance.sounds[0].clip.length + 5;
            playing = true;
        }
        else
        {
            AudioManager.instance.Play("Witcher3_Wolven_Storm");
            playTime = AudioManager.instance.sounds[1].clip.length + 5;
            playing = true;
        }
    }

    void CheckScene()
    {
        currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;

        // call approriate methods:
        if (sceneName == "Main Menu" || sceneName == "Select Deck")
        {
            MenuMusic();
        }
        else if (sceneName == "GwentBoard")
        {
            GameplayMusic();
             
        }
    }

    void LoadingMusic()
    { }

    void DeckMusic()
    { }

    void GameplayMusic()
    {
        // array code is always game 'n'+2
        AudioManager.instance.Play("Game1");
        playTime = AudioManager.instance.sounds[3].clip.length + 5;
        playing = true;

    }


    void EndSoundTrack()
    {
        timer += Time.deltaTime;
        if (timer > playTime)
        {
            timer = 0;
            playing = false;
        }
    }
}
