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


    public TileController[] tiles;
    //Vector2Int spawnLocation = new Vector2Int(4, 21);
    Vector2Int spawnLocation;

    [SerializeField] bool fullyLocked;
    /// <summary>
    /// Called as soon as the piece is initialized. Initializes some necessary values.
    /// </summary>
    private void Initiate(Vector2Int position)
    {
        spawnLocation = position;
        rotationIndex = 0;

        tiles = new TileController[4];
        if(zombieContr) fullyLocked = true;
        for(int i = 1; i <= tiles.Length; i++)
        {
            if(!zombieContr || i == 1)
            {
                string tileName = "Tile" + i;
                TileController newTile = transform.Find("Tiles").Find(tileName).GetComponent<TileController>();
                tiles[i - 1] = newTile;
            }
        } 
    }

    private void FixedUpdate()
    {
        if(!isPieceIsInNextQueue)
        {
            hideTilesPerUpdates = board.tileInvisTime;
            if (hideTilesPerUpdates > 0 && board.framestepped && fullyLocked)
            {
                float percentage = 1f/hideTilesPerUpdates;
                for (int i = 0; i < tiles.Length; i++)
                {
                    if(tiles[i] != null)tiles[i].spriteRenderer.color -= new Color(0f,0f,0f, percentage);
                }
            }
            if(board.framestepped)
            {
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
                    board.LockDelayf++;
                    if (board.LockDelayf >= board.LockDelay)
                    {
                        board.LockDelayf = 0;
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
    public void SpawnPiece(PieceType newType, PiecesController connector, Vector2Int position)
    {
        board = connector.board;
        Initiate(position);
        ghostContr.Initiate(connector);
        curType = newType;
        int increaseByLevel = board.level >= 600 && board.nextibmblocks == board.nextPieces + 1 ? 14 : 0;
        int RSint = board.RS == RotationSystems.ARS ? 7 : 0;
        int combine = (increaseByLevel + RSint);
        int result = combine;
        tiles[0].UpdatePosition(spawnLocation);

        switch (curType)
        {
            case PieceType.I:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
                tiles[2].UpdatePosition(spawnLocation + (Vector2Int.right * 2));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
                SetTileSprites(tileSprites[0 + result]);
                break;

            case PieceType.J:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
                tiles[2].UpdatePosition(spawnLocation + new Vector2Int(-1, 1));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
                SetTileSprites(tileSprites[1 + result]);
                break;

            case PieceType.L:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
                tiles[2].UpdatePosition(spawnLocation + new Vector2Int(1, 1));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
                SetTileSprites(tileSprites[2 + result]);
                break;

            case PieceType.O:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.right);
                tiles[2].UpdatePosition(spawnLocation + new Vector2Int(1, 1));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.up);
                SetTileSprites(tileSprites[3 + result]);
                break;

            case PieceType.S:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
                tiles[2].UpdatePosition(spawnLocation + new Vector2Int(1, 1));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.up);
                SetTileSprites(tileSprites[4 + result]);
                break;

            case PieceType.T:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.left);
                tiles[2].UpdatePosition(spawnLocation + Vector2Int.up);
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
                SetTileSprites(tileSprites[5 + result]);
                break;

            case PieceType.Z:
                tiles[1].UpdatePosition(spawnLocation + Vector2Int.up);
                tiles[2].UpdatePosition(spawnLocation + new Vector2Int(-1, 1));
                tiles[3].UpdatePosition(spawnLocation + Vector2Int.right);
                SetTileSprites(tileSprites[6 + result]);
                break;

            default:

                break;
        }

        int index = 0;
        foreach(TileController ti in tiles)
        {
            ti.InitializeTile(this, index);
            index++;
        }
    }

    /// <summary>
    /// Gets the coordinates of all active tiles attached to this piece.
    /// </summary>
    /// <returns>Returns array of coordinates for currently active tiles on the piece.</returns>
    public Vector2Int[] GetTileCoords()
    {
        List<Vector2Int> curTileCoords = new List<Vector2Int>();

        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i] == null)
            {
                continue;
            }
            curTileCoords.Add(tiles[i].coordinates);
        }
        curTileCoords = curTileCoords.OrderBy(x => x.x).ThenByDescending(x => x.y).ToList();
        foreach(Vector2Int v2i in curTileCoords)
        {
            if(GameEngine.debugMode) Debug.Log("CurtIle is " + v2i.ToString());
        }
        Vector2Int[] curCoords = curTileCoords.ToArray();
        return curCoords;
    }

    /// <summary>
    /// Sets the sprites of all tiles on this piece
    /// </summary>
    /// <param name="newSpr">New sprite to set for this tile</param>
    public void SetTileSprites(Sprite newSpr)
    {
        for(int i = 0; i < tiles.Length; i++)
        {
            if(tiles[i] == null)
            {
                continue;
            }
            tiles[i].gameObject.GetComponent<SpriteRenderer>().sprite = newSpr;
        }
    }

    /// <summary>
    /// Checks if the piece is able to be moved by the specified amount. A piece cannot be moved if there
    /// is already another piece there or the piece would end up out of bounds.
    /// </summary>
    /// <param name="movement">X,Y amount to move the piece</param>
    /// <returns></returns>
    public bool CanMovePiece(Vector2Int movement)
    {
        foreach (TileController tile in tiles)
        {
            if(tile != null) if (!tile.CanTileMove(movement + tile.coordinates))
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
    public bool ForcefullyMovePiece(Vector2Int movement)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].MoveTile(movement);
            board.LockDelayf = 0;
        }
        return true;
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
            if (!tiles[i].CanTileMove(movement + tiles[i].coordinates))
            {
                // Debug.Log("Cant Go there!");
                if(movement == Vector2Int.down && harddrop == true)
                {
                    SetPiece();
                }
                return false;
            }
        }

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i].MoveTile(movement);
            board.LockDelayf = 0;
        }
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

    public bool isPieceLocked(){return fullyLocked;}

    /// <summary>
    /// Rotates the piece by 90 degrees in specified direction. Offest operations should almost always be attempted,
    /// unless you are rotating the piece back to its original position.
    /// </summary>
    /// <param name="clockwise">Set to true if rotating clockwise. Set to False if rotating CCW</param>
    /// <param name="shouldOffset">Set to true if offset operations should be attempted.</param>
    /// <param name="UD">Set to true if rotating 180 degrees.</param>
    public void RotatePiece(bool clockwise, bool shouldOffset, bool UD)
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
            tiles[i].RotateTile(tiles[0].coordinates, clockwise);
        }
        if (UD)
        {
            RotatePiece180(clockwise, true);
        }

        if (!shouldOffset)
        {
            return;
        }

        bool canOffset = Offset(oldRotationIndex, rotationIndex);
        if(UD == true) canOffset = Offset180(oldRotationIndex, rotationIndex);

        if (!canOffset)
        {
            RotatePiece(!clockwise, /*rotIndex == 2 ? true :*/ false, UD);
        }
        if (board.level < board.sectionSize || board.TLS) ghostContr.UpdateGhostPiece();
    }
    public void RotatePiece180(bool clockwise, bool shouldOffset)
    {
        int oldRotationIndex = Mod(rotationIndex - 2, 4);
        // rotationIndex += clockwise ? 1 : -1;
        // rotationIndex = Mod(rotationIndex, 4);
        // if (GameEngine.instance.RS == RotationSystems.ARS && (curType == PieceType.S || curType == PieceType.Z))
        // {
        //     rotationIndex = Mod(rotationIndex, 2);
        // }

        for(int i = 0; i < tiles.Length; i++)
        {
            tiles[i].RotateTile(tiles[0].coordinates, clockwise);
        }

        if (!shouldOffset)
        {
            return;
        }

        bool canOffset = Offset(oldRotationIndex, rotationIndex);

        if (!canOffset)
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

        offsetVal1 = curOffsetData[0, oldRotIndex];
        offsetVal2 = curOffsetData[0, newRotIndex];
        endOffset = offsetVal1 - offsetVal2;
        if (CanMovePiece(endOffset))
        {
            movePossible = true;
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
            offsetVal1 = curOffsetData[testIndex, oldRotIndex];
            offsetVal2 = curOffsetData[testIndex, newRotIndex];
            endOffset = offsetVal1 - offsetVal2;
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
            if (!tiles[i].SetTile())
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
