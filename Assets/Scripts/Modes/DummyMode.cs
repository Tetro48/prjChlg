using Discord;
using UnityEngine;
using Unity.Collections;

/*
    Project Challenger, an challenging Tetris game.
    Copyright (C) 2022, Aymir

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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

    public int GetResets() => 32;

    public virtual double GetSpawnDelay() => 20;
    public virtual int GetBGMType() => 0;

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

    public virtual bool BeforeStart()
    {
        return true;
    }
}