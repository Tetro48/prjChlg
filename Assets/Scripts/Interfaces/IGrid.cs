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
