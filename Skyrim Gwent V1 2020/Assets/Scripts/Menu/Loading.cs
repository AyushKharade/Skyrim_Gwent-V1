using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


// Fake loading screen:
public class Loading : MonoBehaviour
{
    float loadingTime;
    float Timer;

    //public string[] Tips;
    void Start()
    {
        loadingTime = Random.Range(3,7);
    }

    // Update is called once per frame
    void Update()
    {
        Timer += Time.deltaTime;
        if (Timer > loadingTime)
            LoadGame();
    }

    void LoadGame()
    {
        SceneManager.LoadScene("GwentBoard");
    }
}
