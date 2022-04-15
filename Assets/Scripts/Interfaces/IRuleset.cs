using System;
using Unity.Mathematics;

//Gives a lot of flexibility, however, you have to do a lot on your own.
public interface IRuleset
{
    public bool CanMovePiece();
    public bool CanRotatePiece();

    /// <summary>
    /// Please do not overuse this function. It outputs a ref struct!
    /// </summary>
    public Span<int2> GetPiece();

    /// <summary>
    /// Please do not overuse this function.
    /// This should only be called once a piece spawns.
    /// </summary>
    public void OnPieceSpawn();
    public void OnPieceMove();
    public void OnPieceRotate();
}