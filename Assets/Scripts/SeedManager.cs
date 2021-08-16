using System;

public static class SeedManager
{
    public static int seed;

    public static int NewSeed()
    {
        seed = Environment.TickCount;
        return seed;
    }
}