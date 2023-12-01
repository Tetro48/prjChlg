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
    private int buttonsVisible = 0;

    // Start is called before the first frame update
    private void Start()
    {
        for (int i = 0; i < UIElements.Length; i++)
        {
            UIElements[i].position -= new Vector3(3000f, 0f, 0f);
        }

        for (int i = 0; i < UIPartElements.Length; i++)
        {
            if (UIPartElements[i] != null)
            {
                UIPartElements[i].position -= new Vector3(3000f, 0f, 0f);
            }
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
        double _UITimePassed = UITimePassed % buttonMovementInSeconds;
        double buttonTime = buttonMovementInSeconds;
        bool output = false;
        if (side)
        {
            _UITimePassed += Time.unscaledDeltaTime * speed;
            if (_UITimePassed > buttonTime && buttonsVisible + 1 < count)
            {
                buttonsVisible++;
                output = true;
            }
            if (UITimePassed == 0d)
            {
                output = true;
            }
        }
        else
        {
            _UITimePassed -= Time.unscaledDeltaTime * speed;
            if (_UITimePassed < 0 && buttonsVisible > 0)
            {
                buttonsVisible--;
                output = true;
            }
            if (UITimePassed == buttonTime * count)
            {
                output = true;
            }
        }

        return output;
    }
    /// <param name="side"> False -> Left side. True -> Right side. </param>
    public bool MoveCoupleUIElements(bool side, double speed = 1d)
    {
        if (UITimePassed == double.NaN)
        {
            UITimePassed = 0;
        }
        float time = Time.deltaTime;
        if (time == double.NaN)
        {
            time = 0;
        }
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
                if (i >= UIPartElements.Length || UIPartElements[i] == null)
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
                if (UIPartElements.Length <= i || UIPartElements[i] == null)
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
