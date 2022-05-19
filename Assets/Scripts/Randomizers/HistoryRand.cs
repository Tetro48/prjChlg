using System.Collections;
using UnityEngine;

namespace Tetro48.Randomizers
{
    public class HistoryRand : DummyRand
    {
        public int[] PieceHistory;
        public bool FirstPiece = true;
        public virtual int HistorySize { get; }
        public virtual int HistoryRolls { get; }
        public override int GetPieceID(bool usePiece = true)
        {
            uint randState = _random.state;
            if (FirstPiece)
            {
                int randomPiece = GetRandomPieceID();
                if (usePiece)
                {
                    FirstPiece = false;
                    PieceHistory[0] = randomPiece;
                }
                else
                {
                    _random.state = randState;
                }
                return randomPiece;
            }
            else
            {
                int randomPiece = 0;
                for (int i = 0; i < HistoryRolls; i++)
                {
                    randomPiece = GetRandomPieceID();
                    for (int j = 0; j < HistorySize; j++)
                    {
                        if (randomPiece != PieceHistory[j])
                        {
                            if (usePiece)
                            {
                                PieceHistory[j] = GetRandomPieceID();
                            }
                            return randomPiece;
                        }
                    }
                }
                if (!usePiece)
                {
                    _random.state = randState;
                }
                return randomPiece;
            }
        }
        public override void InitPieceIdentities(string[] ids)
        {
            base.InitPieceIdentities(ids);
            int randID = GetRandomPieceID();
            PieceHistory = new int[] { randID, randID, randID, randID };
        }
    }
}