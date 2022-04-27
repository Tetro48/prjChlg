
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
public interface IGrid
{
    int[] FlatGrid { get; set; }
    int GridWidth { get; set; }
    int GridHeight { get; set; }
    void InitializeGrid(int gridWidth, int gridHeight)
    {
        GridWidth = gridWidth;
        GridHeight = gridHeight;
        FlatGrid = new int[GridWidth * GridHeight];
    }
    /// <summary>
    /// It's a getter of a flat grid, for simplification.
    /// </summary>
    int GetGridTile(int x, int y)
    {
        return FlatGrid[y * GridWidth + x];
    }
    /// <summary>
    /// It's a setter of a flat grid, for simplification.
    /// </summary>
    void SetGridTile(int x, int y, int tileID)
    {
        FlatGrid[y * GridWidth + x] = tileID;
    }
    /// <returns>True meaning a full line</returns>
    bool IsLineFull(uint y)
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
}
