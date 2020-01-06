using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Redraw2 : MonoBehaviour
{
    public int turn = 1;
    int redrawsAllowed = 2;

    public Transform p1Ref;
    public Transform p2Ref;


    GameStarter gameinfo;
    GameObject raycastTarget;

    public List<int> P1Draw;
    public List<int> P2Draw;

    bool ReadyToPlay;
    bool p1Ready;
    bool p2Ready;

    //UI
    public Button StartButton;
    public Button P1ReadyButton;
    public Button P2ReadyButton;

    public Text RedrawRemainingP1;
    public Text RedrawRemainingP2;

    // Start is called before the first frame update
    void Start()
    {
        gameinfo = GameObject.FindGameObjectWithTag("GameInfo").GetComponent<GameStarter>();
        GenerateHand(1);
        GenerateHand(2);

        // flip p2 first
        FlipCards(p2Ref);


        //UI
        StartButton.interactable = false;
        P2ReadyButton.interactable = false;
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
            if (turn == 1 && hit.collider.transform.parent.name == "P1Hand"
                ||
                turn == 2 && hit.collider.transform.parent.name == "P2Hand")
                Redraw(hit.collider.gameObject);

        }
    }


    void CardScaling()              // scales card on which you hover mouse to make it clear what you are selecting
    {
        if (raycastTarget != null)
            raycastTarget.GetComponent<CardScaler>().underCursor = true;
    }



    void Redraw(GameObject Target)
    {
        Debug.Log("Calling Redraw");
        int drawIndex;
        if (turn == 1)
        {
            drawIndex = Target.transform.GetSiblingIndex();
            while (true)
            {
                int r = Random.Range(0, gameinfo.P1Deck.GetComponent<Deck>().totalCards);
                if (!P1Draw.Contains(r))
                {
                    GameObject newCard = gameinfo.P1Deck.GetComponent<Deck>().CardsDeck[r];
                    GameObject newCardRef = Instantiate(newCard, p1Ref);

                    //save old card position.
                    Vector3 pos = Target.transform.position;
                    Destroy(Target.gameObject);
                    newCardRef.transform.position = pos;
                    float scaleFactor = newCardRef.transform.localScale.x;
                    newCardRef.transform.localScale = new Vector3(1.5f * scaleFactor, 1.5f * scaleFactor, 1.5f * scaleFactor);
                    newCardRef.transform.SetSiblingIndex(drawIndex);

                    //replace old index
                    P1Draw[drawIndex] = r;
                    redrawsAllowed--;
                    RedrawRemainingP1.text = "Player 1 redraw cards (" + (2 - redrawsAllowed) + "/2)";
                    if (redrawsAllowed == 0)
                    {
                        if (turn == 1)
                        {
                            RedrawRemainingP1.text = "Press Ready.";
                        }
                        Debug.Log("Player 2's Turn");
                    }
                    break;
                }

            }
        }
        else if (turn == 2)
        {
            drawIndex = Target.transform.GetSiblingIndex();
            while (true)
            {
                int r = Random.Range(0, gameinfo.P2Deck.GetComponent<Deck>().totalCards);
                if (!P2Draw.Contains(r))
                {
                    GameObject newCard = gameinfo.P2Deck.GetComponent<Deck>().CardsDeck[r];
                    GameObject newCardRef = Instantiate(newCard, p2Ref);

                    //save old card position.
                    Vector3 pos = Target.transform.position;
                    Destroy(Target.gameObject);
                    newCardRef.transform.position = pos;
                    float scaleFactor = newCardRef.transform.localScale.x;
                    newCardRef.transform.localScale = new Vector3(1.5f * scaleFactor, 1.5f * scaleFactor, 1.5f * scaleFactor);
                    newCardRef.transform.SetSiblingIndex(drawIndex);

                    //replace old index
                    P2Draw[drawIndex] = r;
                    redrawsAllowed--;
                    RedrawRemainingP2.text = "Player 1 redraw cards (" + (2 - redrawsAllowed) + "/2)";
                    if (redrawsAllowed == 0)
                    {
                        RedrawRemainingP2.text = "Press Ready.";
                        Debug.Log("Ready To Play!");
                    }
                    break;
                }

            }
        }
        

    }


    // flip cards
    void FlipCards(Transform HandRef)
    {
        for (int i = 0; i < HandRef.childCount;i++)
        {
            HandRef.GetChild(i).transform.Rotate(new Vector3(0, 180, 0));
        }
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



    // button functions

    public void P1Ready()
    {
        //assuming it was turn 1, otherwise this button wont be available.
        turn = 2;
        redrawsAllowed = 2;
        P1ReadyButton.interactable = false;
        P2ReadyButton.interactable = true;
        p1Ready = true;
        FlipCards(p1Ref);
        FlipCards(p2Ref);
    }

    public void P2Ready()
    {
        //assuming it was turn 1, otherwise this button wont be available.
        P2ReadyButton.interactable = false;
        p2Ready = true;
        FlipCards(p2Ref);
        if (p1Ready && p2Ready)
        {
            ReadyToPlay = true;
            StartButton.interactable = true;
        }
    }

    public void StartGame()
    {
        //send draw sequences to GameInfo, load gwent board.
        gameinfo.SetDrawSequence(1, P1Draw);
        gameinfo.SetDrawSequence(2, P2Draw);
        Debug.Log("And the Game shall Begin.");

        //load board scene
        SceneManager.LoadScene("GwentBoard");
    }


}
