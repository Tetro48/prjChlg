
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

/// <summary>
/// This by itself can't be used. It must be inherited and overriden.
/// </summary>
public class DummyRand : IRandomizer
{
    public Unity.Mathematics.Random _random;
    public string[] PieceNames;
    public int GetRandomPieceID()
    {
        return _random.NextInt(0, PieceNames.Length - 1);
    }
    public virtual int GetPieceID(bool usePiece = true)
    {
        throw new System.NotImplementedException();
    }

    public virtual string GetPieceName(bool usePiece = false)
    {
        return PieceNames[GetPieceID(usePiece)];
    }

    public virtual void InitPieceIdentities(string[] ids)
    {
        PieceNames = ids;
    }

    public void InitRand(int seed)
    {
        _random = new Unity.Mathematics.Random((uint)seed);
    }
}