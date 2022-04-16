using System;
using Unity.Mathematics;

//I piece kicks are still awkward if using pivots.
public class DefaultSRS : IRuleset
{
    public bool CanMovePiece()
    {
        throw new NotImplementedException();
    }

    public bool CanRotatePiece()
    {
        throw new NotImplementedException();
    }

    public Span<int2> GetPiece()
    {
        throw new NotImplementedException();
    }

    public void OnPieceMove()
    {
        throw new NotImplementedException();
    }

    public void OnPieceRotate()
    {
        throw new NotImplementedException();
    }

    public void OnPieceSpawn()
    {
        throw new NotImplementedException();
    }
}