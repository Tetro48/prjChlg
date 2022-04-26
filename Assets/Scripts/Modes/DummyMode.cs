using Discord;
using UnityEngine;
using Unity.Collections;

public class DummyMode : IMode
{
    public virtual FixedString64Bytes Name { get; set; } = "Dummy Mode";
    public virtual FixedString128Bytes Description { get; set; } = "A dummy mode to ease on mode development.";

    public virtual double GetDAS() => 15;

    public virtual Activity GetDiscordActivity() => new Activity
    {
        State = "Playing a... dummy mode?",
        Assets = {
                LargeImage = "icon"
            }
    };

    public virtual double GetLineDropDelay() => 10;

    public virtual double GetLineSpawnDelay() => 10;

    public virtual double GetLockDelay() => 30;

    public int GetResets() => 20;

    public virtual double GetSpawnDelay() => 20;

    public virtual void OnBlockOut()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnLineClear(NetworkBoard boardRef, int lines, bool spin)
    {
        
    }

    public virtual void OnLineDrop(NetworkBoard boardRef, int lines, bool spin)
    {
        
    }

    public virtual void OnObjectSpawn(Transform transformRef)
    {

    }

    public virtual void OnPieceLock(string piece_name)
    {
        
    }

    public virtual void OnPieceSpawn(string piece_name)
    {
        
    }

    public virtual void OnUpdate(float dt, NetworkBoard board)
    {
        
    }
}