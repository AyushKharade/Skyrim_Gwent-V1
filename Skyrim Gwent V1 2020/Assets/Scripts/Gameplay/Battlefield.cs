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

    // Weather particle systems
    [Header("Weather Particle Systems")]
    public GameObject FrostParticleSystem;

    // baneAetherius
    public GameObject BaneAetheriusParticleSystem;


    private void Start()
    {
        frontline = new LinkedList<GameObject>();
        vantage = new LinkedList<GameObject>();
        shadow = new LinkedList<GameObject>();
        discardpile = new LinkedList<GameObject>();


        // turn on weather particle systems:
        FrostParticleSystem.GetComponent<ParticleSystem>().Pause();
        BaneAetheriusParticleSystem.GetComponent<ParticleSystem>().Pause();
    }

    private void Update()
    {
        UpdateScoreUI();
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
        //adjust scores according to buffs or debuffs (Phase 2)
        if (frostbite && !UnitCard.GetComponent<Card>().info.isHero)
        {
            UnitCard.GetComponent<Card>().info.AddDeBuff(100);
            UnitCard.GetComponent<Card>().DebuffColorEffect();
        }
        frontlineScore += UnitCard.GetComponent<Card>().info.strength;
        
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
            frontline.RemoveFirst();
        }
        while (vantage.Count > 0)
        {
            discardpile.AddLast(vantage.First.Value);
            vantage.RemoveFirst();
        }
        while (shadow.Count > 0)
        {
            discardpile.AddLast(shadow.First.Value);
            shadow.RemoveFirst();
        }

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

        ResetWeatherDebuffs();
        MoveToDiscardPile();
        playerPassed = false;

    }

    void ResetWeatherDebuffs()
    {
        // if any weather was active, make sure to reset all buffs on the cards in that zone
        // iterate and call reset functions on card & info
        if (frostbite)
        {
            foreach (GameObject g in frontline)
            {
                g.GetComponent<Card>().info.ResetBuffs();
                g.GetComponent<Card>().ResetBuffColorEffect();
                g.GetComponent<Card>().UI_Update();
            }
        }
        if (baneAetherius)
        {
            foreach (GameObject g in vantage)
            {
                g.GetComponent<Card>().info.ResetBuffs();
                g.GetComponent<Card>().ResetBuffColorEffect();
                g.GetComponent<Card>().UI_Update();
            }
        }
        if (storm)
        {
            foreach (GameObject g in shadow)
            {
                g.GetComponent<Card>().info.ResetBuffs();
                g.GetComponent<Card>().ResetBuffColorEffect();
                g.GetComponent<Card>().UI_Update();
            }
        }
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


    void UpdateModifiedUnitScores(int i)
    {
        if (i == 1) // frost bite
        {
            int tempScore = 0;
            foreach (GameObject g in frontline)
            {
                string strScore = g.GetComponent<Card>().unitStrength.text + "";
                //Debug.Log("String score is: " + strScore);
                tempScore += int.Parse(strScore);

            }
            //update
            frontlineScore = tempScore;
            UpdateScoreUI();
        }
        else if (i == 2)
        {
            int tempScore = 0;
            foreach (GameObject g in vantage)
            {
                string strScore = g.GetComponent<Card>().unitStrength.text + "";
                tempScore += int.Parse(strScore);
            }
            //update
            vantageScore = tempScore;
            UpdateScoreUI();
        }
        else
        {
            int tempScore = 0;
            foreach (GameObject g in shadow)
            {
                string strScore = g.GetComponent<Card>().unitStrength.text + "";
                tempScore += int.Parse(strScore);
            }
            //update
            shadowScore = tempScore;
            UpdateScoreUI();
        }
    }

    // calling weather functions on any player is enough, do only one call, to keep it simple, call the player battlefield who used the card.
    //weather:
    public void SetFrostbiteWeather()
    {
        if (!frostbite)
        {
            FrostParticleSystem.GetComponent<ParticleSystem>().Play();
            // iterate through front line, set all non-hero's strengh to 1 by calling debuff functions.
            foreach (GameObject g in frontline)
            {
                if (!g.GetComponent<Card>().info.isHero)
                {
                    g.GetComponent<Card>().info.AddDeBuff(100);
                    // call update ui score on card
                    g.GetComponent<Card>().UI_Update();
                    g.GetComponent<Card>().DebuffColorEffect();
                }
            }
            frostbite = true;
            UpdateModifiedUnitScores(1);
        }
    }

    public void SetBaneAetheriusWeather()
    {
        if (!baneAetherius)
        {
            BaneAetheriusParticleSystem.GetComponent<ParticleSystem>().Play();
            // set all vantage (non-hero cards to 1)
            foreach (GameObject g in vantage)
            {
                if (!g.GetComponent<Card>().info.isHero)
                {
                    g.GetComponent<Card>().info.AddDeBuff(100);
                    // call update ui score on card
                    g.GetComponent<Card>().UI_Update();
                    g.GetComponent<Card>().DebuffColorEffect();
                }
            }
            baneAetherius = true;
            UpdateModifiedUnitScores(2);
        }
    }

    public void SetStormWeather()
    { }

    public void SetClearWeather()
    {
        // this is for clear weather card
        FrostParticleSystem.GetComponent<ParticleSystem>().Stop();
        frostbite = false;
    }

    public void ResetWeather()
    {
        //this is for end of round reset
        //turn off all particle systems:
        FrostParticleSystem.GetComponent<ParticleSystem>().Stop();

        //turn off all booleans
        frostbite = false;
    }
}
