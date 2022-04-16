using System;

public static class SIUnitsConversion
{
    //called Double Sided Comparison
    static bool DSC(double compareFrom, double compareTo)
    {
        return compareFrom >= compareTo || compareFrom <= -compareTo;
    }
    static double valueScale(double input, double scale)
    {
        return Math.Floor((input/scale)*100)/100;
    }
    static double[] timeScales = new double[] {3600, 60, 1, 0.001, 0.000001, 0.000000001};
    static string[] timeIndicators = new string[] {"h", "m", "s", "ms", "μs", "ns"};
    static double[] sizeScales = new double[] {1000000, 1000, 1, 0.001, 0.000001, 0.000000001};
    static string[] sizeIndicators = new string[] {"mgm", "km", "m", "mm", "μm", "nm"};

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