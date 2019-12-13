using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 *  This is a dontdestroyonLoad class that stores information about which decks were selected and player names. 
 *  Instance Destroyed manually when required 
 */
public class GameStarter : MonoBehaviour
{
    [Header("GameObject Refernces")]
    public GameObject DeckInfoObj;
    DeckInfo DeckInfoRef;
    public GameObject P1Deck;
    public GameObject P2Deck;

    string P1Name;
    string P2Name;

    string P1DeckName;
    string P2DeckName;

    //[Header("UI_Objects")]
    //UI refernces

    public InputField P1NameUI;
    public InputField P2NameUI;

    public Dropdown P1DeckUI;
    public Dropdown P2DeckUI;

    public Button PlayUIRef;


    void Start()
    {
        PlayUIRef.onClick.AddListener(StartGame);
        DeckInfoRef = DeckInfoObj.GetComponent<DeckInfo>();
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        CheckDeckSelection();
    }

    void StartGame()
    {
        AssignName();
        AssignDeck();
        AcquireDeck();
        Debug.Log("Starting Game, Information Collected:");
        Debug.Log("Player 1: "+P1Name+" starting with deck "+P1DeckName);
        Debug.Log("Player 2: "+P2Name+" starting with deck "+P2DeckName);

    }

    void AssignName()
    {
        // Need to add listener events
        // P1
        //string name = P1NameUI + "";
        /*
        string name = P1NameUI.text.ToString();
        if (name == "")
            P1Name = "Player1";
        else
            P1Name = P1NameUI + "";
        //P2
        name = P2NameUI + "";
        if (name == "")
            P2Name = "Player2";
        else
            P2Name = P1NameUI + "";
            */
        P1Name = "Player 1";
        P2Name = "Player 2";
    }

    void AssignDeck()
    {
        //p1
        switch (P1DeckUI.value)
        {
            case 1:
                {
                    P1DeckName = "Whiterun Warriors";
                    break;
                }
            case 2:
                {
                    P1DeckName = "Undead Draugrs";
                    break;
                }
           
        }
        //p2
        switch (P2DeckUI.value)
        {
            case 1:
                {
                    P2DeckName = "Whiterun Warriors";
                    break;
                }
            case 2:
                {
                    P2DeckName = "Undead Draugrs";
                    break;
                }

        }
    }

    void AcquireDeck()
    {
        P1Deck = DeckInfoRef.FetchDeck(P1DeckName);
        P2Deck = DeckInfoRef.FetchDeck(P2DeckName);
    }

    void CheckDeckSelection()
    {
        if (P1DeckUI.value != 0 && P2DeckUI.value != 0)
            PlayUIRef.interactable = true;
        else
            PlayUIRef.interactable = false;
    }

    
}
