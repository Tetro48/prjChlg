
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

namespace Tetro48.Randomizers
{
    public class HistoryRand : DummyRand
    {
        public int[] PieceHistory;
        public bool FirstPiece = true;
        public virtual int HistorySize { get; }
        public virtual int HistoryRolls { get; }
        public override int GetPieceID(bool usePiece = true)
        {
            uint randState = _random.state;
            if (FirstPiece)
            {
                int randomPiece = GetRandomPieceID();
                if (usePiece)
                {
                    FirstPiece = false;
                    PieceHistory[0] = randomPiece;
                }
                else
                {
                    _random.state = randState;
                }
                return randomPiece;
            }
            else
            {
                int randomPiece = 0;
                for (int i = 0; i < HistoryRolls; i++)
                {
                    randomPiece = GetRandomPieceID();
                    for (int j = 0; j < HistorySize; j++)
                    {
                        if (randomPiece != PieceHistory[j])
                        {
                            if (usePiece)
                            {
                                PieceHistory[j] = GetRandomPieceID();
                            }
                            return randomPiece;
                        }
                    }
                }
                if (!usePiece)
                {
                    _random.state = randState;
                }
                return randomPiece;
            }
        }
        public override void InitPieceIdentities(string[] ids)
        {
            base.InitPieceIdentities(ids);
            int randID = GetRandomPieceID();
            PieceHistory = new int[HistorySize];
            for (int i = 0; i < HistorySize; i++)
            {
                PieceHistory[i] = randID;
            }
        }
    }
}