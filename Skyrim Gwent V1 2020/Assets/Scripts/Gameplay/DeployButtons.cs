﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeployButtons : MonoBehaviour
{
    Player PlayableRef;

    private void Start()
    {
        PlayableRef = GameObject.FindGameObjectWithTag("Playable").GetComponent<Player>();
    }

    public void Close()
    {
        PlayableRef.CloseDetailsMenu();
    }

    /*
     * How revamped Deployment works:
     * DisplayDetails function in player takes care that player can only choose a card from the hand whose turn it is now.
     * 
     * Only appropriate buttons according to unit type are visible, so no need to worry about processing improper inputs, because
     * only proper inputs are visible to the player.
     * The card selected  while displaying is currently saved in player.
     * These functions do the necessary information retrieval such as if healer, if necromancer, and sends this info back to player
     * The original 480~500 line deploy function is now split into smaller functions, original function wil be probably kept as commented out
     * code of .txt file
     *
     * 
     */


    public void DeployFrontLine()
    {
        // fetch data, if spy
        if (PlayableRef != null)
        {
            if (PlayableRef.cardDeploying.GetComponent<Card>().info.GetSubUnitType() == "Spy")
            { }
            else
            {
                // regular function call.
                PlayableRef.DeployToFrontline();
            }
        }
    }

    public void DeployVantage()
    {
        Debug.Log("Deploying Card to Vantage");
        if (PlayableRef.cardDeploying.GetComponent<Card>().info.GetSubUnitType() == "Spy")
        { }
        else
        {
            // regular function call.
            PlayableRef.DeployToVantage("Regular");
        }
    }

    public void DeployShadow()
    {
        Debug.Log("Deploying Card to Shadow");
        if (PlayableRef.cardDeploying.GetComponent<Card>().info.GetSubUnitType() == "Spy")
        { }
        else
        {
            // regular function call.
            PlayableRef.DeployToShadow();
        }
    }

    public void DeploySpecial()
    {
        Debug.Log("Deploying Card to Special");
        string str = PlayableRef.cardDeploying.GetComponent<Card>().info.GetSubUnitType();
        if (str == "FrostWeather" || str == "BaneAetheriusWeather" || str == "StormWeather" || str == "ClearWeather")
        {
            //Debug.Log("Deploying weather: "+str);
            PlayableRef.DeploySpecialWeather(str);
        }
        else
        {
            //Debug.Log("Booster card selected.");
            PlayableRef.DeploySpecialBooster(str);
        }
        //else its a booster 
    }
}
