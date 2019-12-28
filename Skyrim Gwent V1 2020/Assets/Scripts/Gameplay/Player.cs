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

    int P1TotalCards = 10;
    int P2TotalCards = 10;


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
        //check if theres a on display already.
        if (detailedDisplayRef.childCount > 0)
            Destroy(detailedDisplayRef.GetChild(0).gameObject);

        // instantiate a copy in the detailed display area:
        GameObject cardRef=Instantiate(card, detailedDisplayRef);
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
        quoteDetails.text = "" + cardRef.GetComponent<Card>().info.Quotes;
        Debug.Log("" + cardRef.GetComponent<Card>().info.Quotes);

        // hide and show appropriate buttons
        ManageDeployButtons(card);

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
        Destroy(detailedDisplayRef.GetChild(0).gameObject);

        DeployFrontlineButton.gameObject.SetActive(false);
        DeployVantageButton.gameObject.SetActive(false);
        DeployVantage_SpellswordButton.gameObject.SetActive(false);
        DeployShadowButton.gameObject.SetActive(false);
        DeploySpecialButton.gameObject.SetActive(false);
        CloseButton.gameObject.SetActive(false);

        cardDisplaying = false;
        quoteBox.SetActive(false);
    }




    void DeployUnitCard(GameObject card)
    {
        Card cardRef = card.GetComponent<Card>();
        if (cardRef.info.GetUnitType() == "Warrior" && cardRef.GetCardStatus() == "Hand")
        {
            if (turn == 1 && card.transform.parent.name == "Player1_Hand")
            {

                if (cardRef.info.GetSubUnitType() != "Spy")                              // special case
                    P1BFRef.AddUnitToFrontline(card);

                //if spy:
                if (cardRef.info.GetSubUnitType() == "Spy")
                {
                    // set parent to other player, update card no. values
                    //card.transform.Rotate(new Vector3(0, 180, 0));                // dont need to rotate or change turn thrice for some reason??
                    card.transform.position = new Vector3(0, 4.4f, 0);              // probably bec changeturn does work on p2 if turn is not 2
                    card.GetComponent<Card>().SetCardStatus("Hand");
                    card.transform.SetParent(p2HandRef);
                    // place it on other battlefield
                    P2TotalCards++;
                    P1TotalCards--;
                    P2BFRef.AddUnitToFrontline(card);
                    // change turn inside if
                    //ChangeTurn();

                    // loop and draw two cards from deck
                    int maxCards = gameinfo.P1Deck.GetComponent<Deck>().totalCards;
                    int c1Index=P1BFRef.RedrawCard(maxCards);
                    int c2Index=P1BFRef.RedrawCard(maxCards);

                    GameObject c1 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c1Index];
                    GameObject c2 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c2Index];
                    //
                    GameObject c1Ref=Instantiate(c1, p1HandRef);
                    GameObject c2Ref=Instantiate(c2, p1HandRef);

                    c1Ref.transform.Rotate(new Vector3(0, 180, 0));
                    c2Ref.transform.Rotate(new Vector3(0, 180, 0));

                    P1TotalCards += 2;
                    P1Cards += 2;
                    //P1Cards++;                    // required because end of function will also subtract -1

                    // call rearrange function on hand cards
                    P1BFRef.RearrangeHand(p1HandRef, -4.2f);
                }

                ChangeTurn();

                P1Cards--;
                if (P1Cards == 0)
                    ForcePass(1);

            }
            else if (turn == 2 && card.transform.parent.name == "Player2_Hand")
            {
                if (cardRef.info.GetSubUnitType() != "Spy")                              // special case
                    P2BFRef.AddUnitToFrontline(card);
                //if spy:
                if (cardRef.info.GetSubUnitType() == "Spy")
                {
                    // set parent to other player, update card no. values
                    //card.transform.Rotate(new Vector3(0, 180, 0));
                    card.transform.position = new Vector3(0, 4.4f, 0);
                    card.GetComponent<Card>().SetCardStatus("Hand");
                    card.transform.SetParent(p2HandRef);
                    // place it on other battlefield
                    P2TotalCards++;
                    P1TotalCards--;
                    P2BFRef.AddUnitToShadow(card);
                    // change turn inside if
                    //ChangeTurn();

                    // loop and draw two cards from deck
                    int maxCards = gameinfo.P1Deck.GetComponent<Deck>().totalCards;
                    int c1Index = P1BFRef.RedrawCard(maxCards);
                    int c2Index = P1BFRef.RedrawCard(maxCards);

                    GameObject c1 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c1Index];
                    GameObject c2 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c2Index];
                    //
                    GameObject c1Ref = Instantiate(c1, p1HandRef);
                    GameObject c2Ref = Instantiate(c2, p1HandRef);

                    c1Ref.transform.Rotate(new Vector3(0, 180, 0));
                    c2Ref.transform.Rotate(new Vector3(0, 180, 0));

                    P1TotalCards += 2;
                    P1Cards += 2;

                    // call rearrange function on hand cards
                    P1BFRef.RearrangeHand(p1HandRef, -4.2f);
                }
                ChangeTurn();

                P1Cards--;
                if (P1Cards == 0)
                    ForcePass(1);
            }
            
        }

        else if ((cardRef.info.GetUnitType() == "Mage" || cardRef.info.GetUnitType() == "Spellsword") && cardRef.GetCardStatus() == "Hand")
        {
            // place on Vantage -- for now place spellswords on vantage too
            if (turn == 1 && card.transform.parent.name == "Player1_Hand")
            {
                if (cardRef.info.GetSubUnitType() != "Spy")                              // special case
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


                //if spy:
                if (cardRef.info.GetSubUnitType() == "Spy")
                {
                    // set parent to other player, update card no. values
                    //card.transform.Rotate(new Vector3(0, 180, 0));
                    card.transform.position = new Vector3(0, 4.4f, 0);
                    card.GetComponent<Card>().SetCardStatus("Hand");
                    card.transform.SetParent(p2HandRef);
                    // place it on other battlefield
                    P2TotalCards++;
                    P1TotalCards--;
                    P2BFRef.AddUnitToVantage(card);
                    // change turn inside if
                    //ChangeTurn();

                    // loop and draw two cards from deck
                    int maxCards = gameinfo.P1Deck.GetComponent<Deck>().totalCards;
                    int c1Index = P1BFRef.RedrawCard(maxCards);
                    int c2Index = P1BFRef.RedrawCard(maxCards);

                    GameObject c1 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c1Index];
                    GameObject c2 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c2Index];
                    //
                    GameObject c1Ref = Instantiate(c1, p1HandRef);
                    GameObject c2Ref = Instantiate(c2, p1HandRef);

                    c1Ref.transform.Rotate(new Vector3(0, 180, 0));
                    c2Ref.transform.Rotate(new Vector3(0, 180, 0));

                    P1TotalCards += 2;
                    P1Cards += 2;

                    // call rearrange function on hand cards
                    P1BFRef.RearrangeHand(p1HandRef, -4.2f);
                }
                ChangeTurn();

                P1Cards--;
                if (P1Cards == 0)
                    ForcePass(1);
            }

            else if (turn == 2 && card.transform.parent.name == "Player2_Hand")
            {
                if (cardRef.info.GetSubUnitType() != "Spy")                              // special case
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
                //if spy:
                if (cardRef.info.GetSubUnitType() == "Spy")
                {
                    // set parent to other player, update card no. values
                    //card.transform.Rotate(new Vector3(0, 180, 0));
                    card.transform.position = new Vector3(0, 4.4f, 0);
                    card.GetComponent<Card>().SetCardStatus("Hand");
                    card.transform.SetParent(p2HandRef);
                    // place it on other battlefield
                    P2TotalCards++;
                    P1TotalCards--;
                    P2BFRef.AddUnitToShadow(card);
                    // change turn inside if
                    //ChangeTurn();

                    // loop and draw two cards from deck
                    int maxCards = gameinfo.P1Deck.GetComponent<Deck>().totalCards;
                    int c1Index = P1BFRef.RedrawCard(maxCards);
                    int c2Index = P1BFRef.RedrawCard(maxCards);

                    GameObject c1 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c1Index];
                    GameObject c2 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c2Index];
                    //
                    GameObject c1Ref = Instantiate(c1, p1HandRef);
                    GameObject c2Ref = Instantiate(c2, p1HandRef);

                    c1Ref.transform.Rotate(new Vector3(0, 180, 0));
                    c2Ref.transform.Rotate(new Vector3(0, 180, 0));

                    P1TotalCards += 2;
                    P1Cards += 2;

                    // call rearrange function on hand cards
                    P2BFRef.RearrangeHand(p2HandRef, 4.4f);
                }
                ChangeTurn();

                P1Cards--;
                if (P1Cards == 0)
                    ForcePass(1);
            }
            
        }


        else if (cardRef.info.GetUnitType() == "Shadow" && cardRef.GetCardStatus() == "Hand")
        {
            // place on shadow
            if (turn == 1 && card.transform.parent.name == "Player1_Hand")
            {
                if (cardRef.info.GetSubUnitType() != "Spy")                              // special case
                    P1BFRef.AddUnitToShadow(card);
                //if spy:
                if (cardRef.info.GetSubUnitType() == "Spy")
                {
                    // set parent to other player, update card no. values
                    //card.transform.Rotate(new Vector3(0, 180, 0));
                    card.transform.position = new Vector3(0, 4.4f, 0);
                    card.GetComponent<Card>().SetCardStatus("Hand");
                    card.transform.SetParent(p2HandRef);
                    // place it on other battlefield
                    P2TotalCards++;
                    P1TotalCards--;
                    P2BFRef.AddUnitToShadow(card);
                    // change turn inside if
                    //ChangeTurn();

                    // loop and draw two cards from deck
                    int maxCards = gameinfo.P1Deck.GetComponent<Deck>().totalCards;
                    int c1Index = P1BFRef.RedrawCard(maxCards);
                    int c2Index = P1BFRef.RedrawCard(maxCards);

                    GameObject c1 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c1Index];
                    GameObject c2 = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[c2Index];
                    //
                    GameObject c1Ref = Instantiate(c1, p1HandRef);
                    GameObject c2Ref = Instantiate(c2, p1HandRef);

                    c1Ref.transform.Rotate(new Vector3(0, 180, 0));
                    c2Ref.transform.Rotate(new Vector3(0, 180, 0));

                    P1TotalCards += 2;
                    P1Cards += 2;

                    // call rearrange function on hand cards
                    P1BFRef.RearrangeHand(p1HandRef, -4.2f);
                }
                ChangeTurn();

                P1Cards--;
                if (P1Cards == 0)
                    ForcePass(1);
            }
            else if (turn == 2 && card.transform.parent.name == "Player2_Hand")
            {
                if (cardRef.info.GetSubUnitType() != "Spy")                              // special case
                    P2BFRef.AddUnitToShadow(card);

                //if spy:
                if (cardRef.info.GetSubUnitType() == "Spy")
                {
                    // set parent to other player, update card no. values
                    //card.transform.Rotate(new Vector3(0, 180, 0));
                    card.transform.position = new Vector3(0, -4.2f, 0);
                    card.GetComponent<Card>().SetCardStatus("Hand");
                    card.transform.SetParent(p1HandRef);
                    // place it on other battlefield
                    P1TotalCards++;
                    P2TotalCards--;
                    P1BFRef.AddUnitToShadow(card);
                    // change turn inside if
                    //ChangeTurn();

                    // loop and draw two cards from deck
                    int maxCards = gameinfo.P2Deck.GetComponent<Deck>().totalCards;
                    int c1Index = P2BFRef.RedrawCard(maxCards);
                    int c2Index = P2BFRef.RedrawCard(maxCards);

                    GameObject c1 = gameinfo.P2Deck.GetComponent<Deck>().CardsDeck[c1Index];
                    GameObject c2 = gameinfo.P2Deck.GetComponent<Deck>().CardsDeck[c2Index];
                    //
                    GameObject c1Ref = Instantiate(c1, p2HandRef);
                    GameObject c2Ref = Instantiate(c2, p2HandRef);

                    c1Ref.transform.Rotate(new Vector3(0, 180, 0));
                    c2Ref.transform.Rotate(new Vector3(0, 180, 0));

                    P2TotalCards += 2;
                    P2Cards += 2;

                    // call rearrange function on hand cards
                    P2BFRef.RearrangeHand(p2HandRef, 4.4f);
                }

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
                    SFXManager.instance.Play("Frostbite_Weather");
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
                {
                    P1BFRef.AddBooster(1, card);
                    SFXManager.instance.Play("Booster");
                }
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
                    SFXManager.instance.Play("Frostbite_Weather");
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
                {
                    P2BFRef.AddBooster(1,card);
                    SFXManager.instance.Play("Booster");
                }
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
