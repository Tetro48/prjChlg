using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    int vertexIndex = 0;
    int2 chunkSize = new int2(10, 40);
    NativeList<Vector3> verts = new NativeList<Vector3>();
    NativeList<int> tris = new NativeList<int>();
    NativeList<Vector2> UVs = new NativeList<Vector2>();
    

    void UpdateChunk(in int2 size, in int[,] textureIDs)
    {
        
    }
    bool CheckVoxel(in int3 tile)
    {
        return math.all(tile.xy >= int2.zero) && math.all(tile.xy < chunkSize) && tile.z >= 0;
    }

    void AddVoxelDataToChunk(int3 tile)
    {
        for (int p = 0; p < 6; p++)
        {
            if(!CheckVoxel(tile + VoxelData.faceChecks[p]))
            {
                verts.Add((float3)tile + VoxelData.voxelVerts [VoxelData.voxelTris[p, 0]]);
                verts.Add((float3)tile + VoxelData.voxelVerts [VoxelData.voxelTris[p, 1]]);
                verts.Add((float3)tile + VoxelData.voxelVerts [VoxelData.voxelTris[p, 2]]);
                verts.Add((float3)tile + VoxelData.voxelVerts [VoxelData.voxelTris[p, 3]]);
                UVs.Add(VoxelData.voxelUVs[0]);
                UVs.Add(VoxelData.voxelUVs[1]);
                UVs.Add(VoxelData.voxelUVs[2]);
                UVs.Add(VoxelData.voxelUVs[3]);
                tris.Add(vertexIndex);
                tris.Add(vertexIndex+1);
                tris.Add(vertexIndex+2);
                tris.Add(vertexIndex+2);
                tris.Add(vertexIndex+1);
                tris.Add(vertexIndex+3);
                vertexIndex += 4;
            }
        }
    }

    void CreateMesh() {
        
        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = UVs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }
}
