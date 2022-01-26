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
    int[,] textureIDs;
    float[,] transparencyGrid;
    NativeList<Vector3> verts = new NativeList<Vector3>();
    NativeList<int> tris = new NativeList<int>();
    NativeList<Vector2> UVs = new NativeList<Vector2>();
    

    public void UpdateChunk(in int2 size, in int[,] textures, in float[,] transparencies)
    {
        chunkSize = size;
        textureIDs = textures;
        transparencyGrid = transparencies;
        CreateMeshData();
        CreateMesh();
    }
    public void UpdateChunk(in int2 size, in int3[] textures, in float[,] transparencies)
    {
        chunkSize = size;
        textureIDs = new int[chunkSize.x,chunkSize.y];
        for (int i = 0; i < textures.Length; i++)
        {
            textureIDs[textures[i].x, textures[i].y] = textures[i].z;
        }
        transparencyGrid = transparencies;
        CreateMeshData();
        CreateMesh();
    }
    bool CheckVoxel(in int3 tile)
    {
        return (math.all(tile.xy >= int2.zero) && math.all(tile.xy < chunkSize) && tile.z >= 0) || transparencyGrid[tile.x, tile.y] < 1;
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
                AddTexture(tile.z);
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

    void CreateMeshData()
    {
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                AddVoxelDataToChunk(new int3(x, y, textureIDs[x,y]));
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
    void AddTexture(int textureID) {
        float2 xy = TextureUVs.UVs[textureID];

        UVs.Add(new Vector2(xy.x, xy.y));
        UVs.Add(new Vector2(xy.x, xy.y + 0.25f));
        UVs.Add(new Vector2(xy.x + 0.25f, xy.y));
        UVs.Add(new Vector2(xy.x + 0.25f, xy.y + 0.25f));
    }
}
