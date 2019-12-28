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
    Vector4 originalColor;

    // For moving cards when deployed
    Vector3 originalPosition;
    Vector3 newPosition;

    public float moveSpeed;
    bool enableMovement;

    public bool onDisplay;

    private void Start()
    {
        cardState = State.Hand;
        originalColor = unitStrength.color;
        originalPosition = transform.position;
    }

    private void Update()
    {
        UI_Update();
        if (originalPosition != newPosition && enableMovement)
            CardMovement();
    }

    public void UI_Update()
    {
        unitStrength.text = "" + info.strength;
    }

    void CardMovement()
    {
        // move towards new position
        //get direction
        Vector3 dir = newPosition - originalPosition;
        transform.Translate(dir*moveSpeed*Time.deltaTime);
    }







    // strength text color
    public void DebuffColorEffect()
    {
        unitStrength.color = Color.red;
    }

    public void BuffColorEffect()
    {
        unitStrength.color = Color.green;
    }

    public void ResetBuffColorEffect()
    {
        unitStrength.color = originalColor;
    }


    //----------------------------------------------------------------
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
    }

    public void SetDestination(Vector3 dest)
    {
        newPosition = dest;
    }
}
