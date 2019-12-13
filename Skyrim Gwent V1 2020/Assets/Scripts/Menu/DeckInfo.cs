using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckInfo : MonoBehaviour
{


    // has references to all deck prefabs, sends out the deck reference when requested

    public GameObject WhiterunDeck;
    public GameObject DraugrDeck;
    // others when they are available.

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public GameObject FetchDeck(string name)
    {
        switch(name)
        {
            case "Whiterun Warriors":
                {
                    return WhiterunDeck;
                    //break;
                };
            case "Undead Draugrs":
                {
                    return DraugrDeck;
                    //break;
                };
        }
        return null;
    }
}
