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
public enum replayModeType {write, read, none}

public class ReplayRecord : MonoBehaviour
{
    public replayModeType mode;
    public int frames;
    public List<float[]> movementVector;
    public List<bool[]> inputs;
    public List<int> bag;
    public bool[] switches;
    [SerializeField] TextMeshProUGUI textMode;

    public void SaveReplay(string name)
    {
        ReplayScript.SaveReplay(this, name);
    }
    public void LoadReplay(string name)
    {
        ReplayVars data = ReplayScript.LoadReplay(name);

        movementVector = data.movementVector;
        inputs = data.inputs;
        switches = data.switches;
        GameEngine.instance.lineFreezingMechanic = switches[0];
        bag = data.bag;
        frames = 0;
    }
    public void SwitchMode()
    {
        mode++;
        mode = (replayModeType)((int)mode%3);
        textMode.text = "Replay type: " + (mode == replayModeType.write ? "Write" : mode == replayModeType.read ? "Read" : "OFF");
        if(mode != replayModeType.read)
        {
            inputs.RemoveRange(frames,inputs.Count-frames);
            movementVector.RemoveRange(frames,movementVector.Count-frames);
            Time.timeScale = 1.0f;
        }
    }
    public void Reset()
    {
        movementVector = new List<float[]>();
        inputs = new List<bool[]>();
        bag = new List<int>();
    }
    void Awake()
    {
        Reset();
    }
    void FixedUpdate()
    {
        if (mode == replayModeType.write && GameEngine.instance.AREf > (int)GameEngine.instance.ARE -401)
        {
            switches[0] = GameEngine.instance.lineFreezingMechanic;
            if(MenuEngine.instance.curBoard != null && GameEngine.instance.framestepped)
            {
                float[] tempmov = new float[2];
                tempmov[0] = GameEngine.instance.movement.x;
                tempmov[1] = GameEngine.instance.movement.y;
                movementVector.Add(tempmov);
                bool[] localinputs = new bool[8];
                localinputs[7] = false;
                
                localinputs[0] = GameEngine.instance.Inputs[0];
                localinputs[1] = GameEngine.instance.Inputs[1];
                localinputs[2] = GameEngine.instance.Inputs[2];
                localinputs[3] = GameEngine.instance.Inputs[3];
                localinputs[4] = GameEngine.instance.Inputs[4];
                localinputs[5] = GameEngine.instance.Inputs[5];
                localinputs[6] = GameEngine.instance.Inputs[6];
                inputs.Add(localinputs);
                bag = PiecesController.instance.bag;
            }
        }
        else if(MenuEngine.instance.curBoard != null && GameEngine.instance.framestepped) 
        {
            frames++;
        }
    }
}
