using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
    Project Challenger, an challenging Tetris game.
    Copyright (C) 2021, Aymir

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
public enum ReplayModeType { write, read, none }

public class ReplayRecord : MonoBehaviour
{
    public static ReplayRecord instance;
    public ReplayModeType mode;
    public int frames;
    public int boards;
    public List<List<float[]>> movementVector;
    public List<List<bool[]>> inputs;
    public List<bool[]> switches;
    public List<List<int>> bag;
    [SerializeField] TextMeshProUGUI textMode;

    public void SaveReplay(string name)
    {
        ReplayScript.SaveReplay(this, name);
    }
    public void LoadReplay(string name)
    {
        ReplayVars data = ReplayScript.LoadReplay(name);

        boards = data.boards;
        movementVector = data.movementVector;
        inputs = data.inputs;
        switches = data.switches;
        for (int i = 0; i < MenuEngine.players.Count; i++)
        {
            MenuEngine.playersComponent[i].lineFreezingMechanic = switches[i][0];
        }
        bag = data.bag;
        frames = 0;
    }
    public void SwitchMode()
    {
        mode++;
        mode = (ReplayModeType)((int)mode % 3);
        textMode.text = "Replay type: " + (mode == ReplayModeType.write ? "Write" : mode == ReplayModeType.read ? "Read" : "OFF");
        if(mode != ReplayModeType.read)
        {
            inputs.RemoveRange(frames,inputs.Count-frames);
            movementVector.RemoveRange(frames,movementVector.Count-frames);
            Time.timeScale = 1.0f;
        }
    }
    public void Reset()
    {
        boards = new int();
        movementVector = new List<List<float[]>>();
        inputs = new List<List<bool[]>>();
        bag = new List<List<int>>();
        switches = new List<bool[]>();
    }
    void Awake()
    {
        instance = this;
        Reset();
    }
    void FixedUpdate()
    {
        if (mode == ReplayModeType.write && GameEngine.instance.AREf > (int)GameEngine.instance.ARE -401)
        {
            for (int i = 0; i < MenuEngine.players.Count; i++)
            {
                switches[i][0] = MenuEngine.playersComponent[i].lineFreezingMechanic;
                bool[] localInputs = new bool[7];

            }
        }
        else if(MenuEngine.instance.curBoard != null && GameEngine.instance.framestepped) 
        {
            frames++;
        }
    }
}
