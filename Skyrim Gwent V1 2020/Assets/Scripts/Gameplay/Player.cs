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

    public int P1TotalCards = 10;
    public int P2TotalCards = 10;


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
        // so palyers cant click right away
        TurnOnControlLock();
    }

    private void InitializeGame()           // generate initial hand for both players
    {
        GenerateHand(1);
        GenerateHand(2);
        // since prefabs are not being changed anymore, there is no need to resetdeck() if the two decks are the same
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
    }


    void DeployUnitCard(GameObject card)
    {
        Card cardRef = card.GetComponent<Card>();
        if (cardRef.info.GetUnitType() == "Warrior" && cardRef.GetCardStatus() == "Hand")
        {
            if (turn == 1 && card.transform.parent.name == "Player1_Hand")
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

                //if healer, for now randomly redeploy a card into battlefield.
                if (cardRef.info.GetSubUnitType() == "Healer")
                {
                    GameObject RedeployedUnit = P1BFRef.MedicReDeploy();
                    if (RedeployedUnit != null)
                    {
                        Debug.Log("Redeploying Card. " + RedeployedUnit.GetComponent<Card>().info.name);
                        // rotate and change status to hand for redeployment to work.
                        RedeployedUnit.transform.Rotate(new Vector3(0, 180, 0));
                        RedeployedUnit.transform.position = new Vector3(0, -4.2f, 0);          // 4.4 & -4.2
                        RedeployedUnit.GetComponent<Card>().SetCardStatus("Hand");
                        P1Cards++;
                        DeployUnitCard(RedeployedUnit);
                        ChangeTurn();                                      // needed other same player gets the turn  again
                        SFXManager.instance.Play("Medic_Redeploy");
                    }

                }

                // necromancer
                if (cardRef.info.GetSubUnitType() == "Necromancer")
                {
                    GameObject RedeployedUnit = P2BFRef.MedicReDeploy();          // use same function as medic, but dont forget to remove from other player's pile
                    if (RedeployedUnit != null)
                    {
                        RedeployedUnit.transform.Rotate(new Vector3(0, 180, 0));
                        RedeployedUnit.transform.position = new Vector3(0, -4.2f, 0);
                        RedeployedUnit.GetComponent<Card>().SetCardStatus("Hand");
                        RedeployedUnit.transform.SetParent(p1HandRef);                    // change parent, so its a player 1 card now

                        P1Cards++;
                        P1TotalCards++;                             // these variables so flipdecks() dont bug out
                        P2TotalCards--;

                        DeployUnitCard(RedeployedUnit);
                        ChangeTurn();                                      // needed otherwise same player gets the turn  again
                    }
                }
                ChangeTurn();

                P1Cards--;
                if (P1Cards == 0)
                    ForcePass(1);
            }

            else if (turn == 2 && card.transform.parent.name == "Player2_Hand")
            {
                P2BFRef.AddUnitToVantage(card);
                //ChangeTurn();

                //if healer, for now randomly redeploy a card into battlefield.
                if (cardRef.info.GetSubUnitType() == "Healer")
                {
                    GameObject RedeployedUnit = P2BFRef.MedicReDeploy();
                    if (RedeployedUnit != null)
                    {
                        //Debug.Log("Redeploying Card. " + RedeployedUnit.GetComponent<Card>().info.name);
                        RedeployedUnit.transform.Rotate(new Vector3(0, 180, 0));        // rotate and change status to hand for redeployment to work.
                        RedeployedUnit.GetComponent<Card>().SetCardStatus("Hand");
                        RedeployedUnit.transform.position = new Vector3(0, 4.4f, 0);          // 4.4 & -4.2
                        P2Cards++;              // because deploy function decrements
                        DeployUnitCard(RedeployedUnit);
                        ChangeTurn();

                        SFXManager.instance.Play("Medic_Redeploy");
                    }

                }

                // necromancer
                if (cardRef.info.GetSubUnitType() == "Necromancer")
                {
                    GameObject RedeployedUnit = P1BFRef.MedicReDeploy();          // use same function as medic, but dont forget to remove from other player's pile
                    if (RedeployedUnit != null)
                    {
                        RedeployedUnit.transform.Rotate(new Vector3(0, 180, 0));
                        RedeployedUnit.transform.position = new Vector3(0, 4.4f, 0);
                        RedeployedUnit.GetComponent<Card>().SetCardStatus("Hand");
                        RedeployedUnit.transform.SetParent(p2HandRef);                    // change parent, so its a player 2 card now

                        P2Cards++;
                        P1TotalCards--;                             // these variables so flipdecks() dont bug out
                        P2TotalCards++;

                        DeployUnitCard(RedeployedUnit);
                        ChangeTurn();                                      // needed otherwise same player gets the turn  again
                    }
                }

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

        //weather -- make sure to set card to discard later
        else if (cardRef.info.GetUnitType() == "Special")
        {
            if (turn == 1 && card.transform.parent.name == "Player1_Hand")
            {
                if (cardRef.info.GetSubUnitType() == "FrostWeather")
                {
                    P1BFRef.SetFrostbiteWeather();
                    P2BFRef.SetFrostbiteWeather();
                    card.transform.Translate(new Vector3(0, -2, 0));
                }
                else if (cardRef.info.GetSubUnitType() == "BaneAetheriusWeather")
                {
                    P1BFRef.SetBaneAetheriusWeather();
                    P2BFRef.SetBaneAetheriusWeather();
                    card.transform.Translate(new Vector3(0, -2, 0));
                }
                else if (cardRef.info.GetSubUnitType() == "StormWeather")
                {
                    P1BFRef.SetStormWeather();
                    P2BFRef.SetStormWeather();
                    card.transform.Translate(new Vector3(0, -2, 0));
                    SFXManager.instance.Play("Storm_Weather");
                }
                else if (cardRef.info.GetSubUnitType() == "ClearWeather")
                {
                    P1BFRef.SetClearWeather();
                    P2BFRef.SetClearWeather();
                    card.transform.Translate(new Vector3(0, -2, 0));
                }


                // boosters:
                else if (cardRef.info.GetSubUnitType() == "Booster_Frontline")
                { }
                else if (cardRef.info.GetSubUnitType() == "Booster_Vantage")
                {
                    //place at vantage booster offset.
                    // call function on battlefield.
                    P1BFRef.AddBooster(2, card);
                    SFXManager.instance.Play("Booster");
                }



                ChangeTurn();

                P1Cards--;
                if (P1Cards == 0)
                    ForcePass(1);
            }
            //for player 2
            else if (turn == 2 && card.transform.parent.name == "Player2_Hand")
            {
                if (cardRef.info.GetSubUnitType() == "FrostWeather")
                {
                    P2BFRef.SetFrostbiteWeather();
                    P1BFRef.SetFrostbiteWeather();
                    card.transform.Translate(new Vector3(0, 2, 0));
                }
                //else if baneaetherius
                else if (cardRef.info.GetSubUnitType() == "BaneAetheriusWeather")
                {
                    P1BFRef.SetBaneAetheriusWeather();
                    P2BFRef.SetBaneAetheriusWeather();
                    card.transform.Translate(new Vector3(0, 2, 0));
                }
                else if (cardRef.info.GetSubUnitType() == "StormWeather")
                {
                    P1BFRef.SetStormWeather();
                    P2BFRef.SetStormWeather();
                    card.transform.Translate(new Vector3(0, 2, 0));
                    //audio
                    SFXManager.instance.Play("Storm_Weather");
                }
                else if (cardRef.info.GetSubUnitType() == "ClearWeather")
                {
                    P1BFRef.SetClearWeather();
                    P2BFRef.SetClearWeather();
                    card.transform.Translate(new Vector3(0, 2, 0));
                }

                // boosters:
                else if (cardRef.info.GetSubUnitType() == "Booster_Frontline")
                { }
                else if (cardRef.info.GetSubUnitType() == "Booster_Vantage")
                {
                    //place at vantage booster offset.
                    // call function on battlefield.
                    P2BFRef.AddBooster(2, card);
                    SFXManager.instance.Play("Booster");
                }

                ChangeTurn();

                P2Cards--;
                if (P2Cards == 0)
                    ForcePass(2);
            }
        }

        // no match for card type exception
        else
            Debug.Log("No match found for deploying this card: Info:, name: "+cardRef.info.name+", type: "+cardRef.info.GetUnitType()+
                ", hand: "+card.transform.parent.name+", current turn: "+turn+", state: "+cardRef.GetCardStatus());
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
                if(round==3 || round==2)
                    Invoke("EndOfRound", 1f);       //need to delay function call otherwise wont register last deployed card.
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

    void RemoveDeployedCards()
    {
        
        int count = 0;
        while (count<P1TotalCards)
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
        while (count < P2TotalCards)
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

        if (p1Lives == 0 && p2Lives == 0)
            endgameScriptRef.SetWinner(0);
        else if (p1Lives == 0 && p2Lives > 0)
            endgameScriptRef.SetWinner(2);
        else
            endgameScriptRef.SetWinner(1);

        SFXManager.instance.Play("Endgame");
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

    void FlipCardsInDeck(int ID)
    {
        if (hideOpponentCards)
        {
            GameObject card;
            if (ID == 1)
            {
                int count = 0;
                while (count < P1TotalCards)
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
                while (count < P2TotalCards)
                {
                    card = p2HandRef.GetChild(count).gameObject;
                    if(card.GetComponent<Card>().GetCardStatus() =="Hand")
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



    // random but unique sequence for drawing cards
    List<int> GenerateRandomIndices(int deckSize)
    {
        // to replace old card drawing:
        List<int> sequence = new List<int>();

        while (sequence.Count < 10)
        {
            int temp = Random.Range(0,deckSize);
            if (!sequence.Contains(temp))
                sequence.Add(temp);
        }

        /*
        string str="";
        Debug.Log("Displaying all sequence.");
        foreach (int a in sequence)
            str += (a+" ");

        Debug.Log(str);
        */
        return sequence;

    }
}
