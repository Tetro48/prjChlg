using Tetro48.Interfaces;

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

namespace Tetro48.Grids
{
    public class DefaultGrid : IGrid
    {
        public int[] FlatGrid;

        public bool[] UnclearableLineFlags;

        public int GridWidth { get; set; }

        public int GridHeight { get; set; }

        public void InitializeGrid(int gridWidth, int gridHeight)
        {
            GridWidth = gridWidth;
            GridHeight = gridHeight;
            FlatGrid = new int[gridWidth * gridHeight];
        }

        public int GetGridTile(int x, int y)
        {
            return FlatGrid[y * GridWidth + x];
        }

        public void SetGridTile(int x, int y, int tileID)
        {
            FlatGrid[y * GridWidth + x] = tileID;
        }

        public bool IsLineFull(uint y)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                if (FlatGrid[(y * GridWidth) + x] == 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool IsLineFull(uint y, bool clear)
        {
            for (int i = 0; i < GridWidth; i++)
            {
                if (GetGridTile(i, (int)y) == 0)
                {
                    return false;
                }
            }
            if (clear && !UnclearableLineFlags[y])
            {
                ClearLine(y);
            }
            return true;
        }

        public bool ClearLine(uint y)
        {
            bool flag = false;
            for (int x = 0; x < GridWidth; x++)
            {
                if (GetGridTile(x, (int)y) != 0) flag = true;
                SetGridTile(x, (int)y, 0);
            }
            return flag;
        }

        public void SetUnclearableLine(uint y, bool flag)
        {
            UnclearableLineFlags[y] = flag;
        }
    }
}