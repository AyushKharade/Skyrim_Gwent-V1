using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    
    public CardInfo info = new CardInfo();
    enum State{Alive,Dead,Discarded};

    private void Start()
    {
        State cardState = new State();
        cardState = State.Alive;
        
    }

    private void Update()
    {
        
    }
}
