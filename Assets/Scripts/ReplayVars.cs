using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ReplayVars
{
    public List<float[]> movementVector;
    public List<bool[]> inputs;
    public bool[] switches = new bool[1];
    public List<int> bag;
    
    public ReplayVars (ReplayRecord replay)
    {
        movementVector = replay.movementVector;
        inputs = replay.inputs;
        switches = replay.switches;
        bag = replay.bag;
    }
}
