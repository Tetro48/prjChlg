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

    public static BoardController instance;

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
    [SerializeField]Sprite boneblock;

    List<int> allClearFireworkTime = new List<int>();

    GridUnit[,] fullGrid;

    private void Awake()
    {
        instance = this;
    }

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
                GameEngine.instance.SpawnFireworks();
            }
            if (allClearFireworkTime[fire] == 200)
            {
                allClearFireworkTime.RemoveAt(fire);
            }
            allClearFireworkTime[fire]++;
        }
        if(GameEngine.instance.framestepped && !MenuEngine.instance.GameOver)
        {
            if(linecleared == true)
            {
                LineClear(ldldy);
                arereseted = false;
            }
            else if(ldldy.Count > 0 && arereseted == false)
            {
                arereseted = true;
                GameEngine.instance.lineDelayf = 0;
                if(GameEngine.instance.level < 2099)GameEngine.instance.AREf = (int)Math.Floor(GameEngine.instance.ARE - GameEngine.instance.AREline);
            }
        }
    }

    public void TopoutWarning()
    {
        int tilesToWarn = 0;
        for (int y = 16; y < gridSizeY; y++)
        {
            for (int x = 0; x < gridSizeX; x++)
            {
                if (fullGrid[x,y].isOccupied)
                {
                    tilesToWarn += y-15;
                }
            }
        }
        if (tilesToWarn > 18 && !MenuEngine.instance.GameOver)
        {
            gameAudio.loop = true;
            if(!gameAudio.isPlaying)gameAudio.Play();
        }
        else
        {gameAudio.loop = false;}

    }
    private void LineClear(List<int> linesToClear)
    {
        GameEngine.instance.lineDelayf++;
        if(GameEngine.instance.lineDelayf >= (int)Math.Floor(GameEngine.instance.lineDelay) && GameEngine.instance.lineDelay >= 1)
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
                if(GameEngine.instance.sectAfter20g > 1) tileContr.tiles[0].GetComponent<SpriteRenderer>().sprite = boneblock;
                tileContr.tiles[0].UpdatePosition(new Vector2Int(x,line));
                tileContr.tiles[0].SetTileUp();
                PiecesController.instance.piecesInGame.Add(clonedTile);
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
                        PieceController tileContr = clonedTile.GetComponent<PieceController>();
                        if(GameEngine.instance.sectAfter20g > 1) tileContr.tiles[0].GetComponent<SpriteRenderer>().sprite = boneblock;
                        tileContr.tiles[0].UpdatePosition(new Vector2Int(x,y));
                        tileContr.tiles[0].SetTileUp();
                        PiecesController.instance.piecesInGame.Add(clonedTile);
                    }
                }        
            }
        }
        gameAudio.PlayOneShot(audioLineClone);
        if (PiecesController.instance.curPieceController != null) if(!PiecesController.instance.curPieceController.isPieceLocked()) if (PiecesController.instance.curPieceController.LockDelayEnable) PiecesController.instance.curPieceController.MovePiece(Vector2Int.up);
    }
    public void DestroyLine(int line)
    {
        for (int i = 0; i < gridSizeX; i++)
        {
            // if (line < 0)
            // {
            //     Debug.LogError("Out of bounds!");
            //     break;
            // }
            if(fullGrid[i,line].isOccupied){PieceController curPC = fullGrid[i, line].tileOnGridUnit.GetComponent<TileController>().pieceController;
            curPC.tiles[fullGrid[i, line].tileOnGridUnit.GetComponent<TileController>().tileIndex] = null;
            Destroy(fullGrid[i, line].tileOnGridUnit);
            
            if (!curPC.AnyTilesLeft()) { Destroy(curPC.gameObject); }}
            fullGrid[i, line].tileOnGridUnit = null;
            fullGrid[i,line].isOccupied = false;
        }
    }
    public void DecayLine(int line, float percentage)
    {
        for (int i = 0; i < gridSizeX; i++)
        {
            DecayTile(new Vector2Int(i, line), percentage);
        }
    }
    public void DecayTile(Vector2Int coords, float percentage)
    {
        if(fullGrid[coords.x, coords.y].isOccupied == true)fullGrid[coords.x, coords.y].tileOnGridUnit.GetComponent<SpriteRenderer>().color -= new Color(0f,0f,0f,percentage);
    }

    /// <summary>
    /// Creates a grid of sized based off of gridSizeX and gridSizeY public variables
    /// </summary>
    private void CreateGrid()
    {
        fullGrid = new GridUnit[gridSizeX, gridSizeY];

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
        gameAudio.PlayOneShot(PiecesController.instance.levelup);
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

        int linesFrozen = GameEngine.instance.lineFreezingMechanic ? GameEngine.instance.linesFrozen[GameEngine.instance.curSect] : 0;
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
                    Debug.Log("<color=red>T</color>" +
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
            bool tspinned = GameEngine.instance.tSpin;
            if (tspinned)
            {
                int limitedTSSEcount = linesToClear.Count > audioTSpinClear.Length ? audioTSpinClear.Length-1 : linesToClear.Count-1;
                gameAudio.PlayOneShot(audioTSpinClear[limitedTSSEcount]);
                GameEngine.instance.tSpin = false;
            }
            else
            {
                int limitedSEcount = linesToClear.Count > audioLineClear.Length ? audioLineClear.Length-1 : linesToClear.Count-1;
                gameAudio.PlayOneShot(audioLineClear[limitedSEcount]);
            }
            GameEngine.instance.LineClears(linesToClear.Count, tspinned);
            CheckAllClear();

            // PiecesController.instance.lineDelayf++;
            if(GameEngine.instance.lineDelay < 1)
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
    /// Displays the Tetris text when a Tetris line clear is achieved.
    /// </summary>
    void ShowTetrisText()
    {
        tetrisText.SetActive(true);
        Invoke("HideTetrisText", 4f);
    }

    /// <summary>
    /// Hides the Tetris line clear text.
    /// </summary>
    void HideTetrisText()
    {
        tetrisText.SetActive(false);
    }

    /// <summary>
    /// Moves an individual tile down one unit.
    /// </summary>
    /// <param name="curGridUnit">The grid unit that contains the tile to be moved down</param>
    void MoveTileDown(GridUnit curGridUnit)
    {
        TileController curTile = curGridUnit.tileOnGridUnit.GetComponent<TileController>();
        curTile.MoveTile(Vector2Int.down);
        curTile.SetTile();
        curGridUnit.tileOnGridUnit = null;
        curGridUnit.isOccupied = false;
    }
    /// <summary>
    /// Moves an individual tile up one unit.
    /// </summary>
    /// <param name="curGridUnit">The grid unit that contains the tile to be moved up</param>
    void MoveTileUp(GridUnit curGridUnit)
    {
        TileController curTile = curGridUnit.tileOnGridUnit.GetComponent<TileController>();
        curTile.MoveTile(Vector2Int.up);
        if(!curTile.SetTileUp())
        {
            Destroy(curGridUnit.tileOnGridUnit);
        }
        curGridUnit.tileOnGridUnit = null;
        curGridUnit.isOccupied = false;
    }

    /// <summary>
    /// Clears all tiles from a specified line
    /// </summary>
    /// <param name="lineToClear">Index of the line to be cleared</param>
    void ClearLine(int lineToClear)
    {
        int linesFrozen = GameEngine.instance.lineFreezingMechanic ? GameEngine.instance.linesFrozen[GameEngine.instance.curSect] : 0;
        if(lineToClear < linesFrozen || lineToClear > gridSizeY)
        {
            Debug.LogError("Error: Cannot Clear Line: " + lineToClear);
            return;
        }
        for(int x = 0; x < gridSizeX; x++)
        {
            PieceController curPC = fullGrid[x, lineToClear].tileOnGridUnit.GetComponent<TileController>().pieceController;
            curPC.tiles[fullGrid[x, lineToClear].tileOnGridUnit.GetComponent<TileController>().tileIndex] = null;
            int tileTexture = 0;
            for (int i = 0; i < 28; i++)
            {
                if(fullGrid[x, lineToClear].tileOnGridUnit.GetComponent<SpriteRenderer>().sprite == curPC.tileSprites[i])
                {
                    tileTexture = i;
                }
            }
            BoardParticleSystem.instance.SummonParticles(new Vector2Int(x, lineToClear), tileTexture);
            Destroy(fullGrid[x, lineToClear].tileOnGridUnit);
            if (!curPC.AnyTilesLeft()) { Destroy(curPC.gameObject); }
            fullGrid[x, lineToClear].tileOnGridUnit = null;
            fullGrid[x, lineToClear].isOccupied = false;
        }
    }

    /// <summary>
    /// Clears out the references to the piece being occupied on the grid unit,
    /// then drops all pieces above them by one unit.
    /// </summary>
    /// <param name="pieceCoords">Array of coordinates where where the pieces were occupying</param>
    public void PieceRemoved(Vector2Int[] pieceCoords)
    {
        foreach(Vector2Int coords in pieceCoords)
        {
            GridUnit curGridUnit = fullGrid[coords.x, coords.y];
            curGridUnit.tileOnGridUnit = null;
            curGridUnit.isOccupied = false;
        }

        for(int i = 0; i < pieceCoords.Length; i++)
        {
            for(int y = pieceCoords[i].y + 1; y < gridSizeY; y++)
            {
                GridUnit curGridUnit = fullGrid[pieceCoords[i].x, y];
                if (curGridUnit.isOccupied)
                {
                    MoveTileDown(curGridUnit);
                }
            }
        }
        CheckLineClears();
    }

    /// <summary>
    /// Determines which pieces are unavailable to be 'sacrificed.' Any piece where one tile is at the top of a given
    /// column is unable to be sacrificed.
    /// </summary>
    /// <returns>Returns a list of tiles unable to be sacrificed.</returns>
    public List<GameObject> GetUnavailablePieces()
    {
        List<GameObject> unavaiablePieces = new List<GameObject>();

        for (int x = 0; x < gridSizeX; x++) {
            for(int y = gridSizeY - 1; y >= 0; y--)
            {
                if (fullGrid[x, y].isOccupied)
                {
                    GameObject curPC = fullGrid[x, y].tileOnGridUnit.GetComponent<TileController>().pieceController.gameObject;
                    if (!unavaiablePieces.Any(test => test.GetInstanceID() == curPC.GetInstanceID()))
                    {
                        unavaiablePieces.Add(curPC);
                    }
                    y = -1;
                }
            }
        }
        Debug.Log("there are " + unavaiablePieces.Count + " Unavailable pieces");
        return unavaiablePieces;
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
        if(y<20) gameObject.GetComponent<SpriteRenderer>().sprite = null;
        location = new Vector2Int(x, y);
        isOccupied = false;

        gameObject.transform.position = new Vector3(location.x, location.y);
    }
}