using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    
    public CardInfo info = new CardInfo();
    public enum State{Hand,Deployed,Dead,Discard,Resurrected,Deck};
    public State cardState = new State();

    //UI References:
    
    public Text unitStrength;

    private void Start()
    {
        cardState = State.Hand;
    }

    private void Update()
    {
        UI_Update();
    }

    private void UI_Update()
    {
        unitStrength.text = "" + info.strength;
    }



    //getters
    public string GetCardStatus()
    {
        return cardState + "";
    }



    //setters
    public void SetCardStatus(string state)
    {
        if (state == "Hand")
            cardState = State.Hand;
        else if (state == "Deployed")
            cardState = State.Deployed;
        else if (state == "Dead")
            cardState = State.Dead;
        else if (state == "Discard")
            cardState = State.Discard;
        else if (state == "Resurrected")
            cardState = State.Resurrected;
        else if (state == "Deck")
            cardState = State.Deck;
        else
            Debug.Log("Invalid state parameter.");

        //Debug.Log("Done changed state to: "+ cardState);
        
    }

}
