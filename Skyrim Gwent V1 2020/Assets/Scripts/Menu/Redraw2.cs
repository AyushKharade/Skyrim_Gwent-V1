using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Redraw2 : MonoBehaviour
{
    public int turn = 1;

    public Transform p1Ref;
    public Transform p2Ref;


    GameStarter gameinfo;
    GameObject raycastTarget;

    List<int> P1Draw;
    List<int> P2Draw;

    bool ReadyToPlay;

    // Start is called before the first frame update
    void Start()
    {
        gameinfo = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameStarter>();
        GenerateHand(1);
        GenerateHand(2);
    }

    // Update is called once per frame
    void Update()
    {
        if (!ReadyToPlay)
        {
            GetCameraRaycast();
            CardScaling();
        }
    }

    void GetCameraRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
            raycastTarget = hit.collider.gameObject;
        else
            raycastTarget = null;

        // input
        if (Input.GetMouseButtonDown(0) && hit.collider != null)
        {
            Debug.Log("Called redraw on "+raycastTarget.GetComponent<Card>().info.name+" for player turn "+turn);
        }
    }


    void CardScaling()              // scales card on which you hover mouse to make it clear what you are selecting
    {
        if (raycastTarget != null)
            raycastTarget.GetComponent<CardScaler>().underCursor = true;
    }


    // copied function for drawing from player
    private void GenerateHand(int PlayerID)     //generate hand function
    {
        int count = 10;
        float yOffset;
        float xOffset = -7.2f;
        GameObject deck;

        if (PlayerID == 1)
        {
            deck = gameinfo.P1Deck;
            yOffset = -3f;
        }
        else
        {
            deck = gameinfo.P2Deck;
            yOffset = 3f;
        }

        int maxCards = deck.GetComponent<Deck>().totalCards;
        List<int> drawSequence = GenerateRandomIndices(maxCards);       // for spy cards, save this sequence

        for (int i = 0; i < count; i++)
        {
            // fetch signature:
            GameObject card;
            card = deck.GetComponent<Deck>().CardsDeck[drawSequence[i]];

            // instantiate
            if (PlayerID == 1)
            {
                GameObject temp = Instantiate(card, p1Ref);
                temp.GetComponent<Card>().SetCardStatus("Hand");
                temp.transform.position = new Vector3(xOffset, yOffset, 0);
                float scaleFactor = temp.transform.localScale.x;
                temp.transform.localScale = new Vector3(1.5f * scaleFactor, 1.5f * scaleFactor, 1.5f * scaleFactor);
                xOffset += 1.25f;
            }
            else
            {
                GameObject temp = Instantiate(card, p2Ref);
                temp.GetComponent<Card>().SetCardStatus("Hand");
                temp.transform.position = new Vector3(xOffset, yOffset, 0);
                float scaleFactor = temp.transform.localScale.x;
                temp.transform.localScale = new Vector3(1.5f * scaleFactor, 1.5f * scaleFactor, 1.5f * scaleFactor);
                xOffset += 1.25f;
            }
        }

        // save sequence to battlefield so further drawing is possible

        if (PlayerID == 1)
        {
            P1Draw = drawSequence;
           // P1BFRef.InitSequence(drawSequence);
        }
        else if (PlayerID == 2)
        {
            P2Draw = drawSequence;                     // rather save it in game info
            //   P2BFRef.InitSequence(drawSequence);
        }
    }



    List<int> GenerateRandomIndices(int deckSize)               // used by GenerateHand() to use a unique sequence of indices which is used to draw cards
    {
        List<int> sequence = new List<int>();
        while (sequence.Count < 10)
        {
            int temp = Random.Range(0, deckSize);
            if (!sequence.Contains(temp))
                sequence.Add(temp);
        }
        return sequence;

    }

}
