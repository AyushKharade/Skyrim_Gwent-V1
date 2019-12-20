using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Endgame : MonoBehaviour
{
    //scores:
    int S_P1R1, S_P1R2, S_P1R3;
    int S_P2R1, S_P2R2, S_P2R3;
    bool scoresSetP1;
    bool scoresSetP2;

    void Start()
    {
        
    }

    void Update()
    {
        if (scoresSetP1 && scoresSetP2)
        {
            UpdateUI();
        }
    }

    public void SetP1Scores(int P1R1, int P1R2, int P1R3)
    {
        S_P1R1 = P1R1;
        S_P1R2 = P1R2;
        S_P1R3 = P1R3;
        scoresSetP1 = true;
    }


    public void SetP2Scores(int P2R1, int P2R2, int P2R3)
    {
        S_P2R1 = P2R1;
        S_P2R2 = P2R2;
        S_P2R3 = P2R3;
        scoresSetP2 = true;
    }

    // update UI
    void UpdateUI()
    {
        GameObject temp=transform.GetChild(3).gameObject;
        // update scores
        temp.transform.GetChild(0).GetComponent<Text>().text = S_P1R1+"";
        temp.transform.GetChild(1).GetComponent<Text>().text = S_P1R2+"";
        temp.transform.GetChild(2).GetComponent<Text>().text = S_P1R3+"";
        temp.transform.GetChild(3).GetComponent<Text>().text = S_P2R1+"";
        temp.transform.GetChild(4).GetComponent<Text>().text = S_P2R2+"";
        temp.transform.GetChild(5).GetComponent<Text>().text = S_P2R3+"";

        int totalP1 = S_P1R1 + S_P1R2 + S_P1R3;
        int totalP2 = S_P2R1 + S_P2R2 + S_P2R3;

        temp = transform.GetChild(4).gameObject;
        if (totalP1 > totalP2)
        {
            temp.transform.GetChild(1).gameObject.SetActive(false);
        }
        else if (totalP1 == totalP2)
        {
            temp.transform.GetChild(0).gameObject.SetActive(false);
            temp.transform.GetChild(1).gameObject.SetActive(false);
        }
        else
        {
            temp.transform.GetChild(0).gameObject.SetActive(false);
        }
    }


}
