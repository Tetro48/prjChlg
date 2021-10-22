using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

public class BoardController : MonoBehaviour {

    public NetworkBoard networkBoard;
    public BoardParticleSystem boardParticles;

    public int playerID;

    public GameObject gridUnitPrefab;
    public int gridSizeX, gridSizeY;
    public AudioSource gameAudio;
    public AudioClip[] audioLineClear, audioTSpinClear;
    public AudioClip audioLineFall, audioPieceLock, audioLineClone, warning;

    public GameObject tetrisText;
    public GameObject tileClone;

    bool linecleared = false;
    bool arereseted = false;
    List<int> ldldy = new List<int>();
    [SerializeField]Vector2 boneblockw, boneblock;

    List<int> allClearFireworkTime = new List<int>();

    GridUnit[,] fullGrid;
    Material[,] gridOfMaterials;

    private void Start()
    {
        CreateGrid();
        GameObject newTileClone = GameObject.Instantiate(tileClone, transform);
        newTileClone.name = "Garbage tile";
        newTileClone.SetActive(false);
        PieceController tileCtrl = newTileClone.GetComponent<PieceController>();
        tileCtrl.board = networkBoard;
        tileClone = newTileClone;
    }

    private void FixedUpdate()
    {
        TopoutWarning();
        for (int fire = 0; fire < allClearFireworkTime.Count; fire++)
        {
            if (allClearFireworkTime[fire] % 20 == 0)
            {
                networkBoard.SpawnFireworks();
            }
            if (allClearFireworkTime[fire] == 200)
            {
                allClearFireworkTime.RemoveAt(fire);
            }
            allClearFireworkTime[fire]++;
        }
        if(networkBoard.framestepped && !networkBoard.GameOver)
        {
            if(linecleared == true)
            {
                LineClear(ldldy);
                arereseted = false;
            }
            else if(ldldy.Count > 0 && arereseted == false)
            {
                arereseted = true;
                networkBoard.lineDelayf = 0;
                if(networkBoard.level < networkBoard.endingLevel - 1)networkBoard.AREf = (int)Math.Floor(networkBoard.ARE - networkBoard.AREline);
            }
        }
    }

    public void TopoutWarning()
    {
        int tilesToWarn = 0;
        for (int y = 15; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                if (fullGrid[x,y].isOccupied)
                {
                    tilesToWarn += y-14;
                }
            }
        }
        if (tilesToWarn > 18 && !networkBoard.GameOver)
        {
            gameAudio.loop = true;
            if(!gameAudio.isPlaying)gameAudio.Play();
        }
        else
        {gameAudio.loop = false;}

    }
    private void LineClear(List<int> linesToClear)
    {
        networkBoard.lineDelayf++;
        if(networkBoard.lineDelayf >= (int)Math.Floor(networkBoard.lineDelay) && networkBoard.lineDelay >= 1)
        {
            gameAudio.PlayOneShot(audioLineFall);
            linecleared = false;
            for(int i = 0; i < linesToClear.Count; i++)
            {
                /* The initial index of lineToDrop is calculated by taking the index of the first line
                * that was cleared then adding 1 to indicate the index of the line above the cleared line,
                * then the value i is subtracted to compensate for any lines already cleared.
                */
                for (int lineToDrop = linesToClear[i] + 1 - i; lineToDrop < gridSizeY; lineToDrop++)
                {
                    for (int x = 0; x < gridSizeX; x++)
                    {
                        GridUnit curGridUnit = fullGrid[x, lineToDrop];
                        if (curGridUnit.isOccupied)
                        {
                            MoveTileDown(curGridUnit);
                        }
                    }
                }
            }    
        }
    }
    public int TilesInALine(int line)
    {
        int amount = 0;
        for (int x = 0; x < gridSizeX; x++)
        {
            if(fullGrid[x, line].isOccupied)amount++;
        }
        return amount;
    }
    public void FillLine(int line)
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            GridUnit curGridUnit = fullGrid[x,line];
            if (!curGridUnit.isOccupied)
            {
                GameObject clonedTile = GameObject.Instantiate(tileClone, transform);
                PieceController tileContr = clonedTile.GetComponent<PieceController>();
                if(networkBoard.sectAfter20g > 1) tileContr.tiles[0].GetComponent<MeshRenderer>().material.mainTextureOffset = networkBoard.RS == RotationSystems.ARS ? boneblock : boneblockw;
                UpdatePosition(tileContr.tiles[0],new Vector2Int(x,line));
                SetTileUp(tileContr.tiles[0].gameObject);
                networkBoard.piecesController.piecesInGame.Add(clonedTile);
            }
        }
    }
    public void CloneLineToBottom()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = gridSizeY - 1; y >= 0 ; y--)
            {
                GridUnit curGridUnit = fullGrid[x,y];
                if (curGridUnit.isOccupied)
                {
                    MoveTileUp(curGridUnit);
                    if (y == 0)
                    {
                        GameObject clonedTile = GameObject.Instantiate(tileClone, transform);
                        clonedTile.SetActive(true);
                        PieceController tileContr = clonedTile.GetComponent<PieceController>();
                        if(networkBoard.sectAfter20g > 1) tileContr.tiles[0].GetComponent<MeshRenderer>().material.mainTextureOffset = networkBoard.RS == RotationSystems.ARS ? boneblock : boneblockw;
                        UpdatePosition(tileContr.tiles[0],new Vector2Int(x,y));
                        SetTileUp(tileContr.tiles[0].gameObject);
                        networkBoard.piecesController.piecesInGame.Add(clonedTile);
                    }
                }        
            }
        }
        gameAudio.PlayOneShot(audioLineClone);
        if (networkBoard.piecesController.curPieceController != null) if(!networkBoard.piecesController.curPieceController.isPieceLocked()) if (networkBoard.LockDelayEnable) networkBoard.piecesController.curPieceController.MovePiece(Vector2Int.up, true);
    }
    /// <summary>
    /// Destroys a line of tiles. Coded to also handle empty grid unit.
    /// </summary>
    public void DestroyLine(int line, bool particles = false)
    {
        for (int i = 0; i < gridSizeX; i++)
        {
            if(fullGrid[i,line].tileOnGridUnit == null) continue;
            TileController tile = fullGrid[i, line].tileOnGridUnit.GetComponent<TileController>();
            if(particles)
            {
                PieceController curPC = tile.pieceController;
                curPC.tiles[tile.tileIndex] = null;
                int tileTexture = tile.textureID;
                boardParticles.SummonParticles(new Vector2Int(i, line), tileTexture);
            }
            if(fullGrid[i,line].tileOnGridUnit != null)if(fullGrid[i,line].isOccupied){PieceController curPC = tile.pieceController;
            curPC.tiles[tile.tileIndex] = null;
            Destroy(fullGrid[i, line].tileOnGridUnit);
            
            if (!curPC.AnyTilesLeft()) { Destroy(curPC.gameObject); }}
            fullGrid[i, line].tileOnGridUnit = null;
            fullGrid[i,line].isOccupied = false;
        }
    }
    /// <summary>
    /// Reduces line of tiles' alpha color by percentage.
    /// Note: Typing 1.0f float percentage will set a line of tiles invisible. 
    /// </summary>
    /// <param name="percentage">Decrease a line of tiles' alpha color by percentage.</param>
    public void DecayLine(int line, float percentage)
    {
        for (int i = 0; i < gridSizeX; i++)
        {
            DecayTile(new Vector2Int(i, line), percentage);
        }
    }
    /// <summary>
    /// Reduces tile's alpha color by percentage. 
    /// Note: Typing 1.0f float percentage will set a tile invisible. 
    /// </summary>
    /// <param name="percentage">Decrease tile's alpha color by percentage.</param>
    public void DecayTile(Vector2Int coords, float percentage)
    {
        if(fullGrid[coords.x, coords.y].tileOnGridUnit != null)if(fullGrid[coords.x, coords.y].isOccupied == true)gridOfMaterials[coords.x, coords.y].color -= new Color(0f,0f,0f,percentage);
    }
    /// <summary>
    /// Resets a line of tiles' alpha color.
    /// </summary>
    public void ResetLineTransparency(int line)
    {
        for (int i = 0; i < gridSizeX; i++)
        {
            ResetTileTransparency(new Vector2Int(i, line));
        }
    }
    /// <summary>
    /// Self explanatory.
    /// </summary>
    public void ResetTileTransparency(Vector2Int coords)
    {
        if(fullGrid[coords.x, coords.y].isOccupied == true)gridOfMaterials[coords.x, coords.y].color = new Color(1f,1f,1f,1f);
    }

    /// <summary>
    /// Creates a grid of sized based off of gridSizeX and gridSizeY public variables
    /// </summary>
    private void CreateGrid()
    {
        fullGrid = new GridUnit[gridSizeX, gridSizeY];
        gridOfMaterials = new Material[gridSizeX, gridSizeY];

        for(int y = 0; y < gridSizeY; y++)
        {
            for(int x = 0; x < gridSizeX; x++)
            {
                GridUnit newGridUnit = new GridUnit(gridUnitPrefab, transform, x, y);
                fullGrid[x, y] = newGridUnit;
            }
        }
    }

    /// <summary>
    /// Checks to see if the coorinate is a valid coordinate on the current tetris board.
    /// </summary>
    /// <param name="coordToTest">The x,y coordinate to test</param>
    /// <returns>Returns true if the coordinate to test is a vaild coordinate on the tetris board</returns>
    public bool IsInBounds(Vector2Int coordToTest)
    {
        if (coordToTest.x < 0 || coordToTest.x >= gridSizeX || coordToTest.y < 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Checks to see if a given coordinate is occupied by a tetris piece
    /// </summary>
    /// <param name="coordToTest">The x,y coordinate to test</param>
    /// <returns>Returns true if the coordinate is not occupied by a tetris piece</returns>
    public bool IsPosEmpty(Vector2Int coordToTest)
    {
        if(coordToTest.y >= 40)
        {
            return true;
        }
        if (coordToTest.x < Vector2Int.zero.x || coordToTest.y < Vector2Int.zero.y)
        {
            return false;
        }

        if(fullGrid[coordToTest.x, coordToTest.y].isOccupied)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Called when a piece is set in place. Sets the grid location to an occupied state.
    /// </summary>
    /// <param name="coords">The x,y coordinates to be occupied.</param>
    /// <param name="tileGO">GameObject of the specific tile on this grid location.</param>
    public void OccupyPos(Vector2Int coords, GameObject tileGO)
    {
        fullGrid[coords.x, coords.y].isOccupied = true;
        if(fullGrid[coords.x, coords.y].tileOnGridUnit != null) Destroy(fullGrid[coords.x, coords.y].tileOnGridUnit);
        fullGrid[coords.x, coords.y].tileOnGridUnit = tileGO;
        gridOfMaterials[coords.x, coords.y] = tileGO.GetComponent<MeshRenderer>().material;
    }
    public bool CheckAllClear()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (fullGrid[x,y].isOccupied) return false;
            }
        }
        gameAudio.PlayOneShot(networkBoard.piecesController.levelup);
        allClearFireworkTime.Add(0);
        return true;
    }

    /// <summary>
    /// Checks line by line from bottom to top to see if that line is full and should be cleared.
    /// </summary>
    public void CheckLineClears()
    {
        //List of indexes for the lines that need to be cleared.
        List<int> linesToClear = new List<int>();

        //Counts how many lines next to each other will be cleared.
        //If this count get  to four lines, that is a Tetris line clear.
        int consecutiveLineClears = 0;

        int linesFrozen = networkBoard.lineFreezingMechanic ? networkBoard.linesFrozen[networkBoard.curSect] : 0;
        for(int y = linesFrozen; y < gridSizeY; y++)
        {
            bool lineClear = true;
            for(int x = 0; x < gridSizeX; x++)
            {
                if (!fullGrid[x, y].isOccupied){
                    lineClear = false;
                    consecutiveLineClears = 0;
                }
            }
            if (lineClear)
            {
                linesToClear.Add(y);
                consecutiveLineClears++;
                if (consecutiveLineClears == 4)
                {
                    if(GameEngine.debugMode) Debug.Log("<color=red>T</color>" +
                              "<color=orange>E</color>" +
                              "<color=yellow>T</color>" +
                              "<color=lime>R</color>" +
                              "<color=aqua>I</color>" +
                              "<color=purple>S</color>" +
                              "<color=blue>!</color>");
                }
                ClearLine(y);
            }
        }

        //Once the lines have been cleared, the lines above those will drop to fill in the empty space
        if (linesToClear.Count > 0)
        {
            linecleared = true;
            ldldy = linesToClear;
            bool tspinned = networkBoard.tSpin;
            if (tspinned)
            {
                int limitedTSSEcount = linesToClear.Count > audioTSpinClear.Length ? audioTSpinClear.Length-1 : linesToClear.Count-1;
                gameAudio.PlayOneShot(audioTSpinClear[limitedTSSEcount]);
                networkBoard.tSpin = false;
            }
            else
            {
                int limitedSEcount = linesToClear.Count > audioLineClear.Length ? audioLineClear.Length-1 : linesToClear.Count-1;
                gameAudio.PlayOneShot(audioLineClear[limitedSEcount]);
            }
            networkBoard.LineClears(linesToClear.Count, tspinned);
            
            CheckAllClear();

            // networkBoard.piecesController.lineDelayf++;
            if(networkBoard.lineDelay < 1)
            {
                gameAudio.PlayOneShot(audioLineFall);
                linecleared = false;
                for(int i = 0; i < linesToClear.Count; i++)
                {
                    /* The initial index of lineToDrop is calculated by taking the index of the first line
                    * that was cleared then adding 1 to indicate the index of the line above the cleared line,
                    * then the value i is subtracted to compensate for any lines already cleared.
                    */
                    for (int lineToDrop = linesToClear[i] + 1 - i; lineToDrop < gridSizeY; lineToDrop++)
                    {
                        for (int x = 0; x < gridSizeX; x++)
                        {
                            GridUnit curGridUnit = fullGrid[x, lineToDrop];
                            if (curGridUnit.isOccupied)
                            {
                                MoveTileDown(curGridUnit);
                            }
                        }
                    }
                }    
            }
        }
    }
    /// <summary>
    /// Moves an individual tile down one unit.
    /// </summary>
    /// <param name="curGridUnit">The grid unit that contains the tile to be moved down</param>
    void MoveTileDown(GridUnit curGridUnit)
    {
        GameObject curTile = curGridUnit.tileOnGridUnit;
        PieceController.MoveTile(curTile,Vector2Int.down);
        SetTile(curTile.gameObject);
        curGridUnit.tileOnGridUnit = null;
        curGridUnit.isOccupied = false;
    }
    /// <summary>
    /// Moves an individual tile up one unit.
    /// </summary>
    /// <param name="curGridUnit">The grid unit that contains the tile to be moved up</param>
    void MoveTileUp(GridUnit curGridUnit)
    {
        GameObject curTile = curGridUnit.tileOnGridUnit;
        PieceController.MoveTile(curTile,Vector2Int.up);
        if(!SetTileUp(curTile))
        {
            Destroy(curTile);
        }
        curGridUnit.tileOnGridUnit = null;
        curGridUnit.isOccupied = false;
    }

    /// <summary>
    /// Sets the tile in it's current position
    /// </summary>
    /// <returns>True if the tile is on the board. False if tile is above playing field, GAME OVER.</returns>
    public bool SetTile(GameObject obj)
    {

        if (obj.transform.localPosition.y >= 24 || !IsPosEmpty(V3ToV2Int(obj.transform.localPosition)) || obj.transform.localPosition.x >= 10) 
        {
            OccupyPos(V3ToV2Int(obj.transform.localPosition), obj);
            return false;
        }

        OccupyPos(V3ToV2Int(obj.transform.localPosition), obj);
        return true; // when if statement up here is false, that line of code is ignored.                     ⬆ is a reason why return true; will not execute when if statement is false.
    }
    /// <summary>
    /// Sets the tile in it's current position
    /// </summary>
    /// <returns>True if the tile is on the board. False if tile is above playing field, DESTROYED.</returns>
    public bool SetTileUp(GameObject obj)
    {
        if (obj.transform.localPosition.y >= 40 || obj.transform.localPosition.x >= 10)
        {
            return false;
        }

        OccupyPos(V3ToV2Int(obj.transform.localPosition), obj);
        return true;
    }
    public void UpdatePosition(GameObject obj, Vector2Int newPos)
    {
        Vector3 newV3Pos = new Vector3(newPos.x, newPos.y);
        obj.transform.localPosition = newV3Pos;
    }

    Vector2Int V3ToV2Int(Vector3 vector3)
    {
        return new Vector2Int((int)vector3.x, (int)vector3.y);
    }

    /// <summary>
    /// Clears all tiles from a specified line
    /// </summary>
    /// <param name="lineToClear">Index of the line to be cleared</param>
    void ClearLine(int lineToClear)
    {
        int linesFrozen = networkBoard.lineFreezingMechanic ? networkBoard.linesFrozen[networkBoard.curSect] : 0;
        if(lineToClear < linesFrozen || lineToClear > gridSizeY)
        {
            if(GameEngine.debugMode) Debug.LogError("Error: Cannot Clear Line: " + lineToClear);
            return;
        }
        // for(int x = 0; x < gridSizeX; x++)
        // {
        //     PieceController curPC = fullGrid[x, lineToClear].tileOnGridUnit.GetComponent<TileController>().pieceController;
        //     curPC.tiles[fullGrid[x, lineToClear].tileOnGridUnit.GetComponent<TileController>().tileIndex] = null;
        //     int tileTexture = 0;
        //     for (int i = 0; i < 28; i++)
        //     {
        //         if(fullGrid[x, lineToClear].tileOnGridUnit.GetComponent<SpriteRenderer>().sprite == curPC.tileSprites[i])
        //         {
        //             tileTexture = i;
        //         }
        //     }
        //     boardParticles.SummonParticles(new Vector2Int(x, lineToClear), tileTexture);
        // }
        DestroyLine(lineToClear, true);
    }
}

public class GridUnit
{
    public GameObject gameObject { get; private set; }
    public GameObject tileOnGridUnit;
    public Vector2Int location { get; private set; }
    public bool isOccupied;

    public GridUnit(GameObject newGameObject, Transform boardParent, int x, int y)
    {
        gameObject = GameObject.Instantiate(newGameObject, boardParent);
        if(y>19) gameObject.GetComponent<SpriteRenderer>().sprite = null;
        location = new Vector2Int(x, y);
        isOccupied = false;

        gameObject.transform.localPosition = new Vector3(location.x, location.y);
    }
}