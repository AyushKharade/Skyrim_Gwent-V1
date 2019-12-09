using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{


    GameObject raycastTarget;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetCameraRaycast();  
    }


    void GetCameraRaycast()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, 100.0f))
        {
            Debug.Log("Hovering on: " + hit.transform.name);
        }
    }
}
