using System;

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

public static class TimeConversion
{
    ///<summary>
    /// Converts integer into time string. 1 int unit to 10 ms.
    /// Inputting integer 1 outputs string 00:00:01.
    /// Inputting integer 3000 outputs string 00:30:00.
    /// Inputting integer 6000 outputs string 01:00:00
    ///</summary>
    public static string timeCount(int time)
    {
        return String.Format("{0}{1}:{2}{3}:{4}{5}",
        Math.Floor((double)time / 60000) % 6,
        Math.Floor((double)time / 6000) % 10,
        Math.Floor((double)time % 6000 / 1000) % 6,
        Math.Floor((double)time % 6000 / 100) % 10,
        Math.Floor(((double)time % 100 / 1000) * 100) % 10,
        Math.Floor(((double)time % 100 / 100) * 100) % 10);
    }
    //Explains itself.
    public static string doubleFloatTimeCount(double time)
    {
        return String.Format("{0}{1}:{2}{3}:{4}",
        Math.Floor(time / 600) % 6,
        Math.Floor(time / 60) % 10,
        Math.Floor(time % 60 / 10) % 6,
        Math.Floor(time % 60) % 10,
        Math.Floor(time % 1 * 100));
    }
}