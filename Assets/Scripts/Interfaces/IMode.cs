using UnityEngine;
using Unity.Collections;
using Discord;

//Mode system???
public interface IMode
{
    public FixedString64Bytes Name { get; set; }
    public FixedString128Bytes Description { get; set; }
    public void OnObjectSpawn(Transform transformRef);
    //Reminder: Keep these getters simple. Seriously.
    //Computing a new variable every time a mode or a core tries to get from will be considered somewhat wasteful, since those 7 could be accessed quite frequently.
    public double GetSpawnDelay();
    public double GetLockDelay();
    public double GetLineDropDelay();
    public double GetLineSpawnDelay();
    public double GetDAS();
    public int GetResets();
    public Activity GetDiscordActivity();
    /// <param name="board">A board reference. In case, you can use functions and/or directly modify variables in it. Be mindful though!</param>
    public void OnUpdate(float dt, NetworkBoard board);
    //why the piece name??? Mode creator(s) will struggle.
    public void OnPieceSpawn(string piece_name);
    public void OnPieceLock(string piece_name);
    public void OnLineClear(NetworkBoard boardRef, int lines, bool spin);
    public void OnLineDrop(NetworkBoard boardRef, int lines, bool spin);
    public void OnBlockOut();
}