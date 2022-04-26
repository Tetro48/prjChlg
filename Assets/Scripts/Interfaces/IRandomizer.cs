public interface IRandomizer
{
    //Initialize a randomizer with a seed.
    public void InitRand(long seed);
    public void InitPieceIdentities(string[] ids);
    public int GetPieceID(int nextPieces = 0);
    public string GetPieceName(int nextPieces = 0);
}