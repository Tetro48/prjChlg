using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;

public class NextPieceManager : MonoBehaviour
{
    EntityManager entityManager;
    BlobAssetStore blobAssetStore;
    void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        blobAssetStore = new BlobAssetStore();
    }
    [SerializeField]Entity prefab;
    Entity[] entityTiles;
    void Destroy(Entity entity)
    {
        entityManager.DestroyEntity(entity);
    }
    Entity Instantiate(Entity entity)
    {
        return entityManager.Instantiate(entity);
    }
    public void SetNextPiece(int2[] tiles, int textureID = 0, float scale = 1f)
    {
        transform.localScale = new Vector3(scale, scale, scale);
        if(entityTiles != null)
        {
            for (int i = 0; i < entityTiles.Length; i++)
            {
                
            }
        }
        if(tiles == null) return;
        entityTiles = new Entity[tiles.Length];
        for (int i = 0; i < entityTiles.Length; i++)
        {
            Entity tile = Instantiate(prefab, transform);
            tile.GetComponent<MeshRenderer>().material.mainTextureOffset = Vector2.right - (Vector2)TextureUVs.UVs[textureID];
            tile.transform.localPosition = new Vector3(tiles[i].x, tiles[i].y);
            entityTiles[i] = tile; 
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
