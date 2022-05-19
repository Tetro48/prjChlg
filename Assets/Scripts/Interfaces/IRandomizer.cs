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

namespace Tetro48.Interfaces
{
    public interface IRandomizer
    {
        //Initialize a randomizer with a seed.
        public void InitRand(int seed);
        public void InitPieceIdentities(string[] ids);
        public int GetPieceID(bool usePiece = true);
        public string GetPieceName(bool usePiece = false);
    }
}