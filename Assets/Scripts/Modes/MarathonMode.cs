using Discord;
using TMPro;
using Unity.Collections;

/*
    Project Challenger, a challenging block stacking game.
    Copyright (C) 2022-2023, Aymir

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

namespace Tetro48.Modes
{
    public class MarathonMode : DummyMode
    {
        private int lines;
        private int score;
        private bool b2b;
        private TextMeshPro _text;
        private static readonly int[] lineScoreCount = { 100, 200, 400, 800, 1200, 1600, 2000, 2500 };
        public override FixedString64Bytes Name { get; } = "150 Lines Marathon";

        public override FixedString128Bytes Description { get; } = "A standard mode. Clear 150 lines!";
        public override Activity GetDiscordActivity() => new Activity
        {
            State = "Playing Marathon.",
            Details = $"Cleared {lines}/150 lines",
            Assets = {
                LargeImage = "icon"
            }
        };
        public override void OnLineClear(NetworkBoard boardRef, int lines, bool spin)
        {
            this.lines += lines;
            score += lineScoreCount[lines];
            if (lines >= 4 || spin)
            {
                b2b = true;
            }
            else
            {
                b2b = false;
            }
            if (this.lines >= 150)
            {
                boardRef.ending = true;
                boardRef.GameOver = true;
            }
        }
        public override double GetLineDropDelay()
        {
            return 20;
        }
        public override double GetLineSpawnDelay()
        {
            return 10;
        }
    }
}