﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostPieceController : MonoBehaviour
{
    public GameObject tileset;
    public GameObject[] tiles;
    public GameObject[] textureReading;
    public bool setOn;
    public bool visibility;
    SpriteRenderer sprRnd;

    void Awake()
    {
        sprRnd = GetComponent<SpriteRenderer>();
        if (GameEngine.instance.level < 100 || GameEngine.instance.TLS) visibility = true;
        float transparency = 0.3f;
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].GetComponent<SpriteRenderer>().color = new Color(1f,1f,1f,transparency);
        }
    }

    /// <summary>
    /// Checks to see if the tile can be moved to the specified positon.
    /// </summary>
    /// <param name="endPos">Coordinates of the position you are trying to move the tile to</param>
    /// <returns>True if the tile can be moved there. False if the tile cannot be moved there</returns>
    public bool CanGhostTileMove(Vector2Int endPos)
    {
        if (!BoardController.instance.IsInBounds(endPos))
        {
            return false;
        }
        if (!BoardController.instance.IsPosEmpty(endPos))
        {
            return false;
        }
        return true;
    }
    public bool CanMoveGhostPiece(Vector2Int movement)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (!CanGhostTileMove(movement + new Vector2Int(textureReading[i].GetComponent<TileController>().coordinates.x, (int)tiles[i].transform.position.y)))
            {
                return false;
            }
        }
        return true;
    }
    public void MoveGhostPiece(Vector2Int movement)
    {
        setOn = false;
        while (CanMoveGhostPiece(movement) && !setOn)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].transform.position += new Vector3(movement.x, movement.y, 0f);
                if (!CanMoveGhostPiece(Vector2Int.down)) setOn = true;
            }
        }
    }
    public void UpdateGhostPiece()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].GetComponent<SpriteRenderer>().sprite = textureReading[i].GetComponent<SpriteRenderer>().sprite;
            tiles[i].transform.position = textureReading[i].transform.position;
        }
        MoveGhostPiece(Vector2Int.down);
    }
    
    // Update is called once per frame
    void Update()
    {
        if (PiecesController.instance.piecemovementlocked || !visibility)
        {
            Destroy(this.gameObject);
        }
        // this.transform.position = textureReading[0].transform.position;
    }
}
