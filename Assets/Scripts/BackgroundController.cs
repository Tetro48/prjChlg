using UnityEngine;

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

public class BackgroundController : MonoBehaviour
{
    public static BackgroundController bginstance;
    public Sprite[] backgrounds;
    public int backgroundType, nextBackground;
    public bool BGChanging;
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private float finishTime;
    private float transitionTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    private void Start()
    {
        bginstance = this;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        float deltaTime = Time.deltaTime;
        if (BGChanging)
        {
            transitionTime += deltaTime;
            if (transitionTime < finishTime / 2)
            {
                float brightness = 1 - (transitionTime / (finishTime / 2));
                spriteRenderer.color = new Color(brightness, brightness, brightness);
            }
            else
            {
                float brightness = (transitionTime / finishTime * 2) - 1;
                spriteRenderer.color = new Color(brightness, brightness, brightness);
            }
            if (transitionTime >= finishTime)
            {
                transitionTime = 0;
                BGChanging = false;
            }
        }
        if (spriteRenderer.color == new Color(0f, 0f, 0f))
        {
            backgroundType = nextBackground;
        }
        if (spriteRenderer.sprite != backgrounds[backgroundType])
        {
            spriteRenderer.sprite = backgrounds[backgroundType];
        }
    }
    ///<param name="bg">Background sprite change by a integer</param>
    public void TriggerBackgroundChange(int bg)
    {
        if (bg != backgroundType)
        {
            BGChanging = true;
        }

        transitionTime = 0;
        nextBackground = bg;
    }
}
