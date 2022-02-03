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
    int[,] textureIDs;
    float[,] transparencyGrid;
    int[,] prevTextureIDGrid;
    Material[,] gridOfMaterials;
    private void Start()
    {
        CreateGrid();
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
    public void UpdateActivePiece(int3[] piece, bool transparent = false)
    {
        for (int i = 0; i < piece.Length; i++)
        {
            if (!transparent) UpdateOccupiedPosition(piece[i]);
            else
                UpdateOccupiedPosition(new int3(piece[i].xy, -1));
        }
    }
    void UpdateRender()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                UpdateOccupiedPosition(new int3(x,y, textureIDs[x,y]));
            }
        }
    }
    public void UpdateOccupiedPosition(int3 tile)
    {
        if (math.any(tile.xy < int2.zero | tile.xy >= new int2(gridSizeX, gridSizeY)))
        {
            return;
        }
        if (tile.z >= 0)
        {
            Vector2 offset = TextureUVs.UVs[tile.z];
            gridOfMaterials[tile.x, tile.y].mainTextureOffset = Vector2.right - offset;
        }
        gridOfMaterials[tile.x, tile.y].color = new Color(1,1,1, tile.z >= 0 ? 1 - transparencyGrid[tile.x,tile.y] : 0);
    }

    public void TopoutWarning()
    {
        int tilesToWarn = 0;
        for (int y = 15; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                if (textureIDs[x,y] >= 0)
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
                        if (textureIDs[x, lineToDrop] >= 0)
                        {
                            textureIDs[x, lineToDrop - 1] = textureIDs[x, lineToDrop];
                            textureIDs[x, lineToDrop] = -1;
                            transparencyGrid[x, lineToDrop - 1] = transparencyGrid[x, lineToDrop];
                            transparencyGrid[x, lineToDrop] = 0f;
                        }
                    }
                }
            }
            UpdateRender();
        }
    }
    public int TilesInALine(int line)
    {
        int amount = 0;
        for (int x = 0; x < gridSizeX; x++)
        {
            if(textureIDs[x,line] >= 0)amount++;
        }
        return amount;
    }
    public void FillLine(int line)
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            textureIDs[x,line] = 0;
        }
    }
    public void CloneLineToBottom()
    {
        int isBigMode = networkBoard.bigMode ? 2 : 1;
        for (int i = 0; i < isBigMode; i++) for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = gridSizeY - 1; y >= 0 ; y--)
            {
                int additive;
                if(y+1 >= gridSizeY) additive = y;
                else additive = y + 1;
                if (textureIDs[x, y] >= 0 || textureIDs[x, additive] >= 0)
                {
                    textureIDs[x, additive] = textureIDs[x, y];
                    transparencyGrid[x, additive] = transparencyGrid[x, y];
                    if (y == 0)
                    {
                        additive = networkBoard.RS == RotationSystems.SRS ? 8 : 0;
                        if(networkBoard.sectAfter20g > 1) textureIDs[x,y] = 16 + additive;
                        else textureIDs[x,y] = 0 + additive;

                    }
                }        
            }
        }
        gameAudio.PlayOneShot(audioLineClone);
        UpdateRender();
        if (networkBoard.LockDelayEnable) networkBoard.MovePiece(new int2(0,1), true);
    }
    /// <summary>
    /// Destroys a line of tiles. Coded to also handle empty grid unit.
    /// </summary>
    public void DestroyLine(int line, bool particles = false)
    {
        for (int i = 0; i < gridSizeX; i++)
        {
            if(textureIDs[i,line] < 0) continue;
            if(particles)
            {
                int tileTexture = textureIDs[i,line];
                boardParticles.SummonParticles(new int2(i, line), tileTexture);
            }
            textureIDs[i,line] = -1;
            transparencyGrid[i,line] = 0f;
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
        if (textureIDs[coords.x, coords.y] < 0)
            return;
        transparencyGrid[coords.x, coords.y] += percentage;
    }
    /// <summary>
    /// Resets a line of tiles' transparency
    /// </summary>
    public void ResetLineTransparency(int line)
    {
        for (int i = 0; i < gridSizeX; i++)
        {
            transparencyGrid[i, line] = 0f;
        }
    }

    /// <summary>
    /// Creates a grid of sized based off of gridSizeX and gridSizeY public variables
    /// </summary>
    private void CreateGrid()
    {
        textureIDs = new int[gridSizeX, gridSizeY];
        transparencyGrid = new float[gridSizeX, gridSizeY];
        gridOfMaterials = new Material[gridSizeX, gridSizeY];
        prevTextureIDGrid = new int[gridSizeX, gridSizeY];
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                textureIDs[x,y] = -1;
                GameObject instantiatedTile = Instantiate(tileBlock, transform);
                instantiatedTile.transform.localPosition = new Vector2(x,y);
                gridOfMaterials[x,y] = instantiatedTile.GetComponent<MeshRenderer>().material;
                gridOfMaterials[x, y].color = new Color(1,1,1,0);
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
        if (coordToTest.x < 0 || coordToTest.y < 0)
        {
            return false;
        }

        else if(coordToTest.x >= gridSizeX) return false;
        else return textureIDs[coordToTest.x, coordToTest.y] < 0;
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
        textureIDs[tile.x, tile.y] = tile.z;
        UpdateOccupiedPosition(tile);
    }
    public bool CheckAllClear()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                if (textureIDs[x,y] >= 0) return false;
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
                if (textureIDs[x, y] < 0){
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
                            if (textureIDs[x, lineToDrop] >= 0)
                            {
                                textureIDs[x, lineToDrop - 1] = textureIDs[x, lineToDrop];
                                textureIDs[x, lineToDrop] = -1;
                                transparencyGrid[x, lineToDrop - 1] = transparencyGrid[x, lineToDrop];
                                transparencyGrid[x, lineToDrop] = 0f;
                            }
                        }
                    }
                }    
            }
        }
        UpdateRender();
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