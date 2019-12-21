/*
 * This script will be used to take care of the 3 battle zones for players. 
 * This has 3 lists for storing all cards, and rearrange functions for properly displaying cards.
 * 2 instantiations of this script will be present in the game
 * this also takes care of UI
 * 
 * In future, also add discard pile and starting hand lists
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//variables

public class Battlefield : MonoBehaviour
{
    public string PlayerName;
    [Range(1, 2)]
    public int PlayerID;
    public bool playerPassed=false;

    [HideInInspector] public LinkedList<GameObject> frontline;
    [HideInInspector] public LinkedList<GameObject> vantage;
    [HideInInspector] public LinkedList<GameObject> shadow;
    [HideInInspector] public LinkedList<GameObject> discardpile;


    // weather effects
    bool frostbite;     // affects melee units
    bool baneAetherius; // affects mages
    bool storm;         // affects archers, thiefs and assassins.

    [HideInInspector]public int frontlineScore=0;
    [HideInInspector] public int vantageScore=0;
    [HideInInspector] public int shadowScore=0;
    [HideInInspector] public int totalScore=0;

    //UI
    [Header("Score UI")]
    public Text frontlineScoreUI;
    public Text vantageScoreUI;
    public Text shadowScoreUI;
    public Text totalScoreUI;


    // card position adjustment parameters
    [Header("Placement Offsets")]
    public float frontlinePosY;
    public float vantagePosY;
    public float shadowPosY;

    //x
    float frontlinePosX=-0.5f;
    float vantagePosX=-0.5f;
    float shadowPosX=-0.5f;
    public float additionOffsetX = 0.75f;

    // public varaibles for checking discard pile
    public int FrontlineContent;
    public int DiscardPileContent;

    private void Start()
    {
        frontline = new LinkedList<GameObject>();
        vantage = new LinkedList<GameObject>();
        shadow = new LinkedList<GameObject>();
        discardpile = new LinkedList<GameObject>();
    }

    private void Update()
    {
        UpdateScoreUI();
        FrontlineContent = frontline.Count;
        DiscardPileContent = discardpile.Count;
    }

  


    // public add cards to any fields
    public void AddUnitToFrontline(GameObject UnitCard)
    {
        
        UnitCard.GetComponent<CardScaler>().deployed = true;                            // set cardrotator to deployed
        UnitCard.GetComponent<Card>().SetCardStatus("Deployed");                        // set status deployed

        //Debug.Log("Unit Received: "+ UnitCard.GetComponent<Card>().info.name);
        UnitCard.transform.Translate(new Vector3(0, frontlinePosY, 0));
        UnitCard.transform.position = new Vector3(frontlinePosX,UnitCard.transform.position.y,0);
        frontlinePosX += additionOffsetX;


        frontline.AddLast(UnitCard);
        frontlineScore += UnitCard.GetComponent<Card>().info.strength;
        
        //adjust scores according to buffs or debuffs (Phase 2)
        // update positions
        
    }

    public void AddUnitToVantage(GameObject UnitCard)
    {
        UnitCard.GetComponent<CardScaler>().deployed = true;                            // set cardrotator to deployed
        UnitCard.GetComponent<Card>().SetCardStatus("Deployed");                        // set status deployed

        //Debug.Log("Unit Received: "+ UnitCard.GetComponent<Card>().info.name);
        UnitCard.transform.Translate(new Vector3(0, vantagePosY, 0));
        UnitCard.transform.position = new Vector3(vantagePosX, UnitCard.transform.position.y, 0);
        vantagePosX += additionOffsetX;

        vantage.AddLast(UnitCard);
        vantageScore += UnitCard.GetComponent<Card>().info.strength;
    }


    public void AddUnitToShadow(GameObject UnitCard)
    {
        UnitCard.GetComponent<CardScaler>().deployed = true;                            // set cardrotator to deployed
        UnitCard.GetComponent<Card>().SetCardStatus("Deployed");                        // set status deployed

        //Debug.Log("Unit Received: "+ UnitCard.GetComponent<Card>().info.name);
        UnitCard.transform.Translate(new Vector3(0, shadowPosY, 0));
        UnitCard.transform.position = new Vector3(shadowPosX, UnitCard.transform.position.y, 0);
        shadowPosX += additionOffsetX;

        shadow.AddLast(UnitCard);
        shadowScore += UnitCard.GetComponent<Card>().info.strength;
    }


    private void MoveToDiscardPile()
    { // move all cards in all decks to discard}

        while (frontline.Count > 0)
        {
            discardpile.AddLast(frontline.First.Value);
            //GameObject temp = frontline.First.Value;
            frontline.RemoveFirst();
        }
        if (vantage.Count > 0)
        {

        }
        if (shadow.Count > 0)
        {
        }



        // clear lists
        frontline.Clear();
        vantage.Clear();
        shadow.Clear();

        //reset scores
        frontlineScore = 0;
        vantageScore = 0;
        shadowScore = 0;
        totalScore = 0;

        // reset placement offsets
        frontlinePosX = -0.5f;
        vantagePosX = -0.5f;
        shadowPosX = -0.5f;
    }

    





    //reset battlefield
    public void Reset()
    {
        MoveToDiscardPile();
        playerPassed = false;
    }




    // ui
    public void UpdateScoreUI()
    {
        frontlineScoreUI.text = "" + frontlineScore;
        vantageScoreUI.text = "" + vantageScore;
        shadowScoreUI.text = "" + shadowScore;

        // total score
        totalScore = frontlineScore + vantageScore + shadowScore;
        totalScoreUI.text = "" + totalScore;
    }



    // passing
    public void SetPassed()
    {
        playerPassed = true;
    }
}
