using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassRound : MonoBehaviour
{


    //button reference
    public int PlayerID;
    Button buttonRef;

    Player playableRef;

    //ui ref
    public GameObject PassedUI;

    void Start()
    {
        playableRef = GameObject.FindGameObjectWithTag("Playable").GetComponent<Player>();

        buttonRef = GetComponent<Button>();
        buttonRef.onClick.AddListener(Pass);

    }

    
    public void Pass()
    {
        Debug.Log("Player "+PlayerID+" Passed.");
        if (PlayerID == 1)
            playableRef.P1Battlefield.GetComponent<Battlefield>().SetPassed();
        else if(PlayerID==2)
            playableRef.P2Battlefield.GetComponent<Battlefield>().SetPassed();

        //disable clicking
        buttonRef.interactable = false;
        PassedUI.SetActive(true);

        playableRef.ChangeTurn();

    }

    public void Reset()
    {
        buttonRef.interactable = true;
        PassedUI.SetActive(false);
    }
}
