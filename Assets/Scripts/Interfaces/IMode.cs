using System;

//Mode system???
public interface IMode
{
    public void OnObjectSpawn();
    public double GetSpawnDelay();
    public double GetLockDelay();
    public double GetLineDropDelay();
    public double GetLineSpawnDelay();
    public double GetDAS();
    public void OnUpdate(float dt);
    //why the piece name??? Mode creator(s) will struggle.
    public void OnPieceSpawn(string piece_name);
    public void OnPieceLock(string piece_name);
    public void OnLineClear(int lines, bool spin);
    public void OnLineDrop(int lines, bool spin);
    public void OnBlockOut();
}