using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
    Project Challenger, an challenging Tetris game.
    Copyright (C) 2022, Aymir

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
public class MenuSegment : MonoBehaviour
{
    double UITimeDelta;
    double buttonMovementInSeconds;
    [SerializeField] RectTransform[] UIElements, UIPartElements;
    [SerializeField] GameObject[] DisableUIElements;
    [SerializeField] EventSystem control;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < UIElements.Length; i++)
        {
            UIElements[i].position -= new Vector3(3000,0f,0f);
        }
        for (int i = 0; i < DisableUIElements.Length; i++)
        {
            DisableUIElements[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
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
            if(uitimedelta >= (double)count * buttonMovementInSeconds) output = false;
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
    public bool MoveCoupleUIElements(bool side, double speed = 1d)
    {
        float time = Time.deltaTime;
        if(CheckUIScroll(side, UIElements.Length, speed))
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
                tempPos.x = Mathf.Clamp(timeToPosX - (300 * i), -200f, 100f);
                // Debug.Log(tempPos.x);
                UIElements[i].localPosition = tempPos;
                if(i < UIPartElements.Length)
                {
                    tempPos.x *= 3.5f;
                    UIPartElements[i].localPosition = tempPos;
                }
            }
            if(UITimeDelta > UIElements.Length * buttonMovementInSeconds)
            {
                UITimeDelta = UIElements.Length * buttonMovementInSeconds;
                for (int i = 0; i < UIElements.Length; i++)
                {
                    if (UIElements[i].gameObject.activeSelf)
                    {
                        control.SetSelectedGameObject(UIElements[i].gameObject);
                    }
                }
                return true;
            }
        }
        else
        {
            for (int i = UIElements.Length - 1; i >= 0; i--)
            {
                Vector3 tempPos = UIElements[i].localPosition;
                tempPos.x = Mathf.Clamp(timeToPosX - (300 * (UIElements.Length - i - 1)), -200f, 100f);
                // Debug.Log(tempPos.x);
                UIElements[i].localPosition = tempPos;
                if(UIPartElements.Length > i)
                {
                    tempPos.x *= 3.5f;
                    UIPartElements[i].localPosition = tempPos;
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
