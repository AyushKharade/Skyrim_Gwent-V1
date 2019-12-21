using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public string DeckName;
    public int ID;
    [TextArea]
    public string description;
    public GameObject[] CardsDeck;
    public int totalCards = 0;


    void Start()
    {
        //CountCards();
    }

    void Update()
    {
        
    }

    // methods
    /*
    void CountCards()
    {
        foreach (GameObject g in CardsDeck)
            totalCards++;
    }
    */

    public void ResetDeckCardsStatus()
    {
        foreach (GameObject g in CardsDeck)
            g.GetComponent<Card>().SetCardStatus("Deck");
    }
}
