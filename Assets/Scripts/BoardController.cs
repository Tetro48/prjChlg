using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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

    public GameObject gridUnitPrefab, tileBlock;
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
    int[,] prevTextureIDGrid;
    Material[,] gridOfMaterials;

    private void Start()
    {
        CreateGrid();
    }

    private void FixedUpdate()
    {
        TopoutWarning();
        UpdateDisplay();
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
    void UpdateDisplay()
    {
        bool ghostPieceHit = false;
        int2[] ghostPieceCoords;
        int3[] ignoreProcessing = default;
        if(networkBoard.activePiece == null) return;
        ghostPieceCoords = new int2[networkBoard.activePiece.Length];
        ignoreProcessing = networkBoard.activePiece;
        for (int i = 0; i < networkBoard.activePiece.Length; i++)
        {
            ignoreProcessing[i] = networkBoard.activePiece[i];
            ghostPieceCoords[i] = ignoreProcessing[i].xy;
            int tempTextureID = ignoreProcessing[i].z;
            int2 pos = ignoreProcessing[i].xy;
            
            Vector2 offset = new Vector2(-(float)(tempTextureID % 4) / 4, (float)Math.Floor((double)tempTextureID/4+1) / 10);
            fullGrid[pos.x,pos.y].material.mainTextureOffset = Vector2.right - offset;
            if(i == networkBoard.activePiece.Length - 1)
            {
                while(!ghostPieceHit)
                {
                    for (int j = 0; j < networkBoard.activePiece.Length; j++)
                    {
                        if(!ghostPieceHit) ghostPieceHit = !IsPosEmpty(ghostPieceCoords[j] - new int2(0,1));
                        ghostPieceCoords[j] -= new int2(0,1);
                    }
                }
            }
        }
        
        for (int i = 0; i < gridSizeX; i++)
        {
            for (int j = 0; j < gridSizeY; j++)
            {
                if (prevTextureIDGrid[i,j] != fullGrid[i,j].textureID)
                {
                    prevTextureIDGrid[i,j] = fullGrid[i,j].textureID;
                    
                    Vector2 offset = new Vector2(-(float)(fullGrid[i,j].textureID % 4) / 4, (float)Math.Floor((double)fullGrid[i,j].textureID/4+1) / 10);
                    fullGrid[i,j].material.mainTextureOffset = Vector2.right - offset;
                }
                if(fullGrid[i,j].isOccupied)
                {
                    fullGrid[i,j].material.color = new Color(1,1,1,1f-fullGrid[i,j].transparency);
                }
                else fullGrid[i,j].material.color = new Color(1,1,1,0f);
            }
        }
        if(!networkBoard.piecesController.piecemovementlocked)for (int i = 0; i < ignoreProcessing.Length; i++)
        {
            Vector2 offset = new Vector2(-(float)(ignoreProcessing[i].z % 4) / 4, (float)Math.Floor((double)ignoreProcessing[i].z/4+1) / 10);
            fullGrid[ignoreProcessing[i].x, ignoreProcessing[i].y].material.mainTextureOffset = Vector2.right - offset;
            fullGrid[ignoreProcessing[i].x, ignoreProcessing[i].y].material.color = new Color(1,1,1,1f);
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
                        GridUnit newGridUnit = fullGrid[x, lineToDrop - 1];
                        if (curGridUnit.isOccupied)
                        {
                            MoveTile(curGridUnit, newGridUnit);
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
                curGridUnit.isOccupied = true;
                curGridUnit.material.mainTextureOffset = new Vector2(0,0);
                curGridUnit.textureID = 28;
            }
        }
    }
    public void CloneLineToBottom()
    {
        int isBigMode = networkBoard.bigMode ? 2 : 1;
        for (int i = 0; i < isBigMode; i++) for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = gridSizeY - 1; y >= 0 ; y--)
            {
                GridUnit curGridUnit = fullGrid[x,y];
                GridUnit newGridUnit = fullGrid[x,y+1];
                if (curGridUnit.isOccupied)
                {
                    MoveTile(curGridUnit, newGridUnit);
                    if (y == 0)
                    {
                        curGridUnit.isOccupied = true;
                        curGridUnit.material.mainTextureOffset = new Vector2(0,0);
                        if(networkBoard.sectAfter20g > 1) curGridUnit.textureID = 0;
                    }
                }        
            }
        }
        gameAudio.PlayOneShot(audioLineClone);
        if (networkBoard.piecesController.curPieceController != null) if(!networkBoard.piecesController.curPieceController.isPieceLocked()) if (networkBoard.LockDelayEnable) networkBoard.piecesController.curPieceController.MovePiece(new int2(0,1), true);
    }
    /// <summary>
    /// Destroys a line of tiles. Coded to also handle empty grid unit.
    /// </summary>
    public void DestroyLine(int line, bool particles = false)
    {
        for (int i = 0; i < gridSizeX; i++)
        {
            if(!fullGrid[i, line].isOccupied) continue;
            GridUnit tile = fullGrid[i, line];
            if(particles)
            {
                int tileTexture = tile.textureID;
                boardParticles.SummonParticles(new int2(i, line), tileTexture);
            }
            fullGrid[i, line].ClearGridUnit();
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
            DecayTile(new int2(i, line), percentage);
        }
    }
    /// <summary>
    /// Reduces tile's alpha color by percentage. 
    /// Note: Typing 1.0f float percentage will set a tile invisible. 
    /// </summary>
    /// <param name="percentage">Decrease tile's alpha color by percentage.</param>
    public void DecayTile(int2 coords, float percentage)
    {
        if(fullGrid[coords.x, coords.y].isOccupied)
        fullGrid[coords.x, coords.y].transparency += percentage;
    }
    /// <summary>
    /// Resets a line of tiles' transparency
    /// </summary>
    public void ResetLineTransparency(int line)
    {
        for (int i = 0; i < gridSizeX; i++)
        {
            fullGrid[i, line].transparency = 0f;
        }
    }

    /// <summary>
    /// Creates a grid of sized based off of gridSizeX and gridSizeY public variables
    /// </summary>
    private void CreateGrid()
    {
        fullGrid = new GridUnit[gridSizeX, gridSizeY];
        prevTextureIDGrid = new int[gridSizeX, gridSizeY];

        for(int y = 0; y < gridSizeY; y++)
        {
            for(int x = 0; x < gridSizeX; x++)
            {
                GridUnit newGridUnit = new GridUnit(gridUnitPrefab, tileBlock, transform, x, y);
                fullGrid[x, y] = newGridUnit;
            }
        }
    }

    /// <summary>
    /// Checks to see if the coorinate is a valid coordinate on the current tetris board.
    /// </summary>
    /// <param name="coordToTest">The x,y coordinate to test</param>
    /// <returns>Returns true if the coordinate to test is a vaild coordinate on the tetris board</returns>
    public bool IsInBounds(int2 coordToTest)
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
    public bool IsPosEmpty(int2 coordToTest)
    {
        if(coordToTest.y >= gridSizeY)
        {
            return false;
        }
        if (coordToTest.x < int2.zero.x || coordToTest.y < int2.zero.y)
        {
            return false;
        }

        else if(coordToTest.x >= gridSizeX) return false;
        else return !fullGrid[coordToTest.x, coordToTest.y].isOccupied;
    }

    /// <summary>
    /// Called when a piece is set in place. Sets the grid location to an occupied state.
    /// </summary>
    /// <param name="coords">The x,y coordinates to be occupied.</param>
    /// <param name="tileGO">GameObject of the specific tile on this grid location.</param>
    public void OccupyPos(int3 tile)
    {
        if (!IsInBounds(tile.xy))
        {
            return;
        }
        Vector2 offset = new Vector2(-(float)(tile.z % 4) / 4, (float)Math.Floor((double)tile.z/4+1) / 10);
        fullGrid[tile.x, tile.y].isOccupied = true;
        fullGrid[tile.x, tile.y].textureID = tile.z;
        fullGrid[tile.x, tile.y].material.mainTextureOffset = Vector2.right - offset;
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
        networkBoard.allClears++;
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
                            GridUnit newGridUnit = fullGrid[x, lineToDrop - 1];
                            if (curGridUnit.isOccupied)
                            {
                                MoveTile(curGridUnit, newGridUnit);
                            }
                        }
                    }
                }    
            }
        }
    }
    /// <summary>
    /// Moves an individual tile up one unit.
    /// </summary>
    /// <param name="curGridUnit">The grid unit that contains the tile to be moved up</param>
    void MoveTile(GridUnit curGridUnit, GridUnit newGridUnit)
    {
        newGridUnit.CopyFrom(curGridUnit);
        curGridUnit.ClearGridUnit();
    }

    /// <summary>
    /// Sets the tile in it's current position
    /// </summary>
    /// <returns>True if the tile is on the board. False if tile is above playing field, GAME OVER.</returns>
    public bool SetTile(int3 tile)
    {

        if (tile.y >= 24 || !IsPosEmpty(tile.xy) || tile.x >= 10) 
        {
            OccupyPos(tile);
            return false;
        }

        OccupyPos(tile);
        return true; // when if statement up here is false, that line of code is ignored.                     ⬆ is a reason why return true; will not execute when if statement is false.
    }
    /// <summary>
    /// Sets the tile in it's current position
    /// </summary>
    /// <returns>True if the tile is on the board. False if tile is above playing field, DESTROYED.</returns>
    public bool SetTileUp(int3 tile)
    {
        if (tile.y >= gridSizeY || tile.x >= gridSizeX)
        {
            return false;
        }

        OccupyPos(tile);
        return true;
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
        //     boardParticles.SummonParticles(new int2(x, lineToClear), tileTexture);
        // }
        DestroyLine(lineToClear, true);
    }
}

public class GridUnit
{
    public GameObject gameObject { get; private set; }
    public int2 position;
    public int textureID;
    public Material material;
    public float transparency; //strange
    public bool isOccupied;

    public GridUnit(GameObject newGameObject, GameObject tile, Transform boardParent, int x, int y)
    {
        transparency = 0f;
        gameObject = GameObject.Instantiate(newGameObject, boardParent);
        tile = GameObject.Instantiate(tile, gameObject.transform);
        material = tile.GetComponent<MeshRenderer>().material;
        material.color = new Color(1,1,1,0f);
        if(y>19) gameObject.GetComponent<SpriteRenderer>().sprite = null;
        isOccupied = false;
        position = new int2(x,y);

        gameObject.transform.localPosition = new Vector3(x, y);
    }
    public void CopyFrom(GridUnit grid)
    {
        textureID = grid.textureID;
        material.mainTextureOffset = grid.material.mainTextureOffset;
        isOccupied = grid.isOccupied;
    }
    public void ClearGridUnit()
    {
        textureID = 0;
        transparency = 0f;
        isOccupied = false;
    }
}