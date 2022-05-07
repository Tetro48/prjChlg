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
    double UITimePassed;
    double buttonMovementInSeconds;
    [SerializeField] RectTransform[] UIElements, UIPartElements;
    [SerializeField] GameObject[] PlatformIncompatibleUIElements;
    [SerializeField] EventSystem control;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < UIElements.Length; i++)
        {
            UIElements[i].position -= new Vector3(3000,0f,0f);
        }

        if (MenuEngine.instance.platformCompat()) return;
        for (int i = 0; i < PlatformIncompatibleUIElements.Length; i++)
        {
            PlatformIncompatibleUIElements[i].SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        buttonMovementInSeconds = MenuEngine.instance.buttonMovementInSeconds;
    }
    bool CheckUIScroll(bool side, int count, double speed = 1d)
    {
        double _UITimePassed = UITimePassed;
        double buttonTime = buttonMovementInSeconds;
        bool output;
        if(side)
        {
            _UITimePassed += Time.unscaledDeltaTime * speed;
            buttonTime *= System.Math.Ceiling(_UITimePassed / buttonTime);
            if(_UITimePassed >= (double)count * buttonMovementInSeconds) output = false;
            else output = _UITimePassed > buttonTime;
        }
        else
        {
            UITimePassed -= Time.unscaledDeltaTime * speed;
            buttonTime *= System.Math.Floor(UITimePassed / buttonTime);
            if(UITimePassed <= 0d) output = false;
            else output = UITimePassed < buttonTime;
            
        }

        // Debug.Log(_UITimePassed + " / " + buttonTime + ". " + output); // for debug purposes
        return output;
    }
    /// <param name="side"> False -> Left side. True -> Right side. </param>
    public bool MoveCoupleUIElements(bool side, double speed = 1d)
    {
        float time = Time.deltaTime;
        if(CheckUIScroll(side, UIElements.Length, speed))
        {
            MenuEngine.instance.audioSource.PlayOneShot(MenuEngine.instance.clip);
        }

        // Debug.Log(time);
        float reversibleTime = time;
        if(!side) reversibleTime *= -1;
        UITimePassed += reversibleTime * speed;
        float timeToPosX = (float)(UITimePassed / buttonMovementInSeconds) * 300 - 200;
        float timeToPosXPart = (float)(UITimePassed / buttonMovementInSeconds) * 300 + 100;
        if(side)
        {
            for (int i = 0; i < UIElements.Length; i++)
            {
                Vector3 tempPos = UIElements[i].localPosition;
                tempPos.x = Mathf.Clamp(timeToPosX - (300 * i), -200f, 100f);
                // Debug.Log(tempPos.x);
                UIElements[i].localPosition = tempPos;
                if (i >= UIPartElements.Length) continue;
                tempPos.x *= 3.5f;
                UIPartElements[i].localPosition = tempPos;
            }

            if (UITimePassed > UIElements.Length * buttonMovementInSeconds)
            {
                UITimePassed = UIElements.Length * buttonMovementInSeconds;
                foreach (RectTransform rectTransform in UIElements)
                {
                    if (rectTransform.gameObject.activeSelf)
                    {
                        control.SetSelectedGameObject(rectTransform.gameObject);
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
                if (UIPartElements.Length <= i) continue;
                tempPos.x *= 3.5f;
                UIPartElements[i].localPosition = tempPos;
            }

            if (UITimePassed < 0)
            {
                UITimePassed = 0;
                gameObject.SetActive(false);
                return true;
            }
        }
        return false;
    }
}
