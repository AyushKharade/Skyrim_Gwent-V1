/*
 * This script will be used to take care of the 3 battle zones for players. 
 * This has 3 lists for storing all cards, and rearrange functions for properly displaying cards.
 * 2 instantiations of this script will be present in the game
 * this also takes care of UI
 * 
 * Takes care of weather cards.
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
    public bool playerPassed = false;

    [HideInInspector] public LinkedList<GameObject> frontline;
    [HideInInspector] public LinkedList<GameObject> vantage;
    [HideInInspector] public LinkedList<GameObject> shadow;
    [HideInInspector] public LinkedList<GameObject> discardpile;


    // weather effects
    bool frostbite;     // affects melee units
    bool baneAetherius; // affects mages
    bool storm;         // affects archers, thiefs and assassins.

    //booster effects
    bool frontlineBoost;
    bool vantageBoost;
    bool shadowBoost;

    [HideInInspector] public int frontlineScore = 0;
    [HideInInspector] public int vantageScore = 0;
    [HideInInspector] public int shadowScore = 0;
    [HideInInspector] public int totalScore = 0;

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
    float frontlinePosX = -0.5f;
    float vantagePosX = -0.5f;
    float shadowPosX = -0.5f;
    public float additionOffsetX = 0.75f;

    // Weather particle systems
    [Header("Weather Particle Systems")]
    public GameObject FrostParticleSystem;
    public GameObject BaneAetheriusParticleSystem;
    public GameObject StormParticleSystem;


    // store sequence of cards drawn to support re-draw and spy cards.
    public List<int> drawnSequence;

    private void Start()
    {
        frontline = new LinkedList<GameObject>();
        vantage = new LinkedList<GameObject>();
        shadow = new LinkedList<GameObject>();
        discardpile = new LinkedList<GameObject>();


        // turn on weather particle systems:
        FrostParticleSystem.GetComponent<ParticleSystem>().Stop();
        BaneAetheriusParticleSystem.GetComponent<ParticleSystem>().Stop();
        StormParticleSystem.GetComponent<ParticleSystem>().Stop();
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
        UnitCard.transform.position = new Vector3(frontlinePosX, UnitCard.transform.position.y, 0);
        frontlinePosX += additionOffsetX;


        frontline.AddLast(UnitCard);
        //adjust scores according to buffs or debuffs (Phase 2)
        if (frostbite && !UnitCard.GetComponent<Card>().info.isHero)
        {
            UnitCard.GetComponent<Card>().info.AddDeBuff(100);
            UnitCard.GetComponent<Card>().DebuffColorEffect();
        }
        if (frontlineBoost && !UnitCard.GetComponent<Card>().info.isHero)
        {
            UnitCard.GetComponent<Card>().info.AddBuff(UnitCard.GetComponent<Card>().info.strength);
            UnitCard.GetComponent<Card>().BuffColorEffect();
        }
        frontlineScore += UnitCard.GetComponent<Card>().info.strength;


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
        //adjust scores according to buffs or debuffs (Phase 2)
        if (baneAetherius && !UnitCard.GetComponent<Card>().info.isHero)
        {
            UnitCard.GetComponent<Card>().info.AddDeBuff(100);
            UnitCard.GetComponent<Card>().DebuffColorEffect();
        }
        if (vantageBoost && !UnitCard.GetComponent<Card>().info.isHero)
        {
            UnitCard.GetComponent<Card>().info.AddBuff(UnitCard.GetComponent<Card>().info.strength);
            UnitCard.GetComponent<Card>().BuffColorEffect();
        }
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
        //adjust scores according to buffs or debuffs (Phase 2)
        if (storm && !UnitCard.GetComponent<Card>().info.isHero)
        {
            UnitCard.GetComponent<Card>().info.AddDeBuff(100);
            UnitCard.GetComponent<Card>().DebuffColorEffect();
        }
        if (shadowBoost && !UnitCard.GetComponent<Card>().info.isHero)
        {
            UnitCard.GetComponent<Card>().info.AddBuff(UnitCard.GetComponent<Card>().info.strength);
            UnitCard.GetComponent<Card>().BuffColorEffect();
        }
        shadowScore += UnitCard.GetComponent<Card>().info.strength;
    }


    private void MoveToDiscardPile()
    { // move all cards in all decks to discard}

        while (frontline.Count > 0)
        {
            if (!frontline.First.Value.GetComponent<Card>().info.isHero)
                discardpile.AddLast(frontline.First.Value);                 // dont save hero cards, you cannot redeploy them, incase it stays, destroy them
            frontline.RemoveFirst();
        }
        while (vantage.Count > 0)
        {
            if (!vantage.First.Value.GetComponent<Card>().info.isHero)
                discardpile.AddLast(vantage.First.Value);
            vantage.RemoveFirst();
        }
        while (shadow.Count > 0)
        {
            if (!shadow.First.Value.GetComponent<Card>().info.isHero)
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
        ResetWeatherDebuffs(); // also removes any buffs from booster cards
        MoveToDiscardPile();
        playerPassed = false;
    }

    void ResetWeatherDebuffs()
    {
        // if any weather was active, make sure to reset all buffs on the cards in that zone
        // iterate and call reset functions on card & info
        if (frostbite || frontlineBoost)
        {
            foreach (GameObject g in frontline)
            {
                g.GetComponent<Card>().info.ResetBuffs();
                g.GetComponent<Card>().ResetBuffColorEffect();
                g.GetComponent<Card>().UI_Update();
            }
        }
        if (baneAetherius || vantageBoost)
        {
            foreach (GameObject g in vantage)
            {
                g.GetComponent<Card>().info.ResetBuffs();
                g.GetComponent<Card>().ResetBuffColorEffect();
                g.GetComponent<Card>().UI_Update();
            }
        }
        if (storm || shadowBoost)
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
                tempScore += g.GetComponent<Card>().info.strength;
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
                //string strScore = g.GetComponent<Card>().unitStrength.text + "";
                //tempScore += int.Parse(strScore);
                tempScore += g.GetComponent<Card>().info.strength;
            }
            Debug.Log("newly updated vantage score: " + tempScore);
            //update
            vantageScore = tempScore;
            UpdateScoreUI();
        }
        else
        {
            int tempScore = 0;
            foreach (GameObject g in shadow)
            {
                tempScore += g.GetComponent<Card>().info.strength;
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
    {
        if (!storm)
        {
            StormParticleSystem.GetComponent<ParticleSystem>().Play();
            // set all vantage (non-hero cards to 1)
            foreach (GameObject g in shadow)
            {
                if (!g.GetComponent<Card>().info.isHero)
                {
                    g.GetComponent<Card>().info.AddDeBuff(100);
                    // call update ui score on card
                    g.GetComponent<Card>().UI_Update();
                    g.GetComponent<Card>().DebuffColorEffect();
                }
            }
            storm = true;
            UpdateModifiedUnitScores(3);
        }
    }

    // this is for clear weather card
    public void SetClearWeather()
    {
        FrostParticleSystem.GetComponent<ParticleSystem>().Stop();
        BaneAetheriusParticleSystem.GetComponent<ParticleSystem>().Stop();
        StormParticleSystem.GetComponent<ParticleSystem>().Stop();

        //reset debuffs
        ResetWeatherDebuffs();

        if (frostbite)
            UpdateModifiedUnitScores(1);
        if (baneAetherius)
            UpdateModifiedUnitScores(2);
        if (storm)
            UpdateModifiedUnitScores(3);

        // if any boosters were active, redo all the buffs:
        if (frontlineBoost && frostbite)
        {
            //add buffs again.
            foreach (GameObject g in frontline)
            {
                g.GetComponent<Card>().info.ResetBuffs();

                

                //g.GetComponent<Card>().info.AddBuff(g.GetComponent<Card>().info.strength);
                //g.GetComponent<Card>().BuffColorEffect();
            }
            AddBooster(1, null);
        }
        if (baneAetherius && baneAetherius)
        {
            //add buffs again.
            foreach (GameObject g in vantage)
            {
                g.GetComponent<Card>().info.ResetBuffs();
                //
                //g.GetComponent<Card>().info.AddBuff(g.GetComponent<Card>().info.strength);
                //g.GetComponent<Card>().BuffColorEffect();
            }
            AddBooster(2, null);
        }
        if (shadowBoost && storm)
        {
            //add buffs again.
            foreach (GameObject g in shadow)
            {
                g.GetComponent<Card>().info.ResetBuffs();
                //g.GetComponent<Card>().info.AddBuff(g.GetComponent<Card>().info.strength);
                //g.GetComponent<Card>().BuffColorEffect();
            }
            AddBooster(3, null);
        }


        frostbite = false;
        baneAetherius = false;
        storm = false;

    }

    //this is for end of round reset
    public void ResetWeather()
    {
        //turn off all particle systems:
        FrostParticleSystem.GetComponent<ParticleSystem>().Stop();
        BaneAetheriusParticleSystem.GetComponent<ParticleSystem>().Stop();
        StormParticleSystem.GetComponent<ParticleSystem>().Stop();

        //turn off all booleans
        frostbite = false;
        baneAetherius = false;
        storm = false;
    }


    public void ResetBoosters()
    {
        frontlineBoost = false;
        vantageBoost = false;
        shadowBoost = false;
    }


    // medic and necromancer cards

    public GameObject MedicReDeploy()
    {
        //return a random card (random for now) from discard pile.
        if (discardpile.Count > 0)
        {
            int index = Random.Range(0, discardpile.Count-1);
            int i = 0;
            LinkedListNode<GameObject> temp = discardpile.First;

            Debug.Log("Discard pile size: "+discardpile.Count);
            Debug.Log("Randomly generated index: "+index);

            while (i < index)
            {
                temp = temp.Next;
                i++;
            }
            if (temp != null)
            {
                GameObject redeployRef = temp.Value;
                // remove this from the discard pile
                discardpile.Remove(redeployRef);
                return redeployRef;
            }
            else
                return null;
        }
        else
            return null;

    }

    // necromancer functions
    public GameObject NecromancerReDeploy()
    {
        //return a random card (random for now) from discard pile.
        if (discardpile.Count > 0)
        {
            int index = Random.Range(0, discardpile.Count - 1);
            int i = 0;
            LinkedListNode<GameObject> temp = discardpile.First;
            
            while (i < index)
            {
                temp = temp.Next;
                i++;
            }
            if (temp != null)
            {
                GameObject redeployRef = temp.Value;
                // remove this from the discard pile
                discardpile.Remove(redeployRef);
                return redeployRef;
            }
            else
                return null;
        }
        else
            return null;
    }


    // booster functions:
    public void AddBooster(int i, GameObject boosterCard)
    {
        if (i == 1)        // frontline booster
        {
            if (!frontlineBoost)
            {
                if (boosterCard != null)
                {
                    boosterCard.transform.Translate(new Vector3(0, frontlinePosY, 0));
                    boosterCard.transform.position = new Vector3(-1.5f, boosterCard.transform.position.y, 0);
                }
                foreach (GameObject g in frontline)
                {
                    g.GetComponent<Card>().info.AddBuff(g.GetComponent<Card>().info.strength);
                    if (!frostbite)
                        g.GetComponent<Card>().BuffColorEffect();
                }
                UpdateModifiedUnitScores(1);
                frontlineBoost = true;
            }
            else
            {
                // move so it wont bug the turn
                if (boosterCard != null)
                {
                    boosterCard.transform.Translate(new Vector3(0, frontlinePosY, 0));
                    boosterCard.transform.position = new Vector3(-1.5f, boosterCard.transform.position.y, 0);
                }
            }
        }
        else if (i == 2)  // vantage booster
        {
            if (!vantageBoost)
            {
                if (boosterCard != null)
                {
                    boosterCard.transform.Translate(new Vector3(0, vantagePosY, 0));
                    boosterCard.transform.position = new Vector3(-1.5f, boosterCard.transform.position.y, 0);
                }
                foreach (GameObject g in vantage)
                {
                    g.GetComponent<Card>().info.AddBuff(g.GetComponent<Card>().info.strength);
                    if (!baneAetherius)
                        g.GetComponent<Card>().BuffColorEffect();
                }
                UpdateModifiedUnitScores(2);
                vantageBoost = true;
            }
            else
            {
                // move so it wont bug the turn
                if (boosterCard != null)
                {
                    boosterCard.transform.Translate(new Vector3(0, vantagePosY, 0));
                    boosterCard.transform.position = new Vector3(-1.5f, boosterCard.transform.position.y, 0);
                }
            }

        }
        else if (i == 3)
        {
            if (!shadowBoost)
            {
                if (boosterCard != null)
                {
                    boosterCard.transform.Translate(new Vector3(0, shadowPosY, 0));
                    boosterCard.transform.position = new Vector3(-1.5f, boosterCard.transform.position.y, 0);
                }
                foreach (GameObject g in shadow)
                {
                    g.GetComponent<Card>().info.AddBuff(g.GetComponent<Card>().info.strength);
                    if (!storm)
                        g.GetComponent<Card>().BuffColorEffect();
                }
                UpdateModifiedUnitScores(3);
                vantageBoost = true;
            }
            else
            {
                // move so it wont bug the turn
                if (boosterCard != null)
                {
                    boosterCard.transform.Translate(new Vector3(0, vantagePosY, 0));
                    boosterCard.transform.position = new Vector3(-1.5f, boosterCard.transform.position.y, 0);
                }
            }
        }
        if(boosterCard!=null)
            boosterCard.GetComponent<Card>().SetCardStatus("Deployed");
    }

    // for re-drawing, update sequence

    public void InitSequence(List<int> list)
    {
        drawnSequence = list;
    }

    public int RedrawCard(int maxDeckSize)
    {
        int r;
        while (true)
        {
            r = Random.Range(0,maxDeckSize);
            if (!drawnSequence.Contains(r))
            {
                drawnSequence.Add(r);
                return r;
            }
        }
    }

    // rearrange hand cards function
    // x offset = -2, y take in as parameter
    public void RearrangeHand(Transform HandRef, float yPos)
    {
        float x = -2;
        int count = HandRef.childCount;
        for (int i = 0; i < count; i++)
        {
            HandRef.GetChild(i).transform.position = new Vector3(x,yPos,0);
            x += 0.75f;
        }

    }

    
}
