using System.Collections;
using System.Collections.Generic;
using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;

/*
    Project Challenger, an challenging Tetris game.
    Copyright (C) 2022, Aymir

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

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
