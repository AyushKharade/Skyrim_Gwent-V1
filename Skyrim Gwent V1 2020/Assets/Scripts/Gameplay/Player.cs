using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
 * This is the main gameplay loop, or the main game script.
 * It controls player inputs, game functions to advance games.
 * Relies on GameInfo object to retrieve decks
 * Generates hands from decks
 * Uses battlefield class onjects for both players as supporting methods
 * Does most UI (except scores --> done in battlefield.
 */
public class Player : MonoBehaviour
{
    GameObject raycastTarget;

    bool gameEnded;
    bool controlLock;
    float controlLockTimer;
    float controlLockTime = 1.5f;


    [Range(1,2)]
    [HideInInspector]public int turn;
    [Range(1, 3)]
    public int round=1;

    public int p1Lives = 2;
    public int p2Lives = 2;

    public GameObject P1Battlefield;
    public GameObject P2Battlefield;

    Battlefield P1BFRef;
    Battlefield P2BFRef;

    // ui done from p1_battlefield
    public Text RoundUI;
    public Image P1HP1;
    public Image P1HP2;
    public Image P2HP1;
    public Image P2HP2;

    // dead rgb = 126,99,99
    // references to pass buttons
    public Button P1PassRef; 
    public Button P2PassRef;
    Button P1Pass;
    Button P2Pass;


    //temp card count:
    int P1Cards = 10;
    int P2Cards = 10;

    // game info ref
    GameStarter gameinfo;

    // hand instantiation references.
    public Transform p1HandRef;
    public Transform p2HandRef;

    // discard pile offset
    float p1DiscardXPos=5;
    float p1DiscardYPos=-2.8f;
    float p2DiscardXPos=5;
    float p2DiscardYPos=2.8f;


    // if hide
    public bool hideOpponentCards;


    //ref to popup message prefab
    public GameObject popupPrefab;
    public GameObject endgamePrefab;

    // endgame info
    int ScoreR1P1;
    int ScoreR1P2;
    int ScoreR2P1;
    int ScoreR2P2;
    int ScoreR3P1;
    int ScoreR3P2;

    void Start()
    {
        // random turn
        int randNo = Random.Range(0,100);
        turn = (randNo % 2)+1;

        // get references to battlefield objects
        P1BFRef = P1Battlefield.GetComponent<Battlefield>();
        P2BFRef = P2Battlefield.GetComponent<Battlefield>();

        //references to pass buttons
        P1Pass = P1PassRef.GetComponent<Button>();
        P2Pass = P2PassRef.GetComponent<Button>();

        // fetch info
        gameinfo = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameStarter>();

        //init
        InitializeGame();
        if (hideOpponentCards)
        { 
            if (turn == 2)
                FlipCardsInDeck(2);
            else
                FlipCardsInDeck(1);
        }

        // popup message for the first message to be displayed
        if (turn == 1)
        {
            GameObject popup = Instantiate(popupPrefab);
            popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetMessage("Player 1 goes first.");
        }
        else
        {
            GameObject popup = Instantiate(popupPrefab);
            popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetMessage("Player 2 goes first.");
        }
        // so palyers cant click right away
        TurnOnControlLock();
    }

    private void InitializeGame()
    {
        // generate initial hand for both players
        GenerateHand(1);
        GenerateHand(2);
    }


    void Update()
    {
        if (!gameEnded && !controlLock)
        {
            GetCameraRaycast();               // main input
            CardScaling();
            PassButtonController();
        }
        else if (controlLock)
            ControlLockCounter();

        // Round UI (change to function
        RoundUI.text = "Round: " + round;
    }


    void GetCameraRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
            raycastTarget = hit.collider.gameObject;
        else
            raycastTarget = null;

        // input
        if (Input.GetMouseButtonDown(0) && hit.collider != null)
        {
            //place card
            if(hit.collider.gameObject.GetComponent<Card>().GetCardStatus() == "Hand")
                DeployUnitCard(hit.collider.gameObject);
        }
    }

    // early debugger function
    void DisplayRaycastTarget()
    {
        if (raycastTarget != null)
            Debug.Log("Card: " + raycastTarget.transform.name);
    }


    private void GenerateHand(int PlayerID)
    {
        int count = 10;
        float yOffset;
        float xOffset=-2;
        GameObject deck;

        if (PlayerID == 1)
        {
            deck = gameinfo.P1Deck;
            yOffset = -4.2f;
        }
        else
        {
            deck = gameinfo.P2Deck;
            yOffset = 4.4f;
        }

        int maxCards = deck.GetComponent<Deck>().totalCards;
        //int[] arr = new int[maxCards];

        for (int i = 0; i < count; i++)
        {
            // fetch signature:
            GameObject card;
            card = deck.GetComponent<Deck>().CardsDeck[Random.Range(0, maxCards)];

            //if already used
            if (card.GetComponent<Card>().GetCardStatus() != "Deck")
                i--;
            else
            {
                card.GetComponent<Card>().SetCardStatus("Hand");
                // instantiate
                if (PlayerID == 1)
                {
                    GameObject temp = Instantiate(card, p1HandRef);
                    temp.transform.position = new Vector3(xOffset, yOffset, 0);
                    xOffset += 0.75f;
                }
                else
                {
                    GameObject temp = Instantiate(card, p2HandRef);
                    temp.transform.position = new Vector3(xOffset, yOffset, 0);
                    xOffset += 0.75f;
                }
            }
        }
    }


    void DeployUnitCard(GameObject card)
    {
        Card cardRef = card.GetComponent<Card>();
        if (cardRef.info.GetUnitType() == "Warrior" && cardRef.GetCardStatus()=="Hand")
        {
            if (turn == 1 && card.transform.parent.name=="Player1_Hand")
            {
                P1BFRef.AddUnitToFrontline(card);
                ChangeTurn();

                P1Cards--;
                if (P1Cards == 0)
                    ForcePass(1);

            }
            else if (turn == 2 && card.transform.parent.name == "Player2_Hand")
            {
                P2BFRef.AddUnitToFrontline(card);
                ChangeTurn();

                P2Cards--;
                if (P2Cards == 0)
                    ForcePass(2);
            }
        }

        else if ((cardRef.info.GetUnitType() == "Mage" || cardRef.info.GetUnitType() == "Spellsword") && cardRef.GetCardStatus() == "Hand")
        {
            // place on Vantage -- for now place spellswords on vantage too
            if (turn == 1 && card.transform.parent.name == "Player1_Hand")
            {
                P1BFRef.AddUnitToVantage(card);
                ChangeTurn();

                P1Cards--;
                if (P1Cards == 0)
                    ForcePass(1);
            }
            else if (turn == 2 && card.transform.parent.name == "Player2_Hand")
            {
                P2BFRef.AddUnitToVantage(card);
                ChangeTurn();

                P2Cards--;
                if (P2Cards == 0)
                    ForcePass(2);
            }
        }
        

        else if (cardRef.info.GetUnitType() == "Shadow" && cardRef.GetCardStatus() == "Hand")
        {
            // place on shadow
            if (turn == 1 && card.transform.parent.name == "Player1_Hand")
            {
                P1BFRef.AddUnitToShadow(card);
                ChangeTurn();

                P1Cards--;
                if (P1Cards == 0)
                    ForcePass(1);
            }
            else if (turn == 2 && card.transform.parent.name == "Player2_Hand")
            {
                P2BFRef.AddUnitToShadow(card);
                ChangeTurn();

                P2Cards--;
                if (P2Cards == 0)
                    ForcePass(2);
            }
        }
       

    }


    public void ChangeTurn()
    {
        if (turn == 2)
        {
            if (!P1BFRef.playerPassed)
            {
                turn = 1;
                FlipCardsInDeck(1);
                FlipCardsInDeck(2);
                TurnOnControlLock();
                //
                GameObject popup = Instantiate(popupPrefab);
                popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetExpireTimer(1);
                popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetMessage("Player 1's turn.");

            }
        }
        else if (turn == 1)
        {
            if (!P2BFRef.playerPassed)
            {
                turn = 2;
                FlipCardsInDeck(1);
                FlipCardsInDeck(2);
                TurnOnControlLock();
                //
                GameObject popup = Instantiate(popupPrefab);
                popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetExpireTimer(1);
                popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetMessage("Player 2's turn.");
            }
        }

        
        if (P1BFRef.playerPassed && P2BFRef.playerPassed)
        {
            if (!controlLock)                
            {
                //call end of round function
                if(round==3)
                    Invoke("EndOfRound", 1f);
                else
                    EndOfRound();
                Debug.Log("End of round.");
            }
           
        }
        
    }


    void EndOfRound()
    {
        if (P1BFRef.totalScore > P2BFRef.totalScore)
        {
            Debug.Log("Player 1 Won round "+round);
            p2Lives--;
            //
            GameObject popup = Instantiate(popupPrefab);
            popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetExpireTimer(2f);
            popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetMessage("Player 1 won the round.");


        }
        else if (P1BFRef.totalScore == P2BFRef.totalScore)
        {
            Debug.Log("Round "+round+" Tied");
            p1Lives--;
            p2Lives--;
            //
            GameObject popup = Instantiate(popupPrefab);
            popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetExpireTimer(2);
            popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetMessage("Round Draw.");
        }
        else if (P1BFRef.totalScore < P2BFRef.totalScore)
        {
            Debug.Log("Player 2 Won round "+round);
            p1Lives--;
            //
            GameObject popup = Instantiate(popupPrefab);
            popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetExpireTimer(2);
            popup.transform.GetChild(0).gameObject.GetComponent<PopupMessage>().SetMessage("Player 2 won the round.");
        }

        // update
        RoundStatus();
        UpdateLivesUI();
    }

    void RoundStatus()
    {
        // check if anyone lost then end game
        if (p1Lives == 0 && p2Lives == 0)
        {
            Debug.Log("Match Draw!");
            gameEnded = true;
            EndgameStats();
        }
        else if (p1Lives == 0)
        {
            Debug.Log("Player 2 Won the Match!!");
            gameEnded = true;
            EndgameStats();
        }
        else if (p2Lives == 0)
        {
            Debug.Log("Player 1 Won the Match!!");
            gameEnded = true;
            EndgameStats();
        }
        else
        {
            Debug.Log("Starting Next Round");
            round++;
            Reinitialize();
        }
    }


    void Reinitialize()
    {
        // move current cards to discard pile (done in battlefield)
        // reset pass buttons and UI & score
        P1Pass.gameObject.GetComponent<PassRound>().Reset();
        P2Pass.gameObject.GetComponent<PassRound>().Reset();

        //save scores
        if (round - 1 == 1)
        {
            ScoreR1P1 = P1BFRef.totalScore;
            ScoreR1P2 = P2BFRef.totalScore;
        }
        else if (round - 1 == 2)
        {
            ScoreR2P1 = P1BFRef.totalScore;
            ScoreR2P2 = P2BFRef.totalScore;
        }

        //reset battlefield scripts
        P1BFRef.Reset();
        P2BFRef.Reset();

        RemoveDeployedCards();
        if (P1Cards == 0)
            ForcePass(1);
        if (P2Cards == 0)
            ForcePass(2);
    }

    void RemoveDeployedCards()
    {
        
        int count = 0;
        while (count<10)
        {
            if (p1HandRef.GetChild(count).GetComponent<Card>().GetCardStatus() == "Deployed")
            {
                GameObject card = p1HandRef.GetChild(count).gameObject;
                //set status to discard, rotate and move to discard pile
                card.GetComponent<Card>().SetCardStatus("Discard");
                card.transform.Rotate(new Vector3(0,180,0));
                card.transform.position = new Vector3(p1DiscardXPos,p1DiscardYPos);

                //Debug.Log("Destroyed one card: "+p1HandRef.GetChild(count).GetComponent<Card>().name);
                count++;
            }
            else
                count++;
        }
        //p2
        count = 0;
        while (count < 10)
        {
            if (p2HandRef.GetChild(count).GetComponent<Card>().GetCardStatus() == "Deployed")
            {
                GameObject card = p2HandRef.GetChild(count).gameObject;
                //set status to discard, rotate and move to discard pile
                card.GetComponent<Card>().SetCardStatus("Discard");
                card.transform.Rotate(new Vector3(0, 180, 0));
                card.transform.position = new Vector3(p2DiscardXPos, p2DiscardYPos);
                //Debug.Log("Destroyed one card: " + p1HandRef.GetChild(count).GetComponent<Card>().name);
                count++;
            }
            else
                count++;
        }
    }


    void EndgameStats()
    {
        if (round == 2)
        {
            ScoreR2P1 = P1BFRef.totalScore;
            ScoreR2P2 = P2BFRef.totalScore;
        }
        else if (round == 3)
        {
            ScoreR3P1 = P1BFRef.totalScore;
            ScoreR3P2 = P2BFRef.totalScore;
        }
        
        
        //instantiate prefab and send info
        GameObject temp=Instantiate(endgamePrefab);
        Endgame endgameScriptRef=temp.transform.GetChild(0).GetComponent<Endgame>();
        endgameScriptRef.SetP1Scores(ScoreR1P1,ScoreR2P1,ScoreR3P1);
        endgameScriptRef.SetP2Scores(ScoreR1P2,ScoreR2P2,ScoreR3P2);
    }

    

    // extra
    void CardScaling()
    {
        if (raycastTarget != null)
            raycastTarget.GetComponent<CardScaler>().underCursor = true;
    }

    // to disable clicking if player's turn isnt there or if they passed.
    void PassButtonController()
    {
        if (turn == 1 && !P1BFRef.playerPassed)
        {
            P1PassRef.interactable = true;
            P2PassRef.interactable = false;
        }
        else if (turn == 2 && !P2BFRef.playerPassed)
        {
            P1PassRef.interactable = false;
            P2PassRef.interactable = true;
        }
    }

    void ForcePass(int ID)
    {
        if (ID == 1)
            P1Pass.gameObject.GetComponent<PassRound>().Pass();
        else if (ID==2)
            P2Pass.gameObject.GetComponent<PassRound>().Pass();
    }


    //experimental: // allow fliping all cards if not your turn:
    //doesnt work right now fix later
    void FlipCardsInDeck(int ID)
    {
        if (hideOpponentCards)
        {
            Debug.Log("Card Hiding Called");
            if (ID == 1)
            {
                int count = 0;
                while (count < P1Cards)
                {
                    GameObject card = p1HandRef.GetChild(count).gameObject;
                    card.transform.Rotate(new Vector3(0,180,0));
                    count++;
                }
            }
            else if (ID == 2)
            {
                int count = 0;
                while (count < P2Cards)
                {
                    GameObject card = p1HandRef.GetChild(count).gameObject;
                    card.transform.Rotate(new Vector3(0,180,0));
                    count++;
                }
            }
            
        }

    }

    void UpdateLivesUI()
    {
        //p1
        if (p1Lives == 1)
            P1HP1.enabled = false;
        if (p1Lives == 0)
            P1HP2.enabled = false;
        // p2
        if (p2Lives == 1)
            P2HP1.enabled = false;
        if (p2Lives == 0)
            P2HP2.enabled = false;
    }

    void ControlLockCounter()
    {
        controlLockTimer += Time.deltaTime;
        if (controlLockTimer > controlLockTime)
        {
            controlLock = false;
            controlLockTimer = 0;
        }
    }

    void TurnOnControlLock()
    {
        controlLock = true;
        controlLockTimer = 0;
    }
}
