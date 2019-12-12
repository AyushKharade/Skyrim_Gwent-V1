using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    GameObject raycastTarget;

    [Range(1,2)]
    public int turn;

    public GameObject P1Battlefield;
    public GameObject P2Battlefield;

    Battlefield P1BFRef;
    Battlefield P2BFRef;

    // ui done from p1_battlefield
    
    void Start()
    {
        P1BFRef = P1Battlefield.GetComponent<Battlefield>();
        P2BFRef = P2Battlefield.GetComponent<Battlefield>();
    }


    void Update()
    {
        GetCameraRaycast();
        //DisplayRaycastTarget();

        //for scaling
        CardScaling();

      
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
            P1BFRef.AddUnitToFrontline(card);
            // swap turn
        }


        

        else if ((cardRef.info.GetUnitType() == "Mage" || cardRef.info.GetUnitType() == "Spellsword") && cardRef.GetCardStatus() == "Hand")
        {
            // place on Vantage -- for now place spellswords on vantage too
            P1BFRef.AddUnitToVantage(card);
        }

        

        else if (cardRef.info.GetUnitType() == "Shadow" && cardRef.GetCardStatus() == "Hand")
        {
            // place on shadow
            P1BFRef.AddUnitToShadow(card);
        }
       

    }






    // extra
    void CardScaling()
    {
        if (raycastTarget != null)
            raycastTarget.GetComponent<CardScaler>().underCursor = true;

    }
}
