using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{


    GameObject raycastTarget;
    bool displayed;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetCameraRaycast();
        //DisplayRaycastTarget();
      
    }


    void GetCameraRaycast()
    {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

        if (hit.collider != null)
        {
            raycastTarget = hit.collider.gameObject;
        }
        else
        {
            raycastTarget = null;
        }
    }


    void DisplayRaycastTarget()
    {
        if (raycastTarget != null)
            Debug.Log("Card: " + raycastTarget.transform.name);
        else
            Debug.Log("No card:");
    }
}
