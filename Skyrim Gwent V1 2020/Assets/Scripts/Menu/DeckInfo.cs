using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckInfo : MonoBehaviour
{


    // has references to all deck prefabs, sends out the deck reference when requested

    public GameObject WhiterunDeck;
    public GameObject DraugrDeck;
    public GameObject College_of_Winterhold;
    public GameObject Wilderness;
    // others when they are available.

   

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
            case "College Of Winterhold":
                {
                    return College_of_Winterhold;
                    //break;
                }
            case "Wilderness":
                return Wilderness;
        }
        return null;
    }
}
