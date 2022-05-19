using Discord;
using Unity.Collections;
using UnityEngine;

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

namespace Tetro48.Interfaces
{
    //Mode system???
    public interface IMode
    {
        public FixedString64Bytes Name { get; }
        public FixedString128Bytes Description { get; }
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
        public int GetBGMType();
        /// <summary>
        /// This will get executed when pressing Start. It'll determine if you can start it.
        /// Side note: This won't work with C# version lower than 8.
        /// </summary>
        /// <returns>False -> Stops from starting up a mode.</returns>
        public bool BeforeStart();
        /// <param name="board">A board reference. In case, you can use functions and/or directly modify variables in it. Be mindful though!</param>
        public void OnUpdate(float dt, NetworkBoard board);
        //why the piece name??? Mode creator(s) will struggle.
        public void OnPieceSpawn(string piece_name);
        public void OnPieceLock(string piece_name);
        public void OnLineClear(NetworkBoard boardRef, int lines, bool spin);
        public void OnLineDrop(NetworkBoard boardRef, int lines, bool spin);
        public void OnBlockOut();
    }
}