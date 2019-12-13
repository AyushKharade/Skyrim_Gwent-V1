using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void PressPlay()
    {
        Debug.Log("Clicked Play");
    }

    public void PressOptions()
    {
        Debug.Log("Clicked Options");
    }

    public void PressDecks()
    {
        Debug.Log("Clicked Decks");
    }

    public void PressQuit()
    {
        Application.Quit();
    }
}
