using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{


    GameObject raycastTarget;

    public GameObject P1Frontlines;
    public GameObject P1Vantage;
    public GameObject P1Shadow;

    // ui
    public Text P1Frontlines_Score;
    public Text P1Vantage_Score;
    public Text P1Shadow_Score;
    public Text Total_Score;

    


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetCameraRaycast();
        //DisplayRaycastTarget();

        //for scaling
        if (raycastTarget != null)
        {
            raycastTarget.GetComponent<CardScaler>().underCursor=true;
        }
      
    }


    void GetCameraRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            raycastTarget = hit.collider.gameObject;
        }
        else
        {
            raycastTarget = null;
        }


        // input
        if (Input.GetMouseButton(0) && hit.collider != null)
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
        //delete, spawn as a child of respective zone.


        //reset position:
        //Vector3 newCardPos = new Vector3(0,0,0);
        //card.GetComponent<RectTransform>().position = newCardPos; 
        

        //save a temp ref to script
        Card cardRef = card.GetComponent<Card>();

        if (cardRef.info.GetUnitType() == "Warrior")
        {
            // place on frontline
            Card temp=Instantiate(cardRef, P1Frontlines.transform);
            temp.GetComponent<Card>().SetCardStatus("Deployed");
            temp.GetComponent<CardScaler>().deployed = true;
            
            
            // delete
            Destroy(card.gameObject);
        }
        else if (cardRef.info.GetUnitType() == "Mage" || cardRef.info.GetUnitType() == "Spellsword")
        {
            // place on Vantage
        }
        else if (cardRef.info.GetUnitType() == "Shadow")
        {
            // place on shadow
        }
       

    }
}
