using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

//didn't find a way to fix this triangle mess.
public class Chunk : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    private int vertexIndex = 0;
    private int2 chunkSize = new int2(10, 40);
    private int[,] textureIDs;
    private float[,] transparencyGrid;
    private List<Vector3> verts = new List<Vector3>();
    private List<int> tris = new List<int>();
    private List<Vector2> UVs = new List<Vector2>();


    public void UpdateChunk(in int2 size, in int[,] textures, in float[,] transparencies)
    {
        if (textureIDs != null && transparencyGrid != null)
        {
            if (textureIDs.Equals(textures) && transparencyGrid.Equals(transparencies))
            {
                return;
            }
        }

        vertexIndex = 0;
        verts.Clear();
        tris.Clear();
        UVs.Clear();
        chunkSize = size;
        textureIDs = textures;
        transparencyGrid = transparencies;
        CreateMeshData();
        CreateMesh();
    }
    public void UpdateChunk(in int2 size, in int3[] textures, in float[,] transparencies)
    {
        chunkSize = size;
        textureIDs = new int[chunkSize.x, chunkSize.y];
        Debug.Log(textureIDs.GetLength(1));
        for (int i = 0; i < chunkSize.y; i++)
        {
            for (int x = 0; x < chunkSize.x; x++)
            {
                textureIDs[x, i] = -1;
            }
        }
        for (int i = 0; i < textures.Length; i++)
        {
            textureIDs[textures[i].x, textures[i].y] = textures[i].z;
        }
        for (int i = 0; i < 40; i++)
        {
            Debug.Log(textureIDs[0, i]);
            Debug.Log(textureIDs[1, i]);
            Debug.Log(textureIDs[2, i]);
            Debug.Log(textureIDs[3, i]);
            Debug.Log(textureIDs[4, i]);
            Debug.Log(textureIDs[5, i]);
            Debug.Log(textureIDs[6, i]);
            Debug.Log(textureIDs[7, i]);
            Debug.Log(textureIDs[8, i]);
            Debug.Log(textureIDs[9, i]);
        }
        transparencyGrid = transparencies;
        CreateMeshData();
        CreateMesh();
    }

    private bool CheckVoxel(in int3 tile, in bool isFull = true)
    {
        // Debug.Log(tile.xy);
        if (tile.z != 0)
        {
            return false;
        }

        if ((math.any(tile.xy < int2.zero) || math.any(tile.xy >= chunkSize - 1)))
        {
            return false;
        }

        return transparencyGrid[tile.x, tile.y] < 0.5 && isFull;
    }

    private void AddVoxelDataToChunk(int3 tile)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(new int3(tile.xy, 0) + VoxelData.faceChecks[p], tile.z >= 0))
            {
                // Debug.Log("Empty voxel at " + (new int3(tile.xy, 0) + VoxelData.faceChecks[p]));
                verts.Add((float3)tile + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                verts.Add((float3)tile + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                verts.Add((float3)tile + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                verts.Add((float3)tile + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);
                AddTexture(tile.z);
                tris.Add(vertexIndex);
                tris.Add(vertexIndex + 1);
                tris.Add(vertexIndex + 2);
                tris.Add(vertexIndex + 2);
                tris.Add(vertexIndex + 1);
                tris.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }

    private void CreateMeshData()
    {
        for (int x = 0; x < chunkSize.x; x++)
        {
            for (int y = 0; y < chunkSize.y; y++)
            {
                if (transparencyGrid[x, y] < 0.5)
                {
                    AddVoxelDataToChunk(new int3(x, y, textureIDs[x, y]));
                }
            }
        }
    }

    private void CreateMesh()
    {

        // Mesh mesh = meshFilter.mesh;
        meshFilter.mesh.Clear();
        // Debug.Log(ReferenceEquals(meshFilter.mesh, mesh));
        meshFilter.mesh.vertices = verts.ToArray();
        meshFilter.mesh.triangles = tris.ToArray();
        meshFilter.mesh.uv = UVs.ToArray();

        meshFilter.mesh.RecalculateNormals();
        meshFilter.mesh.MarkDynamic();

        // meshFilter.mesh = mesh;
    }

    private void AddTexture(int textureID)
    {
        // if(textureID < 0) return;
        float2 xy = textureID < 0 ? float2.zero : TextureUVs.UVs[textureID];

        UVs.Add(new Vector2(xy.x, xy.y));
        UVs.Add(new Vector2(xy.x, xy.y + 0.1f));
        UVs.Add(new Vector2(xy.x + 0.25f, xy.y));
        UVs.Add(new Vector2(xy.x + 0.25f, xy.y + 0.1f));
    }
}
