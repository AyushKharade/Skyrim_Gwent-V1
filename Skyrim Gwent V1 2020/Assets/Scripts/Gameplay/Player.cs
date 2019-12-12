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
    int P1Cards = 8;
    int P2Cards = 8;

    
    void Start()
    {
        P1BFRef = P1Battlefield.GetComponent<Battlefield>();
        P2BFRef = P2Battlefield.GetComponent<Battlefield>();

        P1Pass = P1PassRef.GetComponent<Button>();
        P2Pass = P1PassRef.GetComponent<Button>();
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




    void DeployUnitCard(GameObject card)
    {
        Card cardRef = card.GetComponent<Card>();
        if (cardRef.info.GetUnitType() == "Warrior" && cardRef.GetCardStatus()=="Hand")
        {
            if (turn == 1 && card.transform.parent.name=="Player1_Hand")
            {
                P1BFRef.AddUnitToFrontline(card);
                ChangeTurn();
            }
            else if (turn == 2 && card.transform.parent.name == "Player2_Hand")
            {
                P2BFRef.AddUnitToFrontline(card);
                ChangeTurn(); 
            }
        }


        

        else if ((cardRef.info.GetUnitType() == "Mage" || cardRef.info.GetUnitType() == "Spellsword") && cardRef.GetCardStatus() == "Hand")
        {
            // place on Vantage -- for now place spellswords on vantage too
            if (turn == 1 && card.transform.parent.name == "Player1_Hand")
            {
                P1BFRef.AddUnitToVantage(card);
                ChangeTurn();
            }
            else if (turn == 2 && card.transform.parent.name == "Player2_Hand")
            {
                P2BFRef.AddUnitToVantage(card);
                ChangeTurn();
            }
        }

        

        else if (cardRef.info.GetUnitType() == "Shadow" && cardRef.GetCardStatus() == "Hand")
        {
            // place on shadow
            if (turn == 1 && card.transform.parent.name == "Player1_Hand")
            {
                P1BFRef.AddUnitToShadow(card);
                ChangeTurn();
            }
            else if (turn == 2 && card.transform.parent.name == "Player2_Hand")
            {
                P2BFRef.AddUnitToShadow(card);
                ChangeTurn();
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


  
    
}
