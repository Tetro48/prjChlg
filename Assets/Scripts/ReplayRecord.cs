using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
    public List<int> frames;
    public int boards;
    public List<NativeList<float2>> movementVector;
    public List<NativeList<bool4x2>> inputs;
    public List<bool[]> switches;
    public static int seed;
    [SerializeField] TextMeshProUGUI textMode;

    public void SaveReplay(string name)
    {
        ReplayScript.SaveReplay(this, name);
    }
    public void LoadReplay(string name)
    {
        ReplayVars data = ReplayScript.LoadReplay(name);

        boards = data.boards;
        frames = new List<int>();
        for (int i = 0; i < boards; i++) frames.Add(0);
        movementVector = data.movementVector;
        inputs = data.inputs;
        switches = data.switches;
        UnityEngine.Random.InitState(data.seed);
        for (int i = 0; i < NetworkBoard.player.Count; i++)
        {
            NetworkBoard.player[i].lineFreezingMechanic = switches[i][0];
            frames[i] = 0;
        }
    }
    public void SwitchMode()
    {
        mode++;
        mode = (ReplayModeType)((int)mode % 3);
        textMode.text = "Replay type: " + (mode == ReplayModeType.write ? "Write" : mode == ReplayModeType.read ? "Read" : "OFF");
        if(mode != ReplayModeType.read)
        {
            for (int i = 0; i < boards; i++)
            {
                inputs[i].RemoveRange(frames[i],inputs[i].Length-frames[i]);
                movementVector[i].RemoveRange(frames[i],movementVector[i].Length-frames[i]);
            }
            Time.timeScale = 1.0f;
        }
    }
    public void Reset(int players = 1)
    {
        boards = players;
        movementVector = new List<NativeList<float2>>();
        inputs = new List<NativeList<bool4x2>>();
        switches = new List<bool[]>();
        seed = new int();
        
    }
    void Awake()
    {
        instance = this;
        Reset();
    }
    void FixedUpdate()
    {
        if (MenuEngine.instance.yourPlayer != null)
        {
            if (mode == ReplayModeType.write && MenuEngine.instance.yourPlayer.AREf > (int)MenuEngine.instance.yourPlayer.ARE -401)
            {
                for (int i = 0; i < NetworkBoard.player.Count; i++)
                {
                    switches[i][0] = NetworkBoard.player[i].lineFreezingMechanic;
                    bool[] localInputs = new bool[7];

                }
            }
            else for (int i = 0; i < boards; i++)
            {
                if(NetworkBoard.player[i].framestepped)frames[i]++;
            }
        }
    }
}
