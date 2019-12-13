using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    GameObject raycastTarget;

    [Range(1,2)]
    public int turn=1;

    public GameObject P1Battlefield;
    public GameObject P2Battlefield;

    Battlefield P1BFRef;
    Battlefield P2BFRef;

    // ui done from p1_battlefield

    // references to both player decks


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
    
    void Start()
    {

        // random turn
        turn = Random.Range(1, 2);

        P1BFRef = P1Battlefield.GetComponent<Battlefield>();
        P2BFRef = P2Battlefield.GetComponent<Battlefield>();

        P1Pass = P1PassRef.GetComponent<Button>();
        P2Pass = P2PassRef.GetComponent<Button>();

        // fetch info
        gameinfo = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameStarter>();




        //init
        InitializeGame();
        //Debug.Log(gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[0].GetComponent<Card>().name);
    }

    private void InitializeGame()
    {
        // randomly select cards from respective decks.

        // for now instantiate it on the field
        //p1
        GenerateHand(1);
        GenerateHand(2);
        
        //p2
    }


    void Update()
    {
        GetCameraRaycast();
        //DisplayRaycastTarget();

        //for scaling
        CardScaling();
        PassButtonController();
      
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


    void DisplayRaycastTarget()
    {
        if (raycastTarget != null)
            Debug.Log("Card: " + raycastTarget.transform.name);
        //else
            //Debug.Log("No card:");
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

        for (int i = 0; i < count; i++)
        {
            // fetch signature:
            //Debug.Log(gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[0].GetComponent<Card>().name);
            GameObject card;
            //card = deck.GetComponent<Deck>().CardsDeck[Random.Range(0, deck.GetComponent<Deck>().totalCards)];
            card = deck.GetComponent<Deck>().CardsDeck[Random.Range(0, 7)];
            // instantiate
            if (PlayerID == 1)
            {
                GameObject temp=Instantiate(card, p1HandRef);
                temp.transform.position = new Vector3(xOffset,yOffset,0);
                xOffset += 0.75f;
            }
            else
            {
                GameObject temp=Instantiate(card, p2HandRef);
                temp.transform.position = new Vector3(xOffset, yOffset, 0);
                xOffset += 0.75f;
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
        //Debug.Log("Change Turn Called: current turn: "+turn);

        if (turn == 2)
        {
            if (!P1BFRef.playerPassed)
                turn = 1;
        }
        else if (turn == 1)
        {
            if(!P2BFRef.playerPassed)
                turn = 2;
        }

        
        if (P1BFRef.playerPassed && P2BFRef.playerPassed)
        {
            //call end of round function
            Debug.Log("End of round.");
            if (P1BFRef.totalScore > P2BFRef.totalScore)
            {
                Debug.Log("Player 1 Won!");
            }
            else if (P1BFRef.totalScore == P2BFRef.totalScore)
            {
                Debug.Log("Tied");
            }
            else if( P1BFRef.totalScore < P2BFRef.totalScore)
            {
                Debug.Log("Player 2 Won!");
            }
        }
        
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
        {
            //P1BFRef.SetPassed();
            //P1Pass.interactable = false;
            P1Pass.gameObject.GetComponent<PassRound>().Pass();
            //Debug.Log("Force Passed on button ID: "+P1Pass.GetComponent<PassRound>().PlayerID);
        }
        else if (ID==2)
        {
            //P2BFRef.SetPassed();
            //P2Pass.interactable = false;
            P2Pass.gameObject.GetComponent<PassRound>().Pass();
            //Debug.Log("Force Passed on button ID: " + P2Pass.GetComponent<PassRound>().PlayerID);
        }
        //ChangeTurn();             //Pass () does it
    }


  
    
}
