using System;
using Unity.Mathematics;

//I piece kicks are still awkward if using pivots.
public class DefaultSRS : IRuleset
{
    public int Pieces { get; } = 7;
    public bool Synchro { get; } = false;
    public bool SwapRotation { get; } = false;
    public string[] PieceNames { get; } =
    {
        "O", "I", "S", "T", "Z", "L", "J"
    };

    public bool CanMovePiece(in NetworkBoard board, int2 moveBy)
    {
        throw new NotImplementedException();
    }

    public bool CanRotatePiece(in NetworkBoard board, int oldRotationIndex, int rotationIndex)
    {
        throw new NotImplementedException();
    }

    public Span<int2> GetPiece()
    {
        throw new NotImplementedException();
    }

    public bool OnPieceMove(int2 moveBy)
    {
        throw new NotImplementedException();
    }

    public bool OnPieceRotate(int oldRotationIndex, int rotationIndex)
    {
        throw new NotImplementedException();
    }

    public void OnPieceSpawn()
    {
        throw new NotImplementedException();
    }

    public void ProcessPiece()
    {
        throw new NotImplementedException();
    }
}