using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PassRound : MonoBehaviour
{


    //button reference
    public int PlayerID;
    Button buttonRef;

    void Start()
    {
        buttonRef = GetComponent<Button>();
        buttonRef.onClick.AddListener(Pass);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Pass()
    {
        Debug.Log("Passed.");
    }
}
