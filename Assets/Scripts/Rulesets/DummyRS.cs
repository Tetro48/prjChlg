using System;
using Unity.Mathematics;

public class DummyRS : IRuleset
{
    public virtual int Pieces { get; } = 7;
    public virtual bool Synchro { get; }
    public virtual bool SwapRotation { get; }
    public virtual string[] PieceNames { get; } =
    {
        "O", "I", "S", "T", "Z", "L", "J"
    };

    // first index is piece id, second index is for rotation index. For most pieces, the first mino MUST BE in 0,0.
    private readonly Memory<int2>[][] pieceMatrix =
    {
        //O piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(1, 0), new int2(1, 1) },
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(1, 0), new int2(1, 1) },
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(1, 0), new int2(1, 1) },
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(1, 0), new int2(1, 1) },
        },
        //I piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(-2, 0) },
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(0, -2), new int2(0, -1) },
            new int2[] { new int2(0, -1), new int2(1, -1), new int2(-1, -1), new int2(-2, -1) },
            new int2[] { new int2(-1, 0), new int2(-1, 1), new int2(-1, -2), new int2(-1, -1)},
        },
        //S piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(-1,0), new int2(1,1), new int2(0,1) },
            new int2[] { new int2(0, 0), new int2(0, 1), new int2(1, 0), new int2(1, -1) },
            new int2[] { new int2(0, 0), new int2(-1, -1), new int2(1, 0), new int2(0, -1) },
            new int2[] { new int2(0, 0), new int2(-1, 0), new int2(-1, 1), new int2(0, -1) },
        },
        //T piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(0, -1), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, -1)},
            new int2[] { new int2(0, 0), new int2(-1, 0), new int2(0, -1), new int2(0, 1)},
        },
        //Z piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 1), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(1, -1), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, -1)},
            new int2[] { new int2(0, 0), new int2(-1, 0), new int2(0, -1), new int2(0, 1)},
        },
        // L piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(1, 1)},
            new int2[] { new int2(0, 0), new int2(1, -1), new int2(0, -1), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, -1)},
            new int2[] { new int2(0, 0), new int2(-1, 0), new int2(0, -1), new int2(0, 1)},
        },
        // J piece
        new Memory<int2>[]
        {
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(0, -1), new int2(0, 1)},
            new int2[] { new int2(0, 0), new int2(1, 0), new int2(-1, 0), new int2(0, -1)},
            new int2[] { new int2(0, 0), new int2(-1, 0), new int2(0, -1), new int2(0, 1)},
        },
    };
    private Memory<int2> GetPieceInMatrix(int id, int rotationIndex)
    {
        return pieceMatrix[id][rotationIndex];
    }
    public virtual bool CanMovePiece(in NetworkBoard board, int2 moveBy)
    {
        Span<int2> piece = GetPieceInMatrix((int)board.curType, board.rotationIndex).Span;
        for (int i = 0; i < piece.Length; i++)
        {
            if (board.grid.GetGridTile(piece[i].x, piece[i].y) > 0)
            {
                return false;
            }
        }
        return true;
    }

    public virtual bool CanRotatePiece(in NetworkBoard board, int oldRotationIndex, int newRotationIndex)
    {
        Span<int2> piece = GetPieceInMatrix((int)board.curType, newRotationIndex).Span;
        for (int i = 0; i < piece.Length; i++)
        {
            if (board.grid.GetGridTile(piece[i].x, piece[i].y) > 0)
            {
                return false;
            }
        }
        return true;
    }

    public virtual Span<int2> GetPiece()
    {
        throw new NotImplementedException();
    }

    public virtual bool OnPieceMove(int2 moveBy)
    {
        throw new NotImplementedException();
    }

    public virtual bool OnPieceRotate(int oldRotationIndex, int rotationIndex)
    {
        throw new NotImplementedException();
    }

    public virtual void OnPieceSpawn()
    {
        throw new NotImplementedException();
    }

    public virtual void ProcessPiece()
    {
        throw new NotImplementedException();
    }
}