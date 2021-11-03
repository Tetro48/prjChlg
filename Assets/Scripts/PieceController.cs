using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
    Project Challenger, an challenging Tetris game.
    Copyright (C) 2021, Aymir

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

public enum PieceType { O, I, S, Z, L, J, T }

public class PieceController : MonoBehaviour {

    public PieceType curType;
    public Sprite[] tileSprites;

    public NetworkBoard board;

    public int rotationIndex { get; private set; }
    public int setDelayTime;
    public GhostPieceController ghostContr;
    public bool harddrop;
    public bool zombieContr;
    public int LockDelayf;
    public bool isPieceIsInNextQueue;
    public int hideTilesPerUpdates;


    public Transform pivot;
    public GameObject[] tiles;
    [SerializeField]
    GameObject tileBlock;
    public Material[] materials;
    [SerializeField]int[] numberToTextureIDs;
    [SerializeField] Vector2Int spawnLocation;

    [SerializeField] bool fullyLocked;
    int textureRelation;
    /// <summary>
    /// Called as soon as the piece is initialized. Initializes some necessary values.
    /// </summary>
    private void Initiate(Vector2Int[] positions, GameObject obj, Vector2Int scaling, int textureSelect, Vector2Int nextPiecePosition)
    {
        rotationIndex = 0;
        int textureID = numberToTextureIDs[textureSelect];

        tiles = new GameObject[positions.Length];
        materials = new Material[positions.Length];
        Vector2 offset = new Vector2();
        offset = Vector2.right;
        offset -= new Vector2(-(float)(textureID % scaling.x) / scaling.x, (float)Math.Floor((double)textureID/4+1) / scaling.y);
        if(GameEngine.debugMode) Debug.Log(new Vector2((float)(textureID % scaling.x) / scaling.x, (float)Math.Floor((double)textureID/4+1) / scaling.y));
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject tile = Instantiate(tileBlock, transform);
            tile.transform.localPosition = (Vector2)(positions[i] + spawnLocation);
            tile.transform.localRotation = Quaternion.Euler(0,-90,0);
            TileController tc = tile.GetComponent<TileController>();
            tc.pieceController = this;
            tc.tileIndex = i;
            tc.textureID = textureSelect;
            tiles[i] = tile;
            materials[i] = tiles[i].GetComponent<MeshRenderer>().material;
            materials[i].mainTextureScale = Vector2.one / scaling;
            materials[i].mainTextureOffset = offset;
        }
        if(zombieContr) fullyLocked = true;
        // for(int i = 1; i <= tiles.Length; i++)
        // {
        //     if(!zombieContr || i == 1)
        //     {
        //         string tileName = "Tile" + i;
        //         TileController newTile = transform.Find("Tiles").Find(tileName).GetComponent<TileController>();
        //         tiles[i - 1] = newTile;
        //     }
        // } 
    }

    //Transitioning to dynamic timing
    void Update()
    {
        if(!isPieceIsInNextQueue)
        {
            if(board.framestepped)
            {
                hideTilesPerUpdates = board.tileInvisTime;
                if (hideTilesPerUpdates > 0 && fullyLocked)
                {
                    float percentage = 1f/hideTilesPerUpdates;
                    for (int i = 0; i < tiles.Length; i++)
                    {
                        if(tiles[i] != null)materials[i].color -= new Color(0f,0f,0f, percentage * Time.deltaTime / Time.fixedDeltaTime);
                    }
                }
                if (!board.LockDelayEnable && !board.piecesController.piecemovementlocked)  
                {
                    if(!CanMovePiece(Vector2Int.down) && !fullyLocked)  
                    {
                        board.LockDelayf = 0;  board.LockDelayEnable = true;
                    }
                    else board.LockDelayEnable = false;
                }
            
                if(board.LockDelayEnable && !harddrop && !fullyLocked)
                {
                    if(board.LockDelayf == 0 && board.LockDelay > 4)
                    {
                        AudioManager.PlayClip(board.audioPieceStep);
                    }
                    board.LockDelayf += Time.deltaTime / Time.fixedDeltaTime;
                    if (board.LockDelayf >= board.LockDelay)
                    {
                        board.LockDelayEnable = false;
                        SetPiece();
                    }
                }
            }
        }
    }
    // private void InitiateLockDelay()
    // {
    //     if(LockDelayEnable == false && board.piecesController.piecemovementlocked == false)  {LockDelayf = 0;  LockDelayEnable = true;}
    // }

    /// <summary>
    /// Moves the attached tiles to form the Tetris piece specified. Also sets the correct color of tile sprite.
    /// </summary>
    /// <param name="newType">Type of tetris piece to be spawned.</param>
    public void SpawnPiece(PieceType newType, PiecesController connector, Vector2Int[] positions, Vector2 setPivot, GameObject obj, Vector2Int scaling, int textureSelect, Vector2Int nextPos)
    {
        pivot.localPosition = setPivot + nextPos;
        int increaseByLevel = board.level >= 600 && board.nextibmblocks == board.nextPieces + 1 ? 14 : 0;
        int RSint = board.RS == RotationSystems.ARS ? 7 : 0;
        int combine = (increaseByLevel + RSint);
        spawnLocation = nextPos;
        board = connector.board;
        Initiate(positions, obj, scaling, textureSelect + combine, nextPos);
        ghostContr.Initiate(this);
        curType = newType;
        textureRelation = combine + textureSelect;
        // UpdatePosition(tiles[0], spawnLocation);

        switch (curType)
        {
            // case PieceType.I:
            //     tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
            //     tiles[2].UpdatePosition(spawnLocation + (Vector2Int.right * 2));
            //     tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
            //     SetTileSprites(tileSprites[0 + result]);
            //     break;

            // case PieceType.J:
            //     tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
            //     tiles[2].UpdatePosition(spawnLocation + new Vector2Int(-1, 1));
            //     tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
            //     SetTileSprites(tileSprites[1 + result]);
            //     break;

            // case PieceType.L:
            //     tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
            //     tiles[2].UpdatePosition(spawnLocation + new Vector2Int(1, 1));
            //     tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
            //     SetTileSprites(tileSprites[2 + result]);
            //     break;

            // case PieceType.O:
            //     tiles[1].UpdatePosition(spawnLocation + Vector2Int.right);
            //     tiles[2].UpdatePosition(spawnLocation + new Vector2Int(1, 1));
            //     tiles[3].UpdatePosition(spawnLocation + Vector2Int.up);
            //     SetTileSprites(tileSprites[3 + result]);
            //     break;

            // case PieceType.S:
            //     tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
            //     tiles[2].UpdatePosition(spawnLocation + new Vector2Int(1, 1));
            //     tiles[3].UpdatePosition(spawnLocation + Vector2Int.up);
            //     SetTileSprites(tileSprites[4 + result]);
            //     break;

            // case PieceType.T:
            //     tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
            //     tiles[2].UpdatePosition(spawnLocation + Vector2Int.up);
            //     tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
            //     SetTileSprites(tileSprites[5 + result]);
            //     break;

            // case PieceType.Z:
            //     tiles[1].UpdatePosition(spawnLocation + Vector2Int.up);
            //     tiles[2].UpdatePosition(spawnLocation + new Vector2Int(-1, 1));
            //     tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
            //     SetTileSprites(tileSprites[6 + result]);
            //     break;

            default:

                break;
        }

        // int index = 0;
        // foreach(TileController ti in tiles)
        // {
        //     ti.InitializeTile(this, index);
        //     index++;
        // }
    }

    // /// <summary>
    // /// Gets the coordinates of all active tiles attached to this piece.
    // /// </summary>
    // /// <returns>Returns array of coordinates for currently active tiles on the piece.</returns>
    // public Vector2Int[] GetTileCoords()
    // {
    //     List<Vector2Int> curTileCoords = new List<Vector2Int>();

    //     for (int i = 0; i < tiles.Length; i++)
    //     {
    //         if (tiles[i] == null)
    //         {
    //             continue;
    //         }
    //         curTileCoords.Add(tiles[i].coordinates);
    //     }
    //     curTileCoords = curTileCoords.OrderBy(x => x.x).ThenByDescending(x => x.y).ToList();
    //     foreach(Vector2Int v2i in curTileCoords)
    //     {
    //         if(GameEngine.debugMode) Debug.Log("CurtIle is " + v2i.ToString());
    //     }
    //     Vector2Int[] curCoords = curTileCoords.ToArray();
    //     return curCoords;
    // }

    /// <summary>
    /// Checks if the piece is able to be moved by the specified amount. A piece cannot be moved if there
    /// is already another piece there or the piece would end up out of bounds.
    /// </summary>
    /// <param name="movement">X,Y amount to move the piece</param>
    /// <returns></returns>
    public bool CanMovePiece(Vector2Int movement)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if(tiles[i] != null) if (!CanTileMove(movement + V3ToV2Int(tiles[i].transform.localPosition)))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Checks to see if there are any tiles left for the given piece.
    /// </summary>
    /// <returns>True if there are still tiles left. False if the piece has no remaining tiles.</returns>
    public bool AnyTilesLeft()
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            if(tiles[i] != null)
            {
                return true;
            }
        }
        if(GameEngine.debugMode) Debug.Log("no tiles left");
        board.piecesController.RemovePiece(gameObject);
        return false;
    }

    /// <summary>
    /// Moves the piece by the specified amount.
    /// </summary>
    /// <param name="movement">X,Y amount to move the piece</param>
    /// <returns>True if the piece was able to be moved. False if the move couln't be completed.</returns>
    public void ForcefullyMovePiece(Vector2Int movement)
    {
        UnisonPieceMove(movement);
        board.LockDelayf = 0;
    }
    /// <summary>
    /// Moves the piece by the specified amount.
    /// </summary>
    /// <param name="movement">X,Y amount to move the piece</param>
    /// <returns>True if the piece was able to be moved. False if the move couln't be completed.</returns>
    public bool MovePiece(Vector2Int movement, bool offset)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            if (!CanTileMove(movement + V3ToV2Int(tiles[i].transform.localPosition)))
            {
                // Debug.Log("Cant Go there!");
                if(movement == Vector2Int.down && harddrop == true)
                {
                    SetPiece();
                }
                return false;
            }
        }

        UnisonPieceMove(movement);
        board.LockDelayf = 0;
        if(movement.y >= 0) if(!offset) if(board.LockDelay > 5 || board.gravity < 19) AudioManager.PlayClip(board.moveSE);
        if(!CanMovePiece(Vector2Int.down))board.countLockResets++;
        if(board.countLockResets >= board.maxLockResets)
        {
            board.LockDelayf = board.LockDelay;
        }
        if (!CanMovePiece(Vector2Int.down) && fullyLocked == false)  
        {
            if(board.LockDelayEnable == false && board.piecesController.piecemovementlocked == false)  
            {
                board.LockDelayf = 0;  board.LockDelayEnable = true;
            }
        }
        else board.LockDelayEnable = false;

        if (board.level < board.sectionSize || board.TLS) ghostContr.UpdateGhostPiece();
        return true;
    }
    public void UnisonPieceMove(Vector2Int movement)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            MoveTile(tiles[i].gameObject, movement);
        }
        pivot.localPosition += (Vector3)(Vector2)movement;
    }

    public bool isPieceLocked(){return fullyLocked;}

    /// <summary>
    /// Rotates the piece by 90 degrees in specified direction. Offest operations should almost always be attempted,
    /// unless you are rotating the piece back to its original position.
    /// </summary>
    /// <param name="clockwise">Set to true if rotating clockwise. Set to False if rotating CCW</param>
    /// <param name="shouldOffset">Set to true if offset operations should be attempted.</param>
    /// <param name="UD">Set to true if rotating 180 degrees.</param>
    public void RotatePiece(bool clockwise, bool shouldOffset, bool UD, bool firstAttempt = true)
    {
        int oldRotationIndex = rotationIndex;
        rotationIndex += clockwise ? 1 : -1;
        if (UD)rotationIndex += clockwise ? 1 : -1;

        rotationIndex = Mod(rotationIndex, 4);
        // if (GameEngine.instance.RS == RotationSystems.ARS && (curType == PieceType.S || curType == PieceType.Z))
        // {
        //     rotationIndex = Mod(rotationIndex, 2);
        // }

        board.tSpin = (curType == PieceType.T && board.LockDelayEnable);

        for(int i = 0; i < tiles.Length; i++)
        {
            RotateObject(tiles[i].gameObject, pivot.position, clockwise);
        }
        if (UD)
        {
            RotatePiece180(clockwise, true, firstAttempt);
        }

        if (!shouldOffset)
        {
            return;
        }

        bool canOffset = Offset(oldRotationIndex, rotationIndex);
        if(UD == true) canOffset = Offset180(oldRotationIndex, rotationIndex);

        if (!canOffset)
        {
            RotatePiece(!clockwise, /*rotIndex == 2 ? true :*/ false, UD, false);
        }
        if (board.level < board.sectionSize || board.TLS) ghostContr.UpdateGhostPiece();
    }
    public void RotatePiece180(bool clockwise, bool shouldOffset, bool firstAttempt = true)
    {
        int oldRotationIndex = Mod(rotationIndex - 2, 4);
        // rotationIndex += clockwise ? 1 : -1;
        // rotationIndex = Mod(rotationIndex, 4);
        // if (GameEngine.instance.RS == RotationSystems.ARS && (curType == PieceType.S || curType == PieceType.Z))
        // {
        //     rotationIndex = Mod(rotationIndex, 2);
        // }

        // for(int i = 0; i < tiles.Length; i++)
        // {
        //     RotateObject(tiles[i].gameObject, pivot.position, clockwise);
        // }

        if (!shouldOffset)
        {
            for(int i = 0; i < tiles.Length; i++)
            {
                RotateObject(tiles[i].gameObject, pivot.position, clockwise);
            }
            return;
        }

        bool canOffset = Offset(oldRotationIndex, rotationIndex);

        if (!canOffset && firstAttempt)
        {
            RotatePiece(!clockwise, /*rotIndex == 2 ? true :*/ false, true);
            if(GameEngine.debugMode) Debug.Log("Couldn't apply 180 offset");
        }
        if (board.level < board.sectionSize || board.TLS) ghostContr.UpdateGhostPiece();
    }

    /// <summary>
    /// True modulo operation that works for positive and negative values.
    /// </summary>
    /// <param name="x">The dividend</param>
    /// <param name="m">The divisor</param>
    /// <returns>Returns the remainder of x divided by m</returns>
    int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
    /// <summary>
    /// Performs 5 tests on the piece to find a valid final location for the piece.
    /// </summary>
    /// <param name="oldRotIndex">Original rotation index of the piece</param>
    /// <param name="newRotIndex">Rotation index the piece will be rotating to</param>
    /// <returns>True if one of the tests passed and a final location was found. False if all test failed.</returns>
    bool Offset180(int oldRotIndex, int newRotIndex)
    {
        Vector2Int[,] curOffsetData;
        
        if(curType == PieceType.O)
        {
            curOffsetData = board.piecesController.O_OFFSET_DATA;
        }
        else if(curType == PieceType.I)
        {
            curOffsetData = board.piecesController.I_OFFSET_DATA;
        }
        else
        {
            curOffsetData = board.piecesController.JLSTZ_OFFSET_DATA;
        }


        bool movePossible = false;

        for(int i = 0; i < tiles.Length; i++)
        {
            RotateObject(tiles[i].gameObject, pivot.position, oldRotIndex < newRotIndex ? true : false);
        }
        if (CanMovePiece(Vector2Int.zero))
        {
            movePossible = true;
        }
        if(board.LockDelay > 6 || board.gravity < 19) AudioManager.PlayClip(board.rotateSE);
        // else
        // {
        //     Debug.Log("Move impossible");
        // }
        return movePossible;
    }

    /// <summary>
    /// Performs 5 tests on the piece to find a valid final location for the piece.
    /// </summary>
    /// <param name="oldRotIndex">Original rotation index of the piece</param>
    /// <param name="newRotIndex">Rotation index the piece will be rotating to</param>
    /// <returns>True if one of the tests passed and a final location was found. False if all test failed.</returns>
    bool Offset(int oldRotIndex, int newRotIndex)
    {
        Vector2Int offsetVal1, offsetVal2, endOffset;
        Vector2Int[,] curOffsetData;
        
        if(curType == PieceType.O)
        {
            curOffsetData = board.piecesController.O_OFFSET_DATA;
        }
        else if(curType == PieceType.I)
        {
            curOffsetData = board.piecesController.I_OFFSET_DATA;
        }
        else
        {
            curOffsetData = board.piecesController.JLSTZ_OFFSET_DATA;
        }

        endOffset = Vector2Int.zero;

        bool movePossible = false;

        for (int testIndex = 0; testIndex < 5; testIndex++)
        {
            if(testIndex == 0)
            {
                for(int i = 0; i < tiles.Length; i++)
                {
                    RotateObject(tiles[i].gameObject, pivot.position, oldRotIndex < newRotIndex ? true : false);
                }
                if (CanMovePiece(endOffset))
                {
                    movePossible = true;
                    break;
                }
            }
            else
            {
                offsetVal1 = curOffsetData[testIndex, oldRotIndex];
                offsetVal2 = curOffsetData[testIndex, newRotIndex];
                endOffset = offsetVal1 - offsetVal2;
                if (CanMovePiece(endOffset))
                {
                    movePossible = true;
                    break;
                }
            }
        }

        if (movePossible)
        {
            MovePiece(endOffset, true);
        }
        if(board.LockDelay > 6 || board.gravity < 19) AudioManager.PlayClip(board.rotateSE);
        // else
        // {
        //     Debug.Log("Move impossible");
        // }
        return movePossible;
    }

    /// <summary>
    /// Sets the piece in its permanent location.
    /// </summary>
    public void SetPiece()
    {
        AudioManager.PlayClip(board.audioPieceLock);
        board.piecesController.piecemovementlocked = true;
        board.piecesController.lockedPieces++;
        fullyLocked = true;
        board.countLockResets = 0;
        for(int i = 0; i < tiles.Length; i++)
        {
            if (!board.boardController.SetTile(tiles[i].gameObject))
            {
                if(GameEngine.debugMode) Debug.Log("GAME OVER!");
                board.GameOver = true;
                board.piecesController.GameOver();
            }
        }
        if (board.GameOver == false)
        {
            if (Input.GetKey(KeyCode.E) && GameEngine.debugMode)
            {
                int incrementbyfrozenlines = board.lineFreezingMechanic ? board.linesFrozen[board.curSect] : 0;
                board.boardController.FillLine(0+incrementbyfrozenlines);
                board.boardController.FillLine(1+incrementbyfrozenlines);
                board.boardController.FillLine(2+incrementbyfrozenlines);
                board.boardController.FillLine(3+incrementbyfrozenlines);
            }
            board.boardController.CheckLineClears();
            board.piecesController.UpdatePieceBag();
        }
    }
    
    /// <summary>
    /// Rotates the tile by 90 degrees about the origin tile.
    /// </summary>
    /// <param name="originPos">Coordinates this tile will be rotating about.</param>
    /// <param name="clockwise">True if rotating clockwise. False if rotatitng CCW</param>
    public static void RotateObject(GameObject obj, Vector3 pivotPos, bool clockwise)
    {
        // Vector2Int relativePos = V3ToV2Int(obj.transform.localPosition) - originPos;
        // Vector2Int[] rotMatrix = clockwise ? new Vector2Int[2] { new Vector2Int(0, -1), new Vector2Int(1, 0) }
        //                                    : new Vector2Int[2] { new Vector2Int(0, 1), new Vector2Int(-1, 0) };
        // int newXPos = (rotMatrix[0].x * relativePos.x) + (rotMatrix[1].x * relativePos.y);
        // int newYPos = (rotMatrix[0].y * relativePos.x) + (rotMatrix[1].y * relativePos.y);
        // Vector2Int newPos = new Vector2Int(newXPos, newYPos);

        // newPos += originPos;
        // UpdatePosition(obj, newPos);
        int multi = clockwise ? 1 : -1;
        obj.transform.RotateAround(pivotPos, Vector3.forward, 90 * multi);
        // obj.transform.Rotate(new Vector3(90f * -multi, 0f, 0f), Space.Self);
    }
    public static void UpdatePosition(GameObject obj, Vector2Int newPos)
    {
        Vector3 newV3Pos = new Vector3(newPos.x, newPos.y);
        obj.transform.localPosition = newV3Pos;
    }

    /// <summary>
    /// Moves the tile by the specified amount
    /// </summary>
    /// <param name="movement">X,Y amount the tile will be moved by</param>
    public static void MoveTile(GameObject obj, Vector2Int movement)
    {
        Vector2Int endPos = V3ToV2Int(localObjPos(obj)) + movement;
        UpdatePosition(obj, endPos);
    }
    /// <summary>
    /// Checks to see if the tile can be moved to the specified positon.
    /// </summary>
    /// <param name="endPos">Coordinates of the position you are trying to move the tile to</param>
    /// <returns>True if the tile can be moved there. False if the tile cannot be moved there</returns>
    public bool CanTileMove(Vector2Int endPos)
    {
        if (!board.boardController.IsInBounds(endPos))
        {
            return false;
        }
        if (!board.boardController.IsPosEmpty(endPos))
        {
            return false;
        }
        return true;
    }
    static Vector3 localObjPos(GameObject obj)
    {
        return obj.transform.localPosition;
    }
    static Vector2Int V3ToV2Int(Vector3 vector3)
    {
        return new Vector2Int((int)vector3.x, (int)vector3.y);
    }
    int maxNumberComparison(int num1, int num2, bool flipIfNeg = false)
    {
        int output;
        if(num1 > num2) output = num1;
        else output = num2;
        if(output < 0 && flipIfNeg) output *= -1;
        return output;
    }

    /// <summary>
    /// Drops piece down as far as it can go.
    /// </summary>
    public void SendPieceToFloor()
    {
        harddrop = true;
        board.piecesController.PrevInputs[0] = true;
        AudioManager.PlayClip(board.hardDropSE);
        while (MovePiece(Vector2Int.down, true)) {}
    }
}
