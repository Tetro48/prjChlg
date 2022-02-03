using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using System;

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

[System.Serializable]
public class ReplayVars : IDisposable
{
    public int boards;
    public int seed;
    public double[][] timings;
    public float2[][] movementVector;
    public bool4x2[][] inputs;
    public bool[][] switches;
    
    public void Dispose()
    {
        UnityEngine.Debug.Log("Disposed of ReplayVars");
    }
    public ReplayVars (ReplayRecord replay)
    {
        boards = replay.boards;
        inputs = arrayOfListsToArrayOfArrays(replay.inputs);
        movementVector = arrayOfListsToArrayOfArrays(replay.movementVector);
        switches = replay.switches;
        seed = ReplayRecord.seed;
    }
    public T[][] arrayOfListsToArrayOfArrays<T>(List<T>[] list)
    {
        T[][] array = new T[list.Length][];
        for (int i = 0; i < list.Length; i++)
        {
            array[i] = list[i].ToArray();
        }
        return array;
    }
}
