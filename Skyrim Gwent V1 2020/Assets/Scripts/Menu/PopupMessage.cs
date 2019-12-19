using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupMessage : MonoBehaviour
{

    float timer; // self destroy
    float expireTimer = 2.5f;
    float alphaFactor=191; // transperancy
    public Text textObj;
    Image image;

    
    void Start()
    {
        image= transform.GetChild(0).gameObject.GetComponent<Image>();
        textObj = transform.GetChild(1).gameObject.GetComponent<Text>();

        //set image alpha to 75% then increase (191)
        image.color = new Vector4(255,255,255,0);
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > expireTimer)
        {
            Destroy(transform.parent.gameObject);
        }

        if (image.color.a != 1)
        {
            alphaFactor+=Time.deltaTime;
            image.color = new Vector4(255, 255, 255, alphaFactor);
        }
    }

    public void SetMessage(string message)
    {
        textObj.text="pop messgae";
    }

    public void SetExpireTimer(float newTimer)
    {
        expireTimer = newTimer; // if not set, it will be defaut 2.5 seconds
    }
}
