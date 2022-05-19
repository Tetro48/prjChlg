
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
    public interface IGrid
    {
        int GridWidth { get; set; }
        int GridHeight { get; set; }
        void InitializeGrid(int gridWidth, int gridHeight);
        /// <summary>
        /// It's a getter of a flat grid, for simplification.
        /// </summary>
        int GetGridTile(int x, int y);
        /// <summary>
        /// It's a setter of a flat grid, for simplification.
        /// </summary>
        void SetGridTile(int x, int y, int tileID);
        /// <returns>True meaning a full line</returns>
        bool IsLineFull(uint y, bool clear);
        /// <summary>
        /// Clears a line.
        /// </summary>
        /// <returns>False if empty.</returns>
        bool ClearLine(uint y);
        /// <summary>
        /// A special use case. It'd usually flag a line to not clear at all.
        /// </summary>
        void SetUnclearableLine(uint y, bool flag);

    }
}