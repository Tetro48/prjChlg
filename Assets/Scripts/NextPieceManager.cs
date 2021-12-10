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
        entityManager.Instantiate(entity);
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
            Vector2 offset = new Vector2(-(float)(textureID % 4) / 4, (float)Math.Floor((double)textureID/4+1) / 10);
            tile.GetComponent<MeshRenderer>().material.mainTextureOffset = Vector2.right - offset;
            tile.transform.localPosition = new Vector3(tiles[i].x, tiles[i].y);
            entityTiles[i] = tile; 
        }
    }
}
