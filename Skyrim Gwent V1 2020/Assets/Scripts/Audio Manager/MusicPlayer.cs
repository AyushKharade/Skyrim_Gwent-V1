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
    string playingSceneName;
    public int musicID;

    float playTime;
    bool playing;
    public float timer;
    public float trackLength;
    public string nowPlaying;


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
        SingletonCheck2();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playing)
        {
            CheckScene();
        }
        else
        {
            CheckChangeInScene();
            EndSoundTrack();
            //UpdateTimerData();
        }
    }

    void CheckChangeInScene()
    {
        currentScene = SceneManager.GetActiveScene();
        sceneName = currentScene.name;
        if (playingSceneName == "Main Menu")
        {
            if (sceneName == "Select Deck")
            { }
            else if (sceneName == "Main Menu")
            { }
            else
            {
                //force stop
                AudioManager.instance.Stop(AudioManager.instance.sounds[musicID].name);
                playing = false;
                timer = 0;
            }

        }
        else if (playingSceneName == "Select Deck")
        {
            if (sceneName == "Main Menu" || sceneName == "Select Deck")
            { }
            else
            {
                //force stop
                AudioManager.instance.Stop(AudioManager.instance.sounds[musicID].name);
                playing = false;
                timer = 0;
            }
        }
        else if (playingSceneName == "GwentBoard" && sceneName == "Main Menu")
        {
            //force stop
            AudioManager.instance.Stop(AudioManager.instance.sounds[musicID].name);
            playing = false;
            timer = 0;
        }
        else if (playingSceneName != sceneName)
        {
            //force stop
            AudioManager.instance.Stop(AudioManager.instance.sounds[musicID].name);
            playing = false;
            timer = 0;
        }
    }

    void MenuMusic()
    {
        int rand = Random.Range(1, 10);
        if (rand <= 3)
        {
            AudioManager.instance.Play("SkyrimTheme");
            playTime = AudioManager.instance.sounds[0].clip.length + 5;
            playing = true;
            playingSceneName = sceneName;
            musicID = 0;
        }
        else if (rand <= 6)
        {
            AudioManager.instance.Play("Wake_The_White_Wolf");
            playTime = AudioManager.instance.sounds[1].clip.length + 5;
            playing = true;
            playingSceneName = sceneName;
            musicID = 1;
        }
        else
        {
            AudioManager.instance.Play("Toss_a_coin");
            playTime = AudioManager.instance.sounds[2].clip.length + 5;
            playing = true;
            playingSceneName = sceneName;
            musicID = 2;
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
        if (!playing)
        {
            // randomly play from track 1 to track 9
            int randInt = Random.Range(1, 11);
            string trackName = "Game" + randInt;

            // array code is always game 'n'+2
            AudioManager.instance.Play(trackName);
            playTime = AudioManager.instance.sounds[randInt + 2].clip.length + 1.5f;
            playing = true;
            playingSceneName = sceneName;
            musicID = randInt;
        }
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

    void UpdateTimerData()
    {
        trackLength = AudioManager.instance.sounds[musicID].clip.length;
        nowPlaying = AudioManager.instance.sounds[musicID].clip.name;
    }



    void SingletonCheck2()
    {
        //delete itself if there already is a class
        GameObject[] arr = GameObject.FindGameObjectsWithTag("MusicPlayer");
        if (arr.Length > 1)
        {
            Destroy(this.gameObject);
        }
    }
}
