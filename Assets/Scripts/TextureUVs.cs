using System;
using System.Collections.Generic;
using Unity.Mathematics;

public static class TextureUVs
{
    static int2 size = new int2(4,10);
    public static float2[] UVs {get; private set;}
    public static void GenerateTextureUVs() 
    {
        UVs = new float2[size.x * size.y];
        for (int i = 0; i < size.x*size.y; i++)
        {
            UVs[i] = new float2(-(float)(i % size.x) / size.x, (float)Math.Floor((double)i / size.x + 1) / size.y);
        }
    }
}
