using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

public class NextPieceManager : MonoBehaviour
{
    [SerializeField]GameObject prefab;
    ObjectPool<GameObject> pieceTiles;
    GameObject[] gameObjectTiles;

    void Awake()
    {
        pieceTiles = new ObjectPool<GameObject>(() => {
            return Instantiate(prefab);
        }, obj => obj.SetActive(true),
        obj => obj.SetActive(false), obj => Destroy(obj), true, 16);
    }
    public void Dispose()
    {
        pieceTiles.Dispose();
    }
    public void SetNextPiece(int2[] tiles, int textureID = 0, float scale = 1f)
    {
        transform.localScale = new Vector3(scale, scale, scale);
        if(gameObjectTiles != null)
        {
            for (int i = 0; i < gameObjectTiles.Length; i++)
            {
                pieceTiles.Release(gameObjectTiles[i]);
            }
        }
        if(tiles == null) return;
        gameObjectTiles = new GameObject[tiles.Length];
        for (int i = 0; i < gameObjectTiles.Length; i++)
        {
            GameObject tile = Instantiate(prefab, transform);
            tile.GetComponent<MeshRenderer>().material.mainTextureOffset = Vector2.right - (Vector2)TextureUVs.UVs[textureID];
            tile.transform.localPosition = new Vector3(tiles[i].x, tiles[i].y);
            gameObjectTiles[i] = tile; 
        }
    }
    public void SetNextPiece(int3[] tiles, float scale = 1f)
    {
        transform.localScale = new Vector3(scale, scale, scale);
        if(gameObjectTiles != null)
        {
            for (int i = 0; i < gameObjectTiles.Length; i++)
            {
                pieceTiles.Release(gameObjectTiles[i]);
            }
        }
        if(tiles == null) return;
        gameObjectTiles = new GameObject[tiles.Length];
        for (int i = 0; i < gameObjectTiles.Length; i++)
        {
            GameObject tile = Instantiate(prefab, transform);
            tile.GetComponent<MeshRenderer>().material.mainTextureOffset = Vector2.right - (Vector2)TextureUVs.UVs[tiles[i].z];
            tile.transform.localPosition = new float3(tiles[i].xy, 0f);
            gameObjectTiles[i] = tile; 
        }
    }
}
