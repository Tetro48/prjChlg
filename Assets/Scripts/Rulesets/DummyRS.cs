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

public class DummyRS : IRuleset
{
    public virtual int Pieces { get; } = 7;
    public virtual bool Synchro { get; }
    public virtual bool SwapRotation { get; }
    public virtual string[] PieceNames { get; } =
    {
        "O", "I", "S", "T", "Z", "L", "J"
    };
    /// <summary>
    /// RGB color array?
    /// </summary>
    public virtual float3[] PieceColors { get; } =
    {
        new float3 { x = 1, y = 1, z = 0}, //O
        new float3 { x = 0, y = 1, z = 1}, //I
        new float3 { x = 0, y = 1, z = 0}, //S
        new float3 { x = 1, y = 0, z = 1}, //T
        new float3 { x = 1, y = 0, z = 0}, //Z
        new float3 { x = 1, y = 0.5f, z = 1}, //L
        new float3 { x = 0, y = 0, z = 1}, //J

    };

    // first index is piece id, second index is for rotation index. For most pieces, the first mino MUST BE in 0,0.
    private readonly Memory<int2>[][] pieceMatrix =
    {
        //O piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(1, 0), new int2(1, 1) },
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(1, 0), new int2(1, 1) },
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(1, 0), new int2(1, 1) },
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(1, 0), new int2(1, 1) },
        },
        //I piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(-2, 0) },
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(0, -2), new int2(0, -1) },
            new int2[] { new int2(0, -1), new int2(1, -1), new int2(-1, -1), new int2(-2, -1) },
            new int2[] { new int2(-1, 0), new int2(-1, 1), new int2(-1, -2), new int2(-1, -1)},
        },
        //S piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(-1,0), new int2(1,1), new int2(0,1) },
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(1, 0), new int2(1, -1) },
            new int2[] { new int2(0, 0), new int2(-1, -1), new int2(1, 0), new int2(0, -1) },
            new int2[] { new int2(0, 0), new int2(-1, 0), new int2(-1, 1), new int2(0, -1) },
        },
        //T piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(0, -1), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, -1)},
            new int2[] { new int2(0, 0), new int2(-1, 0), new int2(0, -1), new int2(0, 1)},
        },
        //Z piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 1), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(1, -1), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, -1)},
            new int2[] { new int2(0, 0), new int2(-1, 0), new int2(0, -1), new int2(0, 1)},
        },
        // L piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(1, 1)},
            new int2[] { new int2(0, 0), new int2(1, -1), new int2(0, -1), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, -1)},
            new int2[] { new int2(0, 0), new int2(-1, 0), new int2(0, -1), new int2(0, 1)},
        },
        // J piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(0, -1), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, -1)},
            new int2[] { new int2(0, 0), new int2(-1, 0), new int2(0, -1), new int2(0, 1)},
        },
    };
    private Memory<int2> GetPieceInMatrix(int id, int rotationIndex)
    {
        return pieceMatrix[id][rotationIndex];
    }
    public virtual bool CanMovePiece(in NetworkBoard board, int2 moveBy)
    {
        Span<int2> piece = GetPieceInMatrix((int)board.curType, board.rotationIndex).Span;
        for (int i = 0; i < piece.Length; i++)
        {
            if (board.grid.GetGridTile(piece[i].x, piece[i].y) > 0)
            {
                return false;
            }
        }
        return true;
    }

    public virtual bool CanRotatePiece(in NetworkBoard board, int oldRotationIndex, int newRotationIndex)
    {
        Span<int2> piece = GetPieceInMatrix((int)board.curType, newRotationIndex).Span;
        for (int i = 0; i < piece.Length; i++)
        {
            if (board.grid.GetGridTile(piece[i].x, piece[i].y) > 0)
            {
                return false;
            }
        }
        return true;
    }

    public virtual Span<int2> GetPiece(int id)
    {
        throw new NotImplementedException();
    }

    public virtual bool OnPieceMove(int id, int2 moveBy)
    {
        throw new NotImplementedException();
    }

    public virtual bool OnPieceRotate(int id, int oldRotationIndex, int rotationIndex)
    {
        throw new NotImplementedException();
    }

    public virtual void OnPieceSpawn(int id)
    {
        throw new NotImplementedException();
    }

    public virtual void ProcessPiece(in NetworkBoard board, int2 moveBy, float2 movement)
    {
        if (Synchro)
        {

        }
        board.MovePiece(moveBy, true);
    }

    public float3 GetPieceColor(int id)
    {
        return PieceColors[id];
    }

    public int GetTextureID(int pieceID) => 0;
}