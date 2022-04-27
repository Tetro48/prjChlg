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
//Gives a lot of flexibility, however, you have to do a lot on your own.
public interface IRuleset
{
    int Pieces { get; }
    bool Synchro { get; }
    bool SwapRotation { get; }
    string[] PieceNames { get; }
    bool CanMovePiece(in NetworkBoard board, int2 moveBy);
    bool CanRotatePiece(in NetworkBoard board, int oldRotationIndex, int newRotationIndex);
    void ProcessPiece();

    /// <summary>
    /// Please do not overuse this function. It outputs a ref struct!
    /// </summary>
    Span<int2> GetPiece();

    /// <summary>
    /// This is called once a piece spawns.
    /// </summary>
    void OnPieceSpawn();
    bool OnPieceMove(int2 moveBy);
    bool OnPieceRotate(int oldRotationIndex, int rotationIndex);
}