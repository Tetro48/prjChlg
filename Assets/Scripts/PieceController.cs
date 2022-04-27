using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    public int LockTicks;
    public bool isPieceIsInNextQueue;
    public int hideTilesPerUpdates;


    public float2 pivot;
    public GameObject[] tiles;
    public TileController[] tileContainers;
    [SerializeField]
    GameObject tileBlock;
    public Material[] materials;
    [SerializeField]int[] numberToTextureIDs;
    [SerializeField] int2 spawnLocation;

    [SerializeField] bool fullyLocked;
    int textureRelation;
    /// <summary>
    /// Called as soon as the piece is initialized. Initializes some necessary values.
    /// </summary>
    private void Initiate(int2[] positions, GameObject obj, float2 scaling, int textureSelect, int2 nextPiecePosition)
    {
        rotationIndex = 0;
        int textureID = numberToTextureIDs[textureSelect];

        tiles = new GameObject[positions.Length];
        tileContainers = new TileController[positions.Length];
        materials = new Material[positions.Length];
        Vector2 offset = new Vector2();
        offset = Vector2.right;
        offset -= new Vector2(-(float)(textureID % scaling.x) / scaling.x, (float)Math.Floor((double)textureID/4+1) / scaling.y);
        if(GameEngine.debugMode) Debug.Log(new Vector2((float)(textureID % scaling.x) / scaling.x, (float)Math.Floor((double)textureID/4+1) / scaling.y));
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject tile = Instantiate(tileBlock, transform);
            tile.transform.localPosition = (Vector2)int2ToV2Int(positions[i] + spawnLocation);
            tile.transform.localRotation = Quaternion.Euler(0,-90,0);
            tiles[i] = tile;
            materials[i] = tiles[i].GetComponent<MeshRenderer>().material;
            materials[i].mainTextureScale = new float2(1,1) / scaling;
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
    void FixedUpdate()
    {
        // if(!isPieceIsInNextQueue)
        // {
        //     if(board.framestepped)
        //     {
        //         // hideTilesPerUpdates = board.tileInvisTime;
        //         // if (hideTilesPerUpdates > 0 && fullyLocked)
        //         // {
        //         //     float percentage = 1f/hideTilesPerUpdates;
        //         //     for (int i = 0; i < tiles.Length; i++)
        //         //     {
        //         //         if(tiles[i] != null)materials[i].color -= new Color(0f,0f,0f, percentage * Time.deltaTime / Time.fixedDeltaTime);
        //         //     }
        //         // }
        //         if (!board.LockDelayEnable && !board.piecesController.piecemovementlocked)  
        //         {
        //             if(!CanMovePiece(new int2(0,-1)) && !fullyLocked)  
        //             {
        //                 board.LockTicks = 0;  board.LockDelayEnable = true;
        //             }
        //             else board.LockDelayEnable = false;
        //         }
            
        //         if(board.LockDelayEnable && !harddrop && !fullyLocked)
        //         {
        //             if(board.LockTicks == 0 && board.LockDelay > 4)
        //             {
        //                 AudioManager.PlayClip(board.audioPieceStep);
        //             }
        //             board.LockTicks += Time.deltaTime / Time.fixedDeltaTime;
        //             if (board.LockTicks >= board.LockDelay)
        //             {
        //                 board.LockDelayEnable = false;
        //                 SetPiece();
        //             }
        //         }
        //     }
        // }
    }
    // private void InitiateLockDelay()
    // {
    //     if(LockDelayEnable == false && board.piecesController.piecemovementlocked == false)  {LockTicks = 0;  LockDelayEnable = true;}
    // }

    /// <summary>
    /// Moves the attached tiles to form the Tetris piece specified. Also sets the correct color of tile sprite.
    /// </summary>
    /// <param name="newType">Type of tetris piece to be spawned.</param>
    public void SpawnPiece(PieceType newType, int2[] positions, float2 setPivot, GameObject obj, int2 scaling, int textureSelect, int2 nextPos)
    {
        pivot = setPivot + nextPos;
        // int increaseByLevel = board.level >= 600 && board.nextibmblocks == board.nextPieces + 1 ? 14 : 0;
        // int RSint = board.RS == RotationSystems.ARS ? 7 : 0;
        // int combine = (increaseByLevel + RSint);
        spawnLocation = nextPos;
        Initiate(positions, obj, scaling, textureSelect, nextPos);
        curType = newType;
        textureRelation = textureSelect;
        // UpdatePosition(tiles[0], spawnLocation);
    }

    /// <summary>
    /// Checks if the piece is able to be moved by the specified amount. A piece cannot be moved if there
    /// is already another piece there or the piece would end up out of bounds.
    /// </summary>
    /// <param name="movement">X,Y amount to move the piece</param>
    /// <returns></returns>
    public bool CanMovePiece(int2 movement)
    {
        for (int i = 0; i < tileContainers.Length; i++)
        {
            if (!CanTileMove(movement + tileContainers[i].position))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Moves the piece by the specified amount forcefully.
    /// </summary>
    /// <param name="movement">X,Y amount to move the piece</param>
    public void ForcefullyMovePiece(int2 movement)
    {
        UnisonPieceMove(movement);
        board.LockTicks = 0;
    }
    /// <summary>
    /// Moves the piece by the specified amount.
    /// </summary>
    /// <param name="movement">X,Y amount to move the piece</param>
    /// <returns>True if the piece was able to be moved. False if the move couln't be completed.</returns>
    public bool MovePiece(int2 movement, bool offset)
    {
        for (int i = 0; i < tileContainers.Length; i++)
        {
            if (!CanTileMove(movement + tileContainers[i].position))
            {
                // Debug.Log("Cant Go there!");
                if(int2ToV2Int(movement) == Vector2Int.down && harddrop == true)
                {
                    SetPiece();
                }
                return false;
            }
        }

        UnisonPieceMove(movement);
        board.LockTicks = 0;
        if(movement.y >= 0) if(!offset) if(board.LockDelay > 5 || board.gravity < 19) AudioManager.PlayClip(board.moveSE);
        if(!CanMovePiece(new int2(0,-1)))board.countLockResets++;
        if(board.countLockResets >= board.maxLockResets)
        {
            board.LockTicks = board.LockDelay;
        }
        if (!CanMovePiece(new int2(0,-1)) && fullyLocked == false)  
        {
            if(board.LockDelayEnable == false && board.piecesController.piecemovementlocked == false)  
            {
                board.LockTicks = 0;  board.LockDelayEnable = true;
            }
        }
        else board.LockDelayEnable = false;

        return true;
    }
    public void UnisonPieceMove(int2 movement)
    {
        for (int i = 0; i < tileContainers.Length; i++)
        {
            tileContainers[i].position = MoveTile(tileContainers[i].position, movement);
        }
        pivot += movement;
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

        // if (UD)
        // {
        //     RotatePiece180(clockwise, true, firstAttempt);
        // }

        if (!shouldOffset)
        {
            int2[,] curOffsetData;
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
            int2 offsetVal1, offsetVal2, endOffset;
            offsetVal1 = curOffsetData[0, oldRotationIndex];
            offsetVal2 = curOffsetData[0, rotationIndex];
            endOffset = offsetVal1 - offsetVal2;
            RotateInUnison(clockwise, UD);
        }

        bool canOffset = Offset(oldRotationIndex, rotationIndex, clockwise, UD);

        if (!canOffset)
        {
            rotationIndex = oldRotationIndex;
        }
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
    bool Offset(int oldRotIndex, int newRotIndex, bool clockwise, bool UD = false)
    {
        int2 offsetVal1, offsetVal2, endOffset;
        int2[,] curOffsetData;
        
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

        endOffset = int2.zero;

        bool movePossible = false;

        for (int testIndex = 0; testIndex < 5; testIndex++)
        {
            offsetVal1 = curOffsetData[testIndex, oldRotIndex];
            offsetVal2 = curOffsetData[testIndex, newRotIndex];
            endOffset = offsetVal1 - offsetVal2;
            if(board.bigMode) endOffset *= 2;
            if(testIndex == 0)
            {
                RotateInUnison(clockwise, UD);
            }
            if (CanMovePiece(endOffset))
            {
                movePossible = true;
                break;
            }
        }

        if (movePossible)
        {
            MovePiece(endOffset, true);
        }
        else RotateInUnison(!clockwise, UD);
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
        for(int i = 0; i < tileContainers.Length; i++)
        {
            if (!board.boardController.SetTile(tileContainers[i].position.xyx))
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
    public static int2 RotateObject(GameObject obj, int2 tilePos, Vector2 pivotPos, bool clockwise, bool UD = false)
    {
        // int2 relativePos = V3ToInt2(obj.transform.localPosition) - originPos;
        // int2[] rotMatrix = clockwise ? new int2[2] { new int2(0, -1), new int2(1, 0) }
        //                                    : new int2[2] { new int2(0, 1), new int2(-1, 0) };
        // int newXPos = (rotMatrix[0].x * relativePos.x) + (rotMatrix[1].x * relativePos.y);
        // int newYPos = (rotMatrix[0].y * relativePos.x) + (rotMatrix[1].y * relativePos.y);
        // int2 newPos = new int2(newXPos, newYPos);

        // newPos += originPos;
        // UpdatePosition(obj, newPos);
        int multi = clockwise ? 1 : -1;
        if(UD) multi *= 2;
        obj.transform.position = (Vector2)int2ToV2Int(tilePos);
        obj.transform.RotateAround(pivotPos, Vector3.forward, -90 * multi);
        obj.transform.Rotate(new Vector3(90f * multi, 0f, 0f), Space.Self);
        return (int2)(float2)(Vector2)obj.transform.position;
    }
    public bool RotateInUnison(bool clockwise, bool UD = false)
    {
        for (int i = 0; i < tileContainers.Length; i++)
        {
            tileContainers[i].position = RotateObject(tiles[i], tileContainers[i].position, pivot, clockwise, UD);
        }
        return CanMovePiece(int2.zero);
    }

    /// <summary>
    /// Moves the tile by the specified amount
    /// </summary>
    /// <param name="movement">X,Y amount the tile will be moved by</param>
    public static int2 MoveTile(int2 position, int2 movement)
    {
        return position + movement;
    }
    /// <summary>
    /// Checks to see if the tile can be moved to the specified positon.
    /// </summary>
    /// <param name="endPos">Coordinates of the position you are trying to move the tile to</param>
    /// <returns>True if the tile can be moved there. False if the tile cannot be moved there</returns>
    public bool CanTileMove(int2 endPos)
    {
        return board.boardController.IsPosEmpty(endPos) && board.boardController.IsInBounds(endPos);
    }
    static Vector3 localObjPos(GameObject obj)
    {
        return obj.transform.localPosition;
    }
    static int2 V3ToInt2(Vector3 vector3)
    {
        return new int2(Mathf.FloorToInt(vector3.x + 0.5f), Mathf.FloorToInt(vector3.y + 0.5f));
    }
    static Vector2Int int2ToV2Int(int2 integers)
    {return new Vector2Int(integers.x, integers.y);}
    static int maxNumberComparison(int num1, int num2, bool flipIfNeg = false)
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
        while (MovePiece(new int2(0,-1), true)) {}
    }
}
