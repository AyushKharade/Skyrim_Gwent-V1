using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// for testing reload for new draw
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 * This is the main gameplay loop, or the main game script.
 * It controls player inputs, game functions to advance games.
 * Relies on GameInfo object to retrieve decks
 * Generates hands from decks
 * Uses battlefield class onjects for both players as supporting methods
 * Does most UI (except scores --> done in battlefield.)
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

    // references to pass buttons
    public Button P1PassRef; 
    public Button P2PassRef;
    Button P1Pass;
    Button P2Pass;


    //temp card count:
    public int P1Cards = 10;
    public int P2Cards = 10;
    
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

    //detailed display
    public GameObject cardDeploying;
    public Transform detailedDisplayRef;
    bool cardDisplaying;

    public Text quoteDetails;
    GameObject quoteBox;

    // deployment buttons
    [Header("Clickable Buttons")]
    public Button DeployFrontlineButton;
    public Button DeployVantage_SpellswordButton;
    public Button DeployVantageButton;
    public Button DeployShadowButton;
    public Button DeploySpecialButton;
    public Button CloseButton;

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
                FlipCardsInDeck(1);
            else
                FlipCardsInDeck(2);
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
        // so players cant click right away
        TurnOnControlLock();


        quoteBox = quoteDetails.transform.parent.gameObject;
        quoteBox.SetActive(false);
    }

    private void InitializeGame()           // generate initial hand for both players
    {
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
        //RoundUI.text = "Round: " + round;


        // for redrawing decks (testing)
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene("GwentBoard");
        }
    }


    // Mouse Input
    //________________________________________________________________________________________________
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
            if (hit.collider.gameObject.GetComponent<Card>().GetCardStatus() == "Hand")
            {
                DisplayDetailsUnitCard(hit.collider.gameObject);
                //DeployUnitCard(hit.collider.gameObject);
            }
        }
    }



    // Deployment functions
    //_________________________________________________________________________________________________________________________
    // display card magnified and in detail, along with their quote.
    void DisplayDetailsUnitCard(GameObject card)
    {
        if (card.GetComponent<Card>().GetCardStatus() =="Hand")
        {
            if ((card.transform.parent.name == "Player1_Hand" && turn == 1) || (card.transform.parent.name == "Player2_Hand" && turn == 2))
            {
                cardDeploying = card;
                //check if theres a on display already.
                if (detailedDisplayRef.childCount > 0)
                    Destroy(detailedDisplayRef.GetChild(0).gameObject);

                // instantiate a copy in the detailed display area:
                GameObject cardRef = Instantiate(card, detailedDisplayRef);
                cardDisplaying = true;

                //position
                cardRef.transform.position = detailedDisplayRef.position;

                //scale care huge
                Vector3 ogScale = cardRef.transform.localScale;
                cardRef.transform.localScale = new Vector3(ogScale.x * 2.5f, ogScale.y * 2.5f, ogScale.z);
                cardRef.GetComponent<CardScaler>().displayCard = true;

                // update quotebox
                if (!quoteBox.activeSelf)
                {
                    quoteBox.SetActive(true);
                }
                if(card.GetComponent<Card>().info.GetUnitType()=="Special")
                    quoteDetails.text = "" + cardRef.GetComponent<Card>().info.Ability_Details;
                else
                    quoteDetails.text = "" + cardRef.GetComponent<Card>().info.Quotes;

                //Debug.Log("" + cardRef.GetComponent<Card>().info.Quotes);

                // hide and show appropriate buttons
                ManageDeployButtons(card);
            }
        }
    }

    void ManageDeployButtons(GameObject card)
    {
        DeployFrontlineButton.gameObject.SetActive(false);
        DeployVantageButton.gameObject.SetActive(false);
        DeployVantage_SpellswordButton.gameObject.SetActive(false);
        DeployShadowButton.gameObject.SetActive(false);
        DeploySpecialButton.gameObject.SetActive(false);
        CloseButton.gameObject.SetActive(true);

        switch (card.GetComponent<Card>().info.GetUnitType())
        {
            case "Warrior":
                {
                    DeployFrontlineButton.gameObject.SetActive(true);
                    break;
                }
            case "Spellsword":
                {
                    DeployFrontlineButton.gameObject.SetActive(true);
                    DeployVantage_SpellswordButton.gameObject.SetActive(true);
                    break;
                }
            case "Mage":
                {
                    DeployVantageButton.gameObject.SetActive(true);
                    break;
                }
            case "Shadow":
                {
                    DeployShadowButton.gameObject.SetActive(true);
                    break;
                }
            default:
                {
                    DeploySpecialButton.gameObject.SetActive(true);
                    break;
                }
        }
    }

    public void CloseDetailsMenu()
    {
        if(detailedDisplayRef.childCount>0)
            Destroy(detailedDisplayRef.GetChild(0).gameObject);
        cardDeploying = null;

        DeployFrontlineButton.gameObject.SetActive(false);
        DeployVantageButton.gameObject.SetActive(false);
        DeployVantage_SpellswordButton.gameObject.SetActive(false);
        DeployShadowButton.gameObject.SetActive(false);
        DeploySpecialButton.gameObject.SetActive(false);
        CloseButton.gameObject.SetActive(false);

        cardDisplaying = false;
        quoteBox.SetActive(false);
        cardDeploying = null;
    }


    // Revamped smaller functions for deployment
    //------------------------------------------------------------------------------
    // actual deployment functions

    public void DeployToFrontline()
    {
        cardDeploying.GetComponent<Card>().SetCardStatus("Deployed");
        if (turn == 1)
        {
            P1BFRef.AddUnitToFrontline(cardDeploying);
            P1Cards--;
            if (P1Cards == 0)
                ForcePass(1);
        }
        else if (turn == 2)
        {
            P2BFRef.AddUnitToFrontline(cardDeploying);
            P2Cards--;
            if (P2Cards == 0)
                ForcePass(2);
        }

        ChangeTurn();
        CloseDetailsMenu();
    }


    public void DeployToVantage(string type)               // type could be regular, healer or necromancer
    {
        cardDeploying.GetComponent<Card>().SetCardStatus("Deployed");
        if (type == "Regular")
        {
            if (turn == 1)
            {
                P1BFRef.AddUnitToVantage(cardDeploying);
                P1Cards--;
                if (P1Cards == 0)
                    ForcePass(1);
            }
            else if (turn == 2)
            {
                P2BFRef.AddUnitToVantage(cardDeploying);
                P2Cards--;
                if (P2Cards == 0)
                    ForcePass(2);
            }
            
        }
        else if (type == "Healer")
        {

        }
        else if (type == "Necromancer")
        {

        }

        ChangeTurn();
        CloseDetailsMenu();
    }


    public void DeployToShadow()
    {
        cardDeploying.GetComponent<Card>().SetCardStatus("Deployed");
        if (turn == 1)
        {
            P1BFRef.AddUnitToShadow(cardDeploying);
            P1Cards--;
            if (P1Cards == 0)
                ForcePass(1);
        }
        else if (turn == 2)
        {
            P2BFRef.AddUnitToShadow(cardDeploying);
            P2Cards--;
            if (P2Cards == 0)
                ForcePass(2);
        }

        ChangeTurn();
        CloseDetailsMenu();
    }

    public void DeploySpy(string zone)
    {
        if (turn == 1)
        {
            cardDeploying.transform.position = new Vector3(0, 4.4f, 0);
            cardDeploying.transform.SetParent(p2HandRef);
            switch (zone)
            {
                case "Frontline":
                    {
                        P2BFRef.AddUnitToFrontline(cardDeploying);
                        break;
                    }
                case "Vantage":
                    {
                        P2BFRef.AddUnitToVantage(cardDeploying);
                        break;
                    }
                case "Shadow":
                    {
                        P2BFRef.AddUnitToShadow(cardDeploying);
                        break;
                    }
            }
            int maxCards = gameinfo.P1Deck.GetComponent<Deck>().totalCards;

            int c1Index = P1BFRef.RedrawCard(maxCards);
            int c2Index = P1BFRef.RedrawCard(maxCards);

            GameObject c1 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c1Index];
            GameObject c2 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c2Index];
            //
            GameObject c1Ref = Instantiate(c1, p1HandRef);
            GameObject c2Ref = Instantiate(c2, p1HandRef);
            P1Cards += 1;

            // manually change state to hand because its still deck when you call rearrange
            c1Ref.GetComponent<Card>().SetCardStatus("Hand");
            c2Ref.GetComponent<Card>().SetCardStatus("Hand");

            // call rearrange function on hand cards
            P1BFRef.RearrangeHand(p1HandRef, -4.2f);
        }
        // player 2's spies
        else if (turn == 2)
        {
            cardDeploying.transform.position = new Vector3(0, -4.2f, 0);
            cardDeploying.transform.SetParent(p1HandRef);
            switch (zone)
            {
                case "Frontline":
                    {
                        P1BFRef.AddUnitToFrontline(cardDeploying);
                        break;
                    }
                case "Vantage":
                    {
                        P1BFRef.AddUnitToVantage(cardDeploying);
                        break;
                    }
                case "Shadow":
                    {
                        P1BFRef.AddUnitToShadow(cardDeploying);
                        break;
                    }
            }
            int maxCards = gameinfo.P2Deck.GetComponent<Deck>().totalCards;

            int c1Index = P2BFRef.RedrawCard(maxCards);
            int c2Index = P2BFRef.RedrawCard(maxCards);

            GameObject c1 = gameinfo.P2Deck.GetComponent<Deck>().CardsDeck[c1Index];
            GameObject c2 = gameinfo.P2Deck.GetComponent<Deck>().CardsDeck[c2Index];
            //
            GameObject c1Ref = Instantiate(c1, p2HandRef);
            GameObject c2Ref = Instantiate(c2, p2HandRef);
            P2Cards += 1;

            // manually change state to hand because its still deck when you call rearrange
            c1Ref.GetComponent<Card>().SetCardStatus("Hand");
            c2Ref.GetComponent<Card>().SetCardStatus("Hand");

            // call rearrange function on hand cards
            P2BFRef.RearrangeHand(p2HandRef, 4.4f);
        }

        ChangeTurn();
        CloseDetailsMenu();
    }

    public void DeploySpecialWeather(string type)
    {
        switch (type)
        {
            case "FrostWeather":
                {
                    P1BFRef.SetFrostbiteWeather();
                    P2BFRef.SetFrostbiteWeather();
                    SFXManager.instance.Play("Frostbite_Weather");
                    break;
                }
            case "BaneAetheriusWeather":
                {
                    P1BFRef.SetBaneAetheriusWeather();
                    P2BFRef.SetBaneAetheriusWeather();
                    break;
                }
            case "StormWeather":
                {
                    P1BFRef.SetStormWeather();
                    P2BFRef.SetStormWeather();
                    SFXManager.instance.Play("Storm_Weather");
                    break;
                }
            case "ClearWeather":
                {
                    P1BFRef.SetClearWeather();
                    P2BFRef.SetClearWeather();
                    break;
                }
            default:
                {
                    Debug.Log("Not a valid weather card.");
                    break;
                }

        }
        // move card out of hand
        if (turn == 1)
        {
            P1Cards--;
            if (P1Cards == 0)
                ForcePass(1);
        }
        else
        {
            P2Cards--;
            if(P2Cards==0)
                ForcePass(2);
        }
        // destroy:

        Destroy(cardDeploying.gameObject);
        
        ChangeTurn();
        CloseDetailsMenu();
    }

    public void DeploySpecialBooster(string type)
    {
        // whose turn, and which type of booster
        switch (type)
        {
            case "Booster_Frontline":
                {
                    if (turn == 1)
                        P1BFRef.AddBooster(1, cardDeploying);
                    else
                        P2BFRef.AddBooster(1, cardDeploying);
                    break;
                }
            case "Booster_Vantage":
                {
                    if (turn == 1)
                        P1BFRef.AddBooster(2, cardDeploying);
                    else
                        P2BFRef.AddBooster(2, cardDeploying);
                    break;
                }
            case "Booster_Shadow":
                {
                    if (turn == 1)
                        P1BFRef.AddBooster(3, cardDeploying);
                    else
                        P2BFRef.AddBooster(3, cardDeploying);
                    break;
                }
        }
        if (turn == 1)
        {
            P1Cards--;
            if (P1Cards == 0)
                ForcePass(1);
        }
        else
        {
            P2Cards--;
            if (P2Cards == 0)
                ForcePass(2);
        }
        

        ChangeTurn();
        CloseDetailsMenu();

        SFXManager.instance.Play("Booster");
    }

    
    //------------------------------------------------------------------------------

    // Deployment Functions Above
    //___________________________________________________________________________________________________________________
    //
    // Other Supporting Functions
    //___________________________________________________________________________________________________________________
    public void ChangeTurn()        // Changes the turn, also calls flip decks function, also ends round when both players pass
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
                if(round==3 || round==2)
                    Invoke("EndOfRound", 1f);       //need to delay function call otherwise wont register last deployed card.
                else
                    EndOfRound();
                Debug.Log("End of round.");
            }
        }
    }


    void EndOfRound()               // is called when both players pass, and round ends, saves scores and changes lives count depending on round outcome
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

    void RoundStatus()              // checks if match is over then calls Endgame(), other wise starts next round and calls reinit()
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
            // Round UI (change to function
            RoundUI.text = "Round: " + round;
            Reinitialize();
            SFXManager.instance.Play("EndOfRound");
        }
    }


    void Reinitialize()             // resets everything, so that you can play next round, cleans up board, resets weather, scores, boosters, etc.
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

        //reset weathers
        P1BFRef.ResetWeather();
        P2BFRef.ResetWeather();

        //reset boosters
        P1BFRef.ResetBoosters();
        P2BFRef.ResetBoosters();

        RemoveDeployedCards();
        if (P1Cards == 0)
            ForcePass(1);
        if (P2Cards == 0)
            ForcePass(2);
    }


    
    private void GenerateHand(int PlayerID)     //generate hand function
    {
        int count = 10;
        float yOffset;
        float xOffset = -2;
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
        List<int> drawSequence = GenerateRandomIndices(maxCards);       // for spy cards, save this sequence

        for (int i = 0; i < count; i++)
        {
            // fetch signature:
            GameObject card;
            card = deck.GetComponent<Deck>().CardsDeck[drawSequence[i]];

            // instantiate
            if (PlayerID == 1)
            {
                GameObject temp = Instantiate(card, p1HandRef);
                temp.GetComponent<Card>().SetCardStatus("Hand");
                temp.transform.position = new Vector3(xOffset, yOffset, 0);
                xOffset += 0.75f;
            }
            else
            {
                GameObject temp = Instantiate(card, p2HandRef);
                temp.GetComponent<Card>().SetCardStatus("Hand");
                temp.transform.position = new Vector3(xOffset, yOffset, 0);
                xOffset += 0.75f;
            }
        }

        // save sequence to battlefield so further drawing is possible
        if (PlayerID == 1)
            P1BFRef.InitSequence(drawSequence);
        else if (PlayerID == 2)
            P2BFRef.InitSequence(drawSequence);
    }



    // can let battlefield do this instead, but find if you keep it here.
    void RemoveDeployedCards()                  // moves all board (deployed cards) to discard pile (physically)
    {
        int count = 0;
        //while (count<P1TotalCards)
        while (count<p1HandRef.childCount)
        {
            if (p1HandRef.GetChild(count).GetComponent<Card>().GetCardStatus() == "Deployed")
            {
                GameObject card = p1HandRef.GetChild(count).gameObject;
                //set status to discard, rotate and move to discard pile
                card.GetComponent<Card>().SetCardStatus("Discard");
                card.transform.Rotate(new Vector3(0,180,0));
                card.transform.position = new Vector3(p1DiscardXPos,p1DiscardYPos);
                count++;
            }
            else
                count++;
        }
        //p2
        count = 0;
        //while (count < P2TotalCards)
        while (count < p2HandRef.childCount)
        {
            if (p2HandRef.GetChild(count).GetComponent<Card>().GetCardStatus() == "Deployed")
            {
                GameObject card = p2HandRef.GetChild(count).gameObject;
                //set status to discard, rotate and move to discard pile
                card.GetComponent<Card>().SetCardStatus("Discard");
                card.transform.Rotate(new Vector3(0, 180, 0));
                card.transform.position = new Vector3(p2DiscardXPos, p2DiscardYPos);
                count++;
            }
            else
                count++;
        }
    }


    void EndgameStats()             // shows end of game stats on screen, each round scores and who won
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

        if (p1Lives == 0 && p2Lives == 0)
            endgameScriptRef.SetWinner(0);
        else if (p1Lives == 0 && p2Lives > 0)
            endgameScriptRef.SetWinner(2);
        else
            endgameScriptRef.SetWinner(1);

        SFXManager.instance.Play("Endgame");
    }

    //____________________________________________________________________________________________________________________________________________
    // Other helper functions below
    

 
    void CardScaling()              // scales card on which you hover mouse to make it clear what you are selecting
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

    void FlipCardsInDeck(int ID)               // flips cards 180 degree when its not your turn
    {
        if (hideOpponentCards)
        {
            GameObject card;
            if (ID == 1)
            {
                int count = 0;
                while (count < p1HandRef.childCount)
                {
                    card = p1HandRef.GetChild(count).gameObject;
                    if (card.GetComponent<Card>().GetCardStatus() == "Hand")
                        card.transform.Rotate(new Vector3(0,180,0));
                    count++;
                }
            }
            else if (ID == 2)
            {
                int count = 0;
                while (count < p2HandRef.childCount)
                {
                    card = p2HandRef.GetChild(count).gameObject;
                    if(card.GetComponent<Card>().GetCardStatus() =="Hand")
                        card.transform.Rotate(new Vector3(0,180,0));
                    count++;
                }
            }
            
        }

    }

    void UpdateLivesUI()                        // ui lives (gems) on screen
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



    List<int> GenerateRandomIndices(int deckSize)               // used by GenerateHand() to use a unique sequence of indices which is used to draw cards
    {
        List<int> sequence = new List<int>();
        while (sequence.Count < 10)
        {
            int temp = Random.Range(0,deckSize);
            if (!sequence.Contains(temp))
                sequence.Add(temp);
        }
        return sequence;

    }
}
