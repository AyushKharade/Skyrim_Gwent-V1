using System.Collections;
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

    public void DeployFrontLine()
    {
        Debug.Log("Deploying Card to frontline");
    }

    public void DeployVantage()
    {
        Debug.Log("Deploying Card to Vantage");
    }

    public void DeployShadow()
    {
        Debug.Log("Deploying Card to Shadow");
    }

    public void DeploySpecial()
    {
        Debug.Log("Deploying Card to Special");
    }
}
