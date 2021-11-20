using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSegment : MonoBehaviour
{
    double UITimeDelta, buttonMovementInSeconds;
    float reswidth;
    [SerializeField] RectTransform[] UIElements, UIPartElements;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < UIElements.Length; i++)
        {
            UIElements[i].position -= new Vector3(3000,0f,0f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        reswidth = MenuEngine.instance.reswidth;
        buttonMovementInSeconds = MenuEngine.instance.buttonMovementInSeconds;
    }
    bool CheckUIScroll(bool side, int count, double speed = 1d)
    {
        double uitimedelta = UITimeDelta;
        double buttonTime = buttonMovementInSeconds;
        bool output;
        if(side)
        {
            uitimedelta += Time.unscaledDeltaTime * speed;
            buttonTime *= System.Math.Ceiling(UITimeDelta / buttonTime);
            if(uitimedelta >= (double)count) output = false;
            else output = uitimedelta > buttonTime;
        }
        else
        {
            uitimedelta -= Time.unscaledDeltaTime * speed;
            buttonTime *= System.Math.Floor(UITimeDelta / buttonTime);
            if(uitimedelta <= 0d) output = false;
            else output = uitimedelta < buttonTime;
            
        }

        // Debug.Log(uitimedelta + " / " + buttonTime + ". " + output); // for debug purposes
        return output;
    }
    /// <param name="side"> False -> Left side. True -> Right side. </param>
    public bool MoveCoupleUIElements(bool side, float multiplication = 1f, double speed = 1d)
    {
        float time = Time.deltaTime;
        if(multiplication == 1f) if(CheckUIScroll(side, UIElements.Length, speed))
        MenuEngine.instance.audioSource.PlayOneShot(MenuEngine.instance.clip);
        // Debug.Log(time);
        float reversibleTime = time;
        if(!side) reversibleTime *= -1;
        UITimeDelta += reversibleTime * speed;
        float timeToPosX = (float)(UITimeDelta / buttonMovementInSeconds) * 300 - 200;
        float timeToPosXPart = (float)(UITimeDelta / buttonMovementInSeconds) * 300 + 100;
        if(side)
        {
            for (int i = 0; i < UIElements.Length; i++)
            {
                Vector3 tempPos = UIElements[i].localPosition;
                tempPos.x = Mathf.Clamp((timeToPosX - 300 * i) * reswidth * multiplication, (-150f * reswidth * multiplication) - 50f, (150f * reswidth * multiplication) - 50f);
                // Debug.Log(tempPos.x);
                UIElements[i].localPosition = tempPos;
                if(i < UIPartElements.Length)
                {
                    UIPartElements[i].localPosition = new Vector3(tempPos.x * 3.5f, tempPos.y, tempPos.z);
                }
            }
            if(UITimeDelta > UIElements.Length * buttonMovementInSeconds)
            {
                UITimeDelta = UIElements.Length * buttonMovementInSeconds;
                return true;
            }
        }
        else
        {
            for (int i = UIElements.Length - 1; i >= 0 ; i--)
            {
                Vector3 tempPos = UIElements[i].localPosition;
                tempPos.x = Mathf.Clamp((timeToPosX - 300 * (UIElements.Length-i-1)) * reswidth * multiplication, (-150f * reswidth * multiplication) - 50f, (150f * reswidth * multiplication) - 50f);
                // Debug.Log(tempPos.x);
                UIElements[i].localPosition = tempPos;
                if(UIPartElements.Length > i)
                {
                    UIPartElements[i].localPosition = new Vector3(tempPos.x * 3.5f, tempPos.y, tempPos.z);
                }
            }
            if(UITimeDelta < 0) 
            {
                UITimeDelta = 0;
                gameObject.SetActive(false);
                return true;
            }
        }
        return false;
    }
}
