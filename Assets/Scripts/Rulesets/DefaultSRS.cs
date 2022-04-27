using System;
using Unity.Mathematics;

/*
    Project Challenger, an challenging Tetris game.
    Copyright (C) 2022, Aymir

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

//I piece kicks are still awkward if using pivots.
public class DefaultSRS : IRuleset
{
    public int Pieces { get; } = 7;
    public bool Synchro { get; } = false;
    public bool SwapRotation { get; } = false;
    public string[] PieceNames { get; } =
    {
        "O", "I", "S", "T", "Z", "L", "J"
    };

    public bool CanMovePiece(in NetworkBoard board, int2 moveBy)
    {
        throw new NotImplementedException();
    }

    public bool CanRotatePiece(in NetworkBoard board, int oldRotationIndex, int rotationIndex)
    {
        throw new NotImplementedException();
    }

    public Span<int2> GetPiece()
    {
        throw new NotImplementedException();
    }

    public bool OnPieceMove(int2 moveBy)
    {
        throw new NotImplementedException();
    }

    public bool OnPieceRotate(int oldRotationIndex, int rotationIndex)
    {
        throw new NotImplementedException();
    }

    public void OnPieceSpawn()
    {
        throw new NotImplementedException();
    }

    public void ProcessPiece()
    {
        throw new NotImplementedException();
    }
}