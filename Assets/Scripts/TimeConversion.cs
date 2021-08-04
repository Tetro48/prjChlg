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
        return Math.Floor(((double)time/60000)%6).ToString() + Math.Floor(((double)time/6000)%10) + ":" + Math.Floor(((double)time%6000/1000)%6) + Math.Floor(((double)time%6000/100)%10) + ":" + Math.Floor((((double)time%100/1000)*100)%10) + Math.Floor((((double)time%100/100)*100)%10);
    }
}