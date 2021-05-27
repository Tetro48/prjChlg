﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
    Project Challenger, an challenging Tetris game.
    Copyright (C) 2021  Aymir

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
    
    SpriteRenderer spriteRenderer;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        bginstance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (BGChanging)
        {
            if (backgroundType != nextBackground)
            {
                this.spriteRenderer.color -= new Color(1f/30f,1f/30f,1f/30f, 0.0f);
            }
            else
            {
                this.spriteRenderer.color += new Color(1/30f,1/30f,1/30f, 0.0f);
            }
            if(this.spriteRenderer.color == new Color(1f, 1f, 1f, 1f)) BGChanging = false;
        }
        if (this.spriteRenderer.color == new Color(0f,0f,0f))
        {
            backgroundType = nextBackground;
        }
        if(this.spriteRenderer.sprite != backgrounds[backgroundType]) this.spriteRenderer.sprite = backgrounds[backgroundType];
    }
    ///<param name="bg">Background sprite change by a integer</param>
    public void TriggerBackgroundChange(int bg)
    {
        BGChanging = true;
        nextBackground = bg;
    }
}
