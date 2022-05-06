
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

public class BagRand : DummyRand
{
    public int[] bag;
    public int iteration;
    public override int GetPieceID(bool usePiece = true)
    {
        int piecesLen = PieceNames.Length;
        if (iteration % piecesLen == 0)
        {
            bag.Shuffle();
        }
        iteration++;
        return bag[iteration % piecesLen];
    }

    public override void InitPieceIdentities(string[] ids)
    {
        base.InitPieceIdentities(ids);
        bag = new int[ids.Length];
        for (int i = 0; i < ids.Length; i++)
        {
            bag[i] = i;
        }
    }
}