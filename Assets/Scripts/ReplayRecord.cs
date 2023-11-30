using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
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
public enum ReplayModeType { write, read, none }

public class ReplayRecord : MonoBehaviour
{
    public static ReplayRecord instance;
    public ReplayModeType mode;
    public List<int> frames;
    public int boards;
    public List<float2>[] movementVector;
    public List<bool4x2>[] inputs;
    public float2[][] replayMovementVector;
    public bool4x2[][] replayInputs;
    public bool[][] switches;
    public static int seed;
    [SerializeField] private TextMeshProUGUI textMode;

    public void SaveReplay(string name)
    {
        ReplayVars vars = new ReplayVars(this);
        ReplayScript.SaveData(vars, name);
        vars.Dispose();
    }
    public void LoadReplay(string name)
    {
        ReplayVars data = ReplayScript.LoadReplay(name);

        boards = data.boards;
        frames = new List<int>();
        for (int i = 0; i < boards; i++)
        {
            frames.Add(0);
        }

        replayMovementVector = data.movementVector;
        replayInputs = data.inputs;
        switches = data.switches;
        UnityEngine.Random.InitState(data.seed);
        for (int i = 0; i < NetworkBoard.player.Count; i++)
        {
            NetworkBoard.player[i].lineFreezingMechanic = switches[i][0];
            NetworkBoard.player[i].bigMode = switches[i][1];
            frames[i] = 0;
        }
    }
    public void SwitchMode()
    {
        mode++;
        mode = (ReplayModeType)((int)mode % 3);
        textMode.text = "Replay type: " + (mode == ReplayModeType.write ? "Write" : mode == ReplayModeType.read ? "Read" : "OFF");
        if (mode != ReplayModeType.read)
        {
            for (int i = 0; i < boards; i++)
            {
                inputs[i].RemoveRange(frames[i], inputs[i].Count - frames[i]);
                movementVector[i].RemoveRange(frames[i], movementVector[i].Count - frames[i]);
            }
            Time.timeScale = 1.0f;
        }
    }
    public void Reset(int players = 1)
    {
        boards = players;
        movementVector = new List<float2>[players];
        inputs = new List<bool4x2>[players];
        switches = new bool[players][];
        seed = new int();
        for (int i = 0; i < players; i++)
        {
            movementVector[i] = new List<float2>();
            inputs[i] = new List<bool4x2>();
            switches[i] = new bool[2];
        }

    }

    private void Awake()
    {
        instance = this;
        Reset();
    }

    private void FixedUpdate()
    {
        if (MenuEngine.instance.mainPlayer != null)
        {
            if (mode == ReplayModeType.write && MenuEngine.instance.mainPlayer.spawnTicks > (int)MenuEngine.instance.mainPlayer.spawnDelay - 401)
            {
                for (int i = 0; i < NetworkBoard.player.Count; i++)
                {
                    switches[i][0] = NetworkBoard.player[i].lineFreezingMechanic;
                    switches[i][1] = NetworkBoard.player[i].bigMode;
                    inputs[i].Add(NetworkBoard.player[i].Inputs);
                }
            }
            else
            {
                for (int i = 0; i < NetworkBoard.player.Count; i++)
                {
                    if (NetworkBoard.player[i].framestepped)
                    {
                        frames[i]++;
                    }
                }
            }
        }
    }
}
