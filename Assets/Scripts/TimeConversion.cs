using System;
using UnityEngine;

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
        Math.Floor((double)time/60000)%6,
        Math.Floor((double)time/6000)%10,
        Math.Floor((double)time%6000/1000)%6,
        Math.Floor((double)time%6000/100)%10,
        Math.Floor(((double)time%100/1000)*100)%10,
        Math.Floor(((double)time%100/100)*100)%10);
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