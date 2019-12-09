using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardRotator : MonoBehaviour
{
    public GameObject frontRef;
    public GameObject backRef;

    void Start()
    {
        //get references to the children that are front and back, toggle them according to the rotation.
        frontRef=transform.GetChild(0).gameObject.transform.GetChild(1).gameObject;   
        backRef=transform.GetChild(0).gameObject.transform.GetChild(0).gameObject;

        backRef.SetActive(false);

    }

    void Update()
    {
        double angleY = transform.rotation.eulerAngles.y;
        if((angleY>90 && angleY<180) || (angleY>180 && angleY<270 ))
        {
            frontRef.SetActive(false);
            backRef.SetActive(true);
        }
        else
        {
            frontRef.SetActive(true);
            backRef.SetActive(false);
        }
        
    }
}
