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

    public Text Hint;

    public string[] hints;


    //public string[] Tips;
    void Start()
    {
        loadingTime = Random.Range(2.5f,4.5f);
        Hint.text = hints[Random.Range(0, hints.Length)];
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
        //SceneManager.LoadScene("GwentBoard");
        SceneManager.LoadScene("Redraw2");
    }
}
