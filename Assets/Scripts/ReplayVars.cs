using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

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
public class ReplayVars
{
    public int boards;
    public int seed;
    public List<double[]> timings;
    public List<NativeList<float2>> movementVector;
    public List<NativeList<bool4x2>> inputs;
    public List<bool[]> switches;
    
    public ReplayVars (ReplayRecord replay)
    {
        boards = replay.boards;
        movementVector = replay.movementVector;
        inputs = replay.inputs;
        switches = replay.switches;
        seed = ReplayRecord.seed;
    }
}
