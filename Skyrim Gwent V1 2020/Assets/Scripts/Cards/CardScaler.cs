using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardScaler : MonoBehaviour
{
    // to give the scaling effect when hovering over cards.


    [HideInInspector]public bool underCursor;
    [HideInInspector]public bool deployed;
    [HideInInspector]public bool displayCard;
    Vector3 originalScale;
    Vector3 cursorScale;

    void Start()
    {
        originalScale = GetComponent<RectTransform>().localScale;
        cursorScale = originalScale * 1.2f;
        deployed = false;
    }

    void Update()
    {
        if (underCursor &&!deployed && !displayCard)
            ScaleUp();
        else
            ScaleDown();
        underCursor = false;

    }

    void ScaleUp()
    {
        GetComponent<RectTransform>().localScale=cursorScale;
    }

    void ScaleDown()
    {
        GetComponent<RectTransform>().localScale=originalScale;
    }
}
