using UnityEngine;
using UnityEngine.EventSystems;

/*
    Project Challenger, a challenging block stacking game.
    Copyright (C) 2022-2023, Aymir

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
    private double UITimePassed;
    private double buttonMovementInSeconds;
    [SerializeField] private RectTransform[] UIElements, UIPartElements;
    [SerializeField] private GameObject[] PlatformIncompatibleUIElements;
    [SerializeField] private EventSystem control;

    // Start is called before the first frame update
    private void Start()
    {
        for (int i = 0; i < UIElements.Length; i++)
        {
            UIElements[i].position -= new Vector3(3000, 0f, 0f);
        }

        if (MenuEngine.instance.platformCompat())
        {
            return;
        }

        for (int i = 0; i < PlatformIncompatibleUIElements.Length; i++)
        {
            PlatformIncompatibleUIElements[i].SetActive(false);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        buttonMovementInSeconds = MenuEngine.instance.buttonMovementInSeconds;
    }

    private bool CheckUIScroll(bool side, int count, double speed = 1d)
    {
        double _UITimePassed = UITimePassed;
        double buttonTime = buttonMovementInSeconds;
        bool output;
        if (side)
        {
            _UITimePassed += Time.unscaledDeltaTime * speed;
            buttonTime *= System.Math.Ceiling(_UITimePassed / buttonTime);
            if (_UITimePassed >= count * buttonTime)
            {
                output = false;
            }
            else
            {
                output = _UITimePassed > buttonTime;
            }
        }
        else
        {
            _UITimePassed -= Time.unscaledDeltaTime * speed;
            buttonTime *= System.Math.Floor(_UITimePassed / buttonTime);
            if (_UITimePassed <= 0d)
            {
                output = false;
            }
            else
            {
                output = _UITimePassed < buttonTime;
            }
        }

        // Debug.Log(_UITimePassed + " / " + buttonTime + ". " + output); // for debug purposes
        return output;
    }
    /// <param name="side"> False -> Left side. True -> Right side. </param>
    public bool MoveCoupleUIElements(bool side, double speed = 1d)
    {
        float time = Time.deltaTime;
        if (CheckUIScroll(side, UIElements.Length, speed))
        {
            AudioManager.PlayClip("scroll");
        }

        // Debug.Log(time);
        float reversibleTime = time;
        if (!side)
        {
            reversibleTime *= -1;
        }

        UITimePassed += reversibleTime * speed;
        float timeToPosX = (float)(UITimePassed / buttonMovementInSeconds) * 300 - 200;
        if (side)
        {
            for (int i = 0; i < UIElements.Length; i++)
            {
                Vector3 tempPos = UIElements[i].localPosition;
                tempPos.x = Mathf.Clamp(timeToPosX - (300 * i), -200f, 100f);
                // Debug.Log(tempPos.x);
                UIElements[i].localPosition = tempPos;
                if (i >= UIPartElements.Length)
                {
                    continue;
                }

                tempPos.x *= 3.5f;
                UIPartElements[i].localPosition = tempPos;
            }

            if (UITimePassed > UIElements.Length * buttonMovementInSeconds)
            {
                UITimePassed = UIElements.Length * buttonMovementInSeconds;
                for (int i = 0; i < UIElements.Length; i++)
                {
                    if (UIElements[i].gameObject.activeSelf)
                    {
                        control.SetSelectedGameObject(UIElements[i].gameObject);
                        break;
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
                if (UIPartElements.Length <= i)
                {
                    continue;
                }

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
