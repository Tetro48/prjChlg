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

public static class SIUnitsConversion
{
    //called Double Sided Comparison
    private static bool DSC(double compareFrom, double compareTo)
    {
        return compareFrom >= compareTo || compareFrom <= -compareTo;
    }

    private static double valueScale(double input, double scale)
    {
        return Math.Floor((input / scale) * 100 + 0.5) / 100;
    }

    private static double[] timeScales = new double[] { 3600, 60, 1, 0.001, 0.000001, 0.000000001 };
    private static string[] timeIndicators = new string[] { "h", "m", "s", "ms", "μs", "ns" };
    private static double[] sizeScales = new double[] { 1000000, 1000, 1, 0.001, 0.000001, 0.000000001 };
    private static string[] sizeIndicators = new string[] { "mgm", "km", "m", "mm", "μm", "nm" };

    ///<summary>
    /// This function assumes one unit is one meter in its configuration.
    ///</summary>
    public static string doubleToSize(double time, bool showSymbol = true)
    {
        double value;
        for (int i = 0; i < sizeScales.Length; i++)
        {
            if (DSC(time, sizeScales[i]))
            {
                value = valueScale(time, sizeScales[i]);
                return value + (showSymbol ? sizeIndicators[i] : string.Empty);
            }
        }
        return "0";
    }
    ///<summary>
    /// This function assumes one unit is one second in its configuration.
    ///</summary>
    public static string doubleToSITime(double time, bool showSymbol = true)
    {
        double value;
        for (int i = 0; i < timeScales.Length; i++)
        {
            if (DSC(time, timeScales[i]))
            {
                value = valueScale(time, timeScales[i]);
                return value + (showSymbol ? timeIndicators[i] : string.Empty);
            }
        }
        return "0";
    }
}