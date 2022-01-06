using Unity.Mathematics;

public static class VoxelData
{
    public static readonly float3[] voxelVerts = new float3[8] {
        new float3(0f, 0f, 0f),
        new float3(1f, 0f, 0f),
        new float3(1f, 1f, 0f),
        new float3(0f, 1f, 0f),
        new float3(0f, 0f, 1f),
        new float3(1f, 0f, 1f),
        new float3(1f, 1f, 1f),
        new float3(0f, 1f, 1f),
    };
    public static readonly int3[] faceChecks = new int3[6] {
        new int3(0, 0, -1),
        new int3(0, 0, 1),
        new int3(0, 1, 0),
        new int3(0, -1, 0),
        new int3(-1, 0, 0),
        new int3(1, 0, 0),
    };

    public static readonly int[,] voxelTris = new int[6,4] {
        {0, 3, 1, 2},
        {5, 6, 4, 7},
        {3, 7, 2, 6},
        {1, 5, 0, 4},
        {4, 7, 0, 3},
        {1, 2, 5, 6},
    };

    public static readonly float2[] voxelUVs = new float2[4] {
        new float2(0f, 0f),
        new float2(0f, 1f),
        new float2(1f, 0f),
        new float2(1f, 1f),
    };
}
