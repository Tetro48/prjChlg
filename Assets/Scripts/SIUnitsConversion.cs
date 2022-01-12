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
        return Math.Floor(input*100)/(100*scale);
    }
    static double[] scales = new double[] {3600, 60, 1, 0.001, 0.000001, 0.000000001};
    static string[] indicators = new string[] {"h", "m", "s", "ms", "μs", "ns"};
    public static string doubleToSITime(double time, bool showSymbol = true)
    {
        double value;
        for (int i = 0; i < scales.Length; i++)
        {
            if (DSC(time, scales[i])) 
            {
                value = valueScale(time, scales[i]);
                return value + (showSymbol ? indicators[i] : string.Empty);
            }
        }
        return "out-of-bounds!";
    }
}