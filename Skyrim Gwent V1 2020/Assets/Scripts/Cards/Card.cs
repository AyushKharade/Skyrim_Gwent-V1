using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    
    public CardInfo info = new CardInfo();
    public enum State{Alive,Dead,Discarded};
    public State cardState = new State();

    //UI References:
    
    public Text unitStrength;

    private void Start()
    {
        cardState = State.Alive;
    }

    private void Update()
    {
        UI_Update();
    }

    private void UI_Update()
    {
        unitStrength.text = "" + info.strength;
    }
}
