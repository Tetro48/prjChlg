using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class GhostPieceController : MonoBehaviour
{
    public NetworkBoard networkBoard;
    public GameObject tileset;
    public GameObject[] tiles;
    public GameObject[] textureReading;
    public bool visibility;
    SpriteRenderer sprRnd;

    public void Initiate(PieceController connector)
    {
        networkBoard = connector.board;
        if (networkBoard.level < networkBoard.sectionSize || networkBoard.TLS) visibility = true;
        else 
        {
            Destroy(gameObject);
            return;
        }
        tiles = new GameObject[connector.tiles.Length];
        textureReading = new GameObject[connector.tiles.Length];
        for (int i = 0; i < connector.tiles.Length; i++)
        {
            textureReading[i] = connector.tiles[i];
            tiles[i] = Instantiate(textureReading[i], transform);
        }
        float transparency = 0.3f;
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].GetComponent<MeshRenderer>().material.color = new Color(1f, 1f, 1f, transparency);
        }
    }

    /// <summary>
    /// Checks to see if the tile can be moved to the specified positon.
    /// </summary>
    /// <param name="endPos">Coordinates of the position you are trying to move the tile to</param>
    /// <returns>True if the tile can be moved there. False if the tile cannot be moved there</returns>
    public bool CanGhostTileMove(int2 endPos)
    {
        if (!networkBoard.boardController.IsInBounds(endPos))
        {
            return false;
        }
        if (!networkBoard.boardController.IsPosEmpty(endPos))
        {
            return false;
        }
        return true;
    }
    public bool CanMoveGhostPiece(int2 movement)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if(textureReading[i] != null)
            if (!CanGhostTileMove(movement + new int2((int)textureReading[i].transform.localPosition.x, (int)tiles[i].transform.localPosition.y)))
            {
                return false;
            }
        }
        return true;
    }
    public void MoveGhostPiece(int2 movement)
    {
        while (CanMoveGhostPiece(movement) && gameObject.activeInHierarchy)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                tiles[i].transform.localPosition += new Vector3(movement.x, movement.y, 0f);
            }
        }
    }
    public void UpdateGhostPiece()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if(tiles[i] != null)
            {
                // tiles[i].GetComponent<SpriteRenderer>().sprite = textureReading[i].GetComponent<SpriteRenderer>().sprite;
                tiles[i].transform.localPosition = textureReading[i].transform.localPosition;
            }
        }
        MoveGhostPiece(new int2(0,-1));
    }
    
    // Update is called once per frame
    void Update()
    {
        if (networkBoard.level >= networkBoard.sectionSize && !networkBoard.TLS) visibility = false;
        if (networkBoard.piecesController.piecemovementlocked || !visibility)
        {
            Destroy(gameObject);
        }
        // this.transform.position = textureReading[0].transform.position;
    }
}
