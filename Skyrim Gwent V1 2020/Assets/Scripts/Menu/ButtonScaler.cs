using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScaler : MonoBehaviour
{

    public bool underCursor;
    Vector3 originalScale;
    Vector3 cursorScale;

    void Start()
    {
        originalScale = GetComponent<RectTransform>().localScale;
        cursorScale = originalScale * 1.2f;
    }

    void Update()
    {
        if (underCursor)
            ScaleUp();
        else
            ScaleDown();
        underCursor = false;

    }

    void ScaleUp()
    {
        GetComponent<RectTransform>().localScale = cursorScale;
    }

    void ScaleDown()
    {
        GetComponent<RectTransform>().localScale = originalScale;
    }
}
