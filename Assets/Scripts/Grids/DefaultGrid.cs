using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Grids
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