using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void PressPlay()
    {
        //Debug.Log("Clicked Play");
        SceneManager.LoadScene("Select Deck");
    }

    public void PressOptions()
    {
        Debug.Log("Clicked Options");
    }

    public void PressDecks()
    {
        SceneManager.LoadScene("Card Creation");
    }

    public void PressQuit()
    {
        Application.Quit();
    }

    public void GoBackToMenu()
    {
        Destroy(GameObject.FindGameObjectWithTag("GameInfo")); // destroy this instance
        SceneManager.LoadScene("Main Menu");
    }


    public void RetryGwentGame()
    {
        SceneManager.LoadScene("GwentBoard");
    }

    // start game:
    public void StartGame()
    {
        // load gwent board scene with info of which decks were chosen
    }
}
