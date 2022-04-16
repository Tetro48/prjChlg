using Unity.Mathematics;
using Unity.Collections;

public interface IRandomizer
{
    //Initializer
    public void InitRand();
    public NativeArray<int2> GetPiece();
    public int GetPieceID();
}