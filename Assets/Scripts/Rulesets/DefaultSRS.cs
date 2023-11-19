using System;
using Unity.Mathematics;
using Tetro48.Interfaces;

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

//I piece kicks are still awkward if using pivots.
namespace Tetro48.Rulesets
{
    public class DefaultSRS : IRuleset
    {
        public int Pieces { get; } = 7;
        public bool SwapRotation { get; } = false;
        public string[] PieceNames { get; } =
        {
            "O", "I", "S", "T", "Z", "L", "J"
        };
        /// <summary>
        /// RGB color array?
        /// </summary>
        public float3[] PieceColors { get; } =
        {
            new float3 { x = 1, y = 1, z = 0}, //O
            new float3 { x = 0, y = 1, z = 1}, //I
            new float3 { x = 0, y = 1, z = 0}, //S
            new float3 { x = 1, y = 0, z = 1}, //T
            new float3 { x = 1, y = 0, z = 0}, //Z
            new float3 { x = 1, y = 0.5f, z = 1}, //L
            new float3 { x = 0, y = 0, z = 1}, //J
        };

        public bool CanMovePiece(in NetworkBoard board, int2 moveBy)
        {
            throw new NotImplementedException();
        }

        public bool CanRotatePiece(in NetworkBoard board, int oldRotationIndex, int rotationIndex)
        {
            throw new NotImplementedException();
        }

        public Span<int2> GetPiece(int id)
        {
            throw new NotImplementedException();
        }

        public float3 GetPieceColor(int id)
        {
            throw new NotImplementedException();
        }

        public int GetTextureID(int pieceID) => 0;

        public bool OnPieceMove(int id, int2 moveBy)
        {
            throw new NotImplementedException();
        }

        public bool OnPieceRotate(int id, int oldRotationIndex, int rotationIndex)
        {
            throw new NotImplementedException();
        }

        public void OnPieceSpawn(int id)
        {
            throw new NotImplementedException();
        }

        public void ProcessPiece(in NetworkBoard board, int2 moveBy, float2 movement)
        {
            throw new NotImplementedException();
        }
    }
}