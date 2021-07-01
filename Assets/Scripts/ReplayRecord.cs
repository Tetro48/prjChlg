using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ReplayRecord : MonoBehaviour
{
    // read if true, write if false
    public bool mode;
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
        mode = !mode;
        textMode.text = "Replay mode: " + (mode ? "ON" : "OFF");
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
        if (!mode)
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