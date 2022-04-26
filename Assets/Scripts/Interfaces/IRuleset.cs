using System;
using Unity.Mathematics;

//Gives a lot of flexibility, however, you have to do a lot on your own.
public interface IRuleset
{
    int Pieces { get; }
    bool Synchro { get; }
    bool SwapRotation { get; }
    string[] PieceNames { get; }
    bool CanMovePiece(in NetworkBoard board, int2 moveBy);
    bool CanRotatePiece(in NetworkBoard board, int oldRotationIndex, int newRotationIndex);
    void ProcessPiece();

    /// <summary>
    /// Please do not overuse this function. It outputs a ref struct!
    /// </summary>
    Span<int2> GetPiece();

    /// <summary>
    /// This is called once a piece spawns.
    /// </summary>
    void OnPieceSpawn();
    bool OnPieceMove(int2 moveBy);
    bool OnPieceRotate(int oldRotationIndex, int rotationIndex);
}