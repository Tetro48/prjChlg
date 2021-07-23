using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

/*
    Project Challenger, an challenging Tetris game.
    Copyright (C) 2021,  Aymir

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

public class PiecesController : MonoBehaviour {

    public static PiecesController instance;

    public int playerID;

    public GameObject piecePrefab;
    public Vector2Int spawnPos;
    public float dropTime;
    public int turnsToSac;
    public Coroutine dropCurPiece;
    public Vector2Int[,] JLSTZ_OFFSET_DATA { get; private set; }
    public Vector2Int[,] I_OFFSET_DATA { get; private set; }
    public Vector2Int[,] O_OFFSET_DATA { get; private set; }
    public List<GameObject> piecesInGame;
    public GameObject pieceToDestroy = null;
    public GameObject sacText, gameOverText;
    public bool piecemovementlocked = false;
    public List<int> bag;
    public float gravityTiles;
    public int pieceHold = 28;

    public AudioSource gameAudio;
    public AudioClip nextpiece1, nextpiece2, nextpiece3, nextpiece4, nextpiece5, nextpiece6, nextpiece7, audioIRS, audioIHS, bell, levelup;
    public GameObject[] nextPieceUI;
    public GameObject[] nextIBMWPieceUI;
    public GameObject[] nextARSPieceUI;
    public GameObject[] nextIBMPieceUI;
    public GameObject[] holdPieceUI;
    //{ Up, CW, CCW, UD, Hold }
    public bool[] PrevInputs;
    

    private bool executedHold = false;
    public bool allowHold = true;
    private int frames;
    private int DASfl, DASfr;
    public int ARRtuning = 1;
    private int DAStuning = (int)GameEngine.instance.DAS;
    public int ARE = 30;
    public int AREf = 29;
    public int AREline = 10;
    public int lineDelayf = 0;

    private bool IRSCW, IRSCCW, IRSUD, IARS, IHS;

    private bool nextpiecequeued = false;
    private bool audioBellPlayed = false;
    public int pieces;
    

    GameObject curPiece = null;
    public PieceController curPieceController = null;
    List<GameObject> availablePieces;


    /// <summary>
    /// Called as soon as the instance is enabled. Sets the singleton and offset data arrays.
    /// </summary>
    private void Awake()
    {
        instance = this;
        
        if(GameEngine.instance.replay.mode != ReplayModeType.read)
        {
            bag = new List<int>();
            for (int i = 0; i < 16; i++)
            {
                // bag.Add(new List<int> { 0, 1, 2, 3, 4, 5, 6 });
                
                List<int> bagshuff = new List<int>(){0,1,2,3,4,5,6};
                Shuffle(bagshuff);
                Debug.Log(bagshuff);
                for (int j = 0; j < 7; j++)
                {
                    bag.Add(bagshuff[j]);
                }
            }
        }
        else bag = GameEngine.instance.replay.bag;

        JLSTZ_OFFSET_DATA = new Vector2Int[5, 4];
        JLSTZ_OFFSET_DATA[0, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[0, 1] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[0, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[0, 3] = Vector2Int.zero;

        JLSTZ_OFFSET_DATA[1, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[1, 1] = new Vector2Int(1,0);
        JLSTZ_OFFSET_DATA[1, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[1, 3] = new Vector2Int(-1, 0);

        JLSTZ_OFFSET_DATA[2, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[2, 1] = new Vector2Int(1, -1);
        JLSTZ_OFFSET_DATA[2, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[2, 3] = new Vector2Int(-1, -1);

        JLSTZ_OFFSET_DATA[3, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[3, 1] = new Vector2Int(0, 2);
        JLSTZ_OFFSET_DATA[3, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[3, 3] = new Vector2Int(0, 2);

        JLSTZ_OFFSET_DATA[4, 0] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[4, 1] = new Vector2Int(1, 2);
        JLSTZ_OFFSET_DATA[4, 2] = Vector2Int.zero;
        JLSTZ_OFFSET_DATA[4, 3] = new Vector2Int(-1, 2);

        I_OFFSET_DATA = new Vector2Int[5, 4];
        I_OFFSET_DATA[0, 0] = Vector2Int.zero;
        I_OFFSET_DATA[0, 1] = new Vector2Int(-1, 0);
        I_OFFSET_DATA[0, 2] = new Vector2Int(-1, 1);
        I_OFFSET_DATA[0, 3] = new Vector2Int(0, 1);

        I_OFFSET_DATA[1, 0] = new Vector2Int(-1, 0);
        I_OFFSET_DATA[1, 1] = Vector2Int.zero;
        I_OFFSET_DATA[1, 2] = new Vector2Int(1, 1);
        I_OFFSET_DATA[1, 3] = new Vector2Int(0, 1);

        I_OFFSET_DATA[2, 0] = new Vector2Int(2, 0);
        I_OFFSET_DATA[2, 1] = Vector2Int.zero;
        I_OFFSET_DATA[2, 2] = new Vector2Int(-2, 1);
        I_OFFSET_DATA[2, 3] = new Vector2Int(0, 1);

        I_OFFSET_DATA[3, 0] = new Vector2Int(-1, 0);
        I_OFFSET_DATA[3, 1] = new Vector2Int(0, 1);
        I_OFFSET_DATA[3, 2] = new Vector2Int(1, 0);
        I_OFFSET_DATA[3, 3] = new Vector2Int(0, -1);

        I_OFFSET_DATA[4, 0] = new Vector2Int(2, 0);
        I_OFFSET_DATA[4, 1] = new Vector2Int(0, -2);
        I_OFFSET_DATA[4, 2] = new Vector2Int(-2, 0);
        I_OFFSET_DATA[4, 3] = new Vector2Int(0, 2);

        O_OFFSET_DATA = new Vector2Int[1, 4];
        O_OFFSET_DATA[0, 0] = Vector2Int.zero;
        O_OFFSET_DATA[0, 1] = Vector2Int.down;
        O_OFFSET_DATA[0, 2] = new Vector2Int(-1, -1);
        O_OFFSET_DATA[0, 3] = Vector2Int.left;

        if(GameEngine.instance.RS == RotationSystems.ARS)
        {
            JLSTZ_OFFSET_DATA = new Vector2Int[5, 4];
            JLSTZ_OFFSET_DATA[0, 0] = Vector2Int.up;
            JLSTZ_OFFSET_DATA[0, 1] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[0, 2] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[0, 3] = Vector2Int.zero;

            JLSTZ_OFFSET_DATA[1, 0] = new Vector2Int(-1, 1);
            JLSTZ_OFFSET_DATA[1, 1] = new Vector2Int(0, 0);
            JLSTZ_OFFSET_DATA[1, 2] = new Vector2Int(-1, 0);
            JLSTZ_OFFSET_DATA[1, 3] = new Vector2Int(-2, 0);

            JLSTZ_OFFSET_DATA[2, 0] = new Vector2Int(-1, 1);
            JLSTZ_OFFSET_DATA[2, 1] = new Vector2Int(-2, 0);
            JLSTZ_OFFSET_DATA[2, 2] = new Vector2Int(-1, 0);
            JLSTZ_OFFSET_DATA[2, 3] = new Vector2Int(0, 0);

            JLSTZ_OFFSET_DATA[3, 0] = new Vector2Int(0, 0);
            JLSTZ_OFFSET_DATA[3, 1] = new Vector2Int(0, 0);
            JLSTZ_OFFSET_DATA[3, 2] = new Vector2Int(0, 0);
            JLSTZ_OFFSET_DATA[3, 3] = new Vector2Int(0, 0);

            JLSTZ_OFFSET_DATA[4, 0] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[4, 1] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[4, 2] = Vector2Int.zero;
            JLSTZ_OFFSET_DATA[4, 3] = Vector2Int.zero;

            I_OFFSET_DATA = new Vector2Int[5, 4];
            I_OFFSET_DATA[0, 0] = Vector2Int.zero;
            I_OFFSET_DATA[0, 1] = new Vector2Int(-1, 0);
            I_OFFSET_DATA[0, 2] = new Vector2Int(-1, 0);
            I_OFFSET_DATA[0, 3] = new Vector2Int(-1, 1);

            I_OFFSET_DATA[1, 0] = new Vector2Int(-1, 0);
            I_OFFSET_DATA[1, 1] = Vector2Int.zero;
            I_OFFSET_DATA[1, 2] = new Vector2Int(1, 1);
            I_OFFSET_DATA[1, 3] = new Vector2Int(0, 1);

            I_OFFSET_DATA[2, 0] = new Vector2Int(2, 0);
            I_OFFSET_DATA[2, 1] = Vector2Int.zero;
            I_OFFSET_DATA[2, 2] = new Vector2Int(-2, 1);
            I_OFFSET_DATA[2, 3] = new Vector2Int(0, 1);

            I_OFFSET_DATA[3, 0] = new Vector2Int(-1, 0);
            I_OFFSET_DATA[3, 1] = new Vector2Int(0, 1);
            I_OFFSET_DATA[3, 2] = new Vector2Int(1, 0);
            I_OFFSET_DATA[3, 3] = new Vector2Int(0, -1);

            I_OFFSET_DATA[4, 0] = new Vector2Int(2, 0);
            I_OFFSET_DATA[4, 1] = new Vector2Int(0, -2);
            I_OFFSET_DATA[4, 2] = new Vector2Int(-2, 0);
            I_OFFSET_DATA[4, 3] = new Vector2Int(0, 2);

            O_OFFSET_DATA = new Vector2Int[1, 4];
            O_OFFSET_DATA[0, 0] = Vector2Int.zero;
            O_OFFSET_DATA[0, 1] = Vector2Int.down;
            O_OFFSET_DATA[0, 2] = new Vector2Int(-1, -1);
            O_OFFSET_DATA[0, 3] = Vector2Int.left;
        }
    }

    /// <summary>
    /// Called at the first frame instance is enabled. Sets some variables.
    /// </summary>
    private void Start()
    {
        piecesInGame = new List<GameObject>();
        availablePieces = new List<GameObject>();
    }

    bool bagPieceRetrieved;
    /// <summary>
    /// Called once game trying to get a tetromino piece.
    /// </summary>
    public void UpdatePieceBag()
    {
        while (bagPieceRetrieved == false)
        {
            if(bag[pieces] < 7)
            {
                if((GameEngine.instance.ARE < 1 && (!GameEngine.instance.ending || GameEngine.instance.AREf >= 0)) || executedHold == true)
                {
                    GameEngine.instance.lineClonePiecesLeft--;
                    if (GameEngine.instance.lineClonePiecesLeft == 0)
                    {
                        GameEngine.instance.lineClonePiecesLeft = GameEngine.instance.lineClonePerPiece[GameEngine.instance.curSect];
                        BoardController.instance.CloneLineToBottom();
                    }
                    else if (GameEngine.instance.lineClonePiecesLeft > GameEngine.instance.lineClonePerPiece[GameEngine.instance.curSect])
                    {
                        GameEngine.instance.lineClonePiecesLeft = GameEngine.instance.lineClonePerPiece[GameEngine.instance.curSect];
                    }
                    SpawnPiece(bag[pieces]);
                    for (int i = 0; i < (int)Math.Floor(gravityTiles); i++)
                    {
                        if(piecemovementlocked == false)MoveCurPiece(Vector2Int.down);
                    }
                    gravityTiles -= (float)Math.Floor(gravityTiles);
                }
                else
                {
                    nextpiecequeued = true;
                }
                bagPieceRetrieved = true;
                Debug.Log("Next random num: " + bag[pieces]);
                executedHold = false;
            }
        }
        if (pieces % 7 == 0 && GameEngine.instance.replay.mode != ReplayModeType.read)
        {
            List<int> bagshuff = new List<int>(){0,1,2,3,4,5,6};
            Shuffle(bagshuff);
            Debug.Log(bagshuff);
            for (int j = 0; j < 7; j++)
            {
                bag.Add(bagshuff[j]);
            }
        }
        GameEngine.instance.replay.bag = bag;
        bagPieceRetrieved = false;
    }
    bool IHSexecuted;
    public void UpdateShownPieces()
    {
        int nxtibmPieces = (GameEngine.instance.nextibmblocks > GameEngine.instance.nextPieces)  ? GameEngine.instance.nextPieces : GameEngine.instance.nextibmblocks;
        for (int i = 0; i < GameEngine.instance.nextPieces; i++)
        {
            if (GameEngine.instance.RS == RotationSystems.SRS)
            {
                if(GameEngine.instance.level < 600)
                {
                    if(pieces > 0)nextPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
                    nextPieceUI[i*7+bag[i+pieces]].SetActive(true);
                }
                else
                {
                    if(pieces > 0)nextPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
                    if(pieces > 0)nextIBMWPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
                    if(i>= GameEngine.instance.nextPieces - nxtibmPieces)nextIBMWPieceUI[i*7+bag[i+pieces]].SetActive(true);
                    else nextPieceUI[i*7+bag[i+pieces]].SetActive(true);
                }
            }
            else if (GameEngine.instance.RS == RotationSystems.ARS)
            {
                if(GameEngine.instance.level < 600)
                {
                    if(pieces > 0)nextARSPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
                    nextARSPieceUI[i*7+bag[i+pieces]].SetActive(true);
                }
                else
                {
                    if(pieces > 0)nextARSPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
                    if(pieces > 0)nextIBMPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
                    if(i>= GameEngine.instance.nextPieces - nxtibmPieces)nextIBMPieceUI[i*7+bag[i+pieces]].SetActive(true);
                    else nextARSPieceUI[i*7+bag[i+pieces]].SetActive(true);
                }
            }
        }
        if (pieceHold < 28)holdPieceUI[pieceHold].SetActive(true);
    }
    private void NextPiece()
    {
        if (allowHold == true && GameEngine.instance.level >= 600 && GameEngine.instance.nextibmblocks < GameEngine.instance.nextPieces + 1)
        {
            GameEngine.instance.nextibmblocks++;
        }
        UpdateShownPieces();
        if (!IHSexecuted)
        {
            if(bag[pieces] == 0)
            {
                gameAudio.PlayOneShot(nextpiece2);
            }
            if(bag[pieces] == 1)
            {
                gameAudio.PlayOneShot(nextpiece1);
            }
            if(bag[pieces] == 2)
            {
                gameAudio.PlayOneShot(nextpiece6);
            }
            if(bag[pieces] == 3)
            {
                gameAudio.PlayOneShot(nextpiece3);
            }
            if(bag[pieces] == 4)
            {
                gameAudio.PlayOneShot(nextpiece7);
            }
            if(bag[pieces] == 5)
            {
                gameAudio.PlayOneShot(nextpiece5);
            }
            if(bag[pieces] == 6)
            {
                gameAudio.PlayOneShot(nextpiece4);
            }
        }
        if(GameEngine.instance.level % 100 < 99 && GameEngine.instance.level < 2100 && pieces > 1 && allowHold == true)
        {
            GameEngine.instance.level++;
            audioBellPlayed = false;
        }
        else if(audioBellPlayed == false && pieces > 1 && allowHold == true)
        {
            audioBellPlayed = true;
            gameAudio.PlayOneShot(bell);
        }
        IHSexecuted = false;
    }

    /// <summary>
    /// Called once every frame. Checks for player input.
    /// </summary>
    private void FixedUpdate()
    {
        if (GameEngine.instance.lineClonePiecesLeft == 0)
        {
            GameEngine.instance.lineClonePiecesLeft = GameEngine.instance.lineClonePerPiece[GameEngine.instance.curSect];
            BoardController.instance.CloneLineToBottom();
        }
        else if (GameEngine.instance.lineClonePiecesLeft > GameEngine.instance.lineClonePerPiece[GameEngine.instance.curSect])
        {
            GameEngine.instance.lineClonePiecesLeft = GameEngine.instance.lineClonePerPiece[GameEngine.instance.curSect];
        }

        DAStuning = (int)GameEngine.instance.DAS;
        if (GameEngine.instance.framestepped && !MenuEngine.instance.GameOver)
        {
            frames++;
            if(piecemovementlocked == false)gravityTiles += GameEngine.instance.gravity;
            if (nextpiecequeued == true)
            {
                GameEngine.instance.AREf++;
                if (GameEngine.instance.AREf >= (int)Math.Floor(GameEngine.instance.ARE))
                {
                    nextpiecequeued = false;
                    GameEngine.instance.lineClonePiecesLeft--;
                    if (GameEngine.instance.lineClonePiecesLeft == 0)
                    {
                        GameEngine.instance.lineClonePiecesLeft = GameEngine.instance.lineClonePerPiece[GameEngine.instance.curSect];
                        BoardController.instance.CloneLineToBottom();
                    }
                    else if (GameEngine.instance.lineClonePiecesLeft > GameEngine.instance.lineClonePerPiece[GameEngine.instance.curSect])
                    {
                        GameEngine.instance.lineClonePiecesLeft = GameEngine.instance.lineClonePerPiece[GameEngine.instance.curSect];
                    }
                    SpawnPiece(bag[pieces]);
                    GameEngine.instance.AREf = 0;
                }
                if ((GameEngine.instance.Inputs[2] || GameEngine.instance.Inputs[6]) && (!GameEngine.instance.Inputs[1]) && (!GameEngine.instance.Inputs[3])) {IRSCW = true; IRSCCW = false; IRSUD = false;}
                else if ((!GameEngine.instance.Inputs[2] && !GameEngine.instance.Inputs[6]) && (GameEngine.instance.Inputs[1]) && !(GameEngine.instance.Inputs[3])) {IRSCCW = true; IRSCW = false; IRSUD = false;}
                else if ((!GameEngine.instance.Inputs[2] && !GameEngine.instance.Inputs[6]) && (!GameEngine.instance.Inputs[1]) && (GameEngine.instance.Inputs[3])) {IRSCCW = false; IRSCW = false; IRSUD = true;}
                else {IRSCCW = false; IRSCW = false; IRSUD = false;}
                if (GameEngine.instance.Inputs[4]) IHS = true;
                else IHS = false;
                if (GameEngine.instance.RS == RotationSystems.ARS) IARS = true;
            }
            else {IRSCCW = false; IRSCW = false; IRSUD = false; IARS = false; IHS = false;}
            if (GameEngine.instance.movement.y < -0.5 && !piecemovementlocked)
            {
                gravityTiles += (float)GameEngine.instance.gravity * (float)GameEngine.instance.SDF * (GameEngine.instance.movement.y * -1);
            }
            if (GameEngine.instance.movement.x < -0.5)
            {
                if (DASfl == 0 && !piecemovementlocked) MoveCurPiece(Vector2Int.left);
                DASfl++;
                if (DASfl > DAStuning && !piecemovementlocked)
                {
                    MoveCurPiece(Vector2Int.left);
                    if (ARRtuning == 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            MoveCurPiece(Vector2Int.left);
                        }
                    }
                }
            }
            else DASfl = 0;
            if (GameEngine.instance.movement.x > 0.5)
            {
                if (DASfr == 0 && !piecemovementlocked) MoveCurPiece(Vector2Int.right);
                DASfr++;
                if (DASfr > DAStuning && !piecemovementlocked)
                {
                    MoveCurPiece(Vector2Int.right);
                    if (ARRtuning == 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            MoveCurPiece(Vector2Int.right);
                        }
                    }
                }
            }
            else DASfr = 0;
            // if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            // {
            //     if(curPieceController != null)
            //     {
            //         return;
            //     }
            //     UpdatePieceBag();
            // }
            if (Input.GetKeyDown(KeyCode.Escape)){
                MenuEngine.instance.GameOver = true;
                MenuEngine.instance.IntentionalGameOver = true;
                MenuEngine.instance.frames = 300;
            }

            if (((GameEngine.instance.Inputs[4] && !PrevInputs[4]) || IHS) && !piecemovementlocked && allowHold)
            {
                if (IHS)
                {
                    gameAudio.PlayOneShot(audioIHS);
                    IHSexecuted = true;
                }
                ExecuteHold();
            }
            if (((GameEngine.instance.Inputs[2] && !PrevInputs[2]) || (GameEngine.instance.Inputs[6] && !PrevInputs[6]) || IRSCW) && !piecemovementlocked)
            {
                curPieceController.RotatePiece(true, true, false);
                if (IRSCW)
                {
                    gameAudio.PlayOneShot(audioIRS);
                }
            }
            if (((GameEngine.instance.Inputs[1] && !PrevInputs[1]) || IRSCCW) && !piecemovementlocked)
            {
                curPieceController.RotatePiece(false, true, false);
                if (IRSCCW)
                {
                    gameAudio.PlayOneShot(audioIRS);
                }
            }
            if (((GameEngine.instance.Inputs[3] && !PrevInputs[3]) || IRSUD) && !piecemovementlocked)
            {
                curPieceController.RotatePiece(true, false, true);
                if (IRSUD)
                {
                    gameAudio.PlayOneShot(audioIRS);
                }
            }
            if (IARS && !piecemovementlocked) curPieceController.RotatePiece(true, false, true);


            // if (Input.GetKeyDown(KeyCode.Alpha0))
            // {
            //     SpawnDebug(0);
            // }
            // if (Input.GetKeyDown(KeyCode.Alpha1))
            // {
            //     SpawnDebug(1);
            // }
            // if (Input.GetKeyDown(KeyCode.Alpha2))
            // {
            //     SpawnDebug(2);
            // }
            // if (Input.GetKeyDown(KeyCode.Alpha3))
            // {
            //     SpawnDebug(3);
            // }
            // if (Input.GetKeyDown(KeyCode.Alpha4))
            // {
            //     SpawnDebug(4);
            // }
            // if (Input.GetKeyDown(KeyCode.Alpha5))
            // {
            //     SpawnDebug(5);
            // }
            // if (Input.GetKeyDown(KeyCode.Alpha6))
            // {
            //     SpawnDebug(6);
            // }
            if (curPieceController != null) if (!curPieceController.CanMovePiece(Vector2Int.zero) && !piecemovementlocked) curPieceController.SendPieceToFloor();
            for (int i = 0; i < (int)Math.Floor(gravityTiles); i++)
            {
                if(piecemovementlocked == false)MoveCurPiece(Vector2Int.down);
            }
            gravityTiles -= (float)Math.Floor(gravityTiles);
            if (GameEngine.instance.Inputs[0] && !PrevInputs[0] && !piecemovementlocked)
            {
                curPieceController.SendPieceToFloor();
            }
            for (int i = 1; i < 7; i++)
            {
                PrevInputs[i] = GameEngine.instance.Inputs[i];
            }
            if (!piecemovementlocked || PrevInputs[0]) PrevInputs[0] = GameEngine.instance.Inputs[0];
        }
    }

    /// <summary>
    /// Drops the piece the current piece the player is controlling by one unit.
    /// </summary>
    /// <returns>Function is called on a loop based on the 'dropTime' variable.</returns>
    IEnumerator DropCurPiece()
    {     
            MoveCurPiece(Vector2Int.down);
            yield return new WaitForSeconds(dropTime);
    }

    /// <summary>
    /// Once the piece is set in it's final location, the coroutine called to repeatedly drop the piece is stopped.
    /// </summary>
    public void PieceSet()
    {
        //if(dropCurPiece == null) { return; }
        StopCoroutine(dropCurPiece);
    }

    /// <summary>
    /// Makes any necessary changes once the game has ended.
    /// </summary>
    public void GameOver()
    {
        MenuEngine.instance.GameOver = true;
    }

    /// <summary>
    /// Removes the specified piece from the list of current pieces in the game.
    /// </summary>
    /// <param name="pieceToRem">Game Object of the Tetris piece to be removed.</param>
    public void RemovePiece(GameObject pieceToRem)
    {
        piecesInGame.Remove(pieceToRem);
    }

    /// <summary>
    /// Makes any necessary changes when destroying a piece.
    /// </summary>
    void DestroyPiece()
    {
        PieceController curPC = pieceToDestroy.GetComponent<PieceController>();
        Vector2Int[] tileCoords = curPC.GetTileCoords();
        RemovePiece(pieceToDestroy);
        Destroy(pieceToDestroy);
        BoardController.instance.PieceRemoved(tileCoords);
    }
    public static List<T> Shuffle<T>(List<T> _list)
    {
        for (int i = 0; i < _list.Count; i++)
        {
            T temp = _list[i];
            int randomIndex = UnityEngine.Random.Range(i, _list.Count);
            _list[i] = _list[randomIndex];
            _list[randomIndex] = temp;
        }

        return _list;
    }

    public void ExecuteHold()
    {
        executedHold = true;
        int increaseByLevel = GameEngine.instance.level >= 600 && GameEngine.instance.nextibmblocks == GameEngine.instance.nextPieces + 1 ? 14 : 0;
        int RSint = GameEngine.instance.RS == RotationSystems.ARS ? 7 : 0;
        if(piecemovementlocked == false && allowHold == true)
        {
            allowHold = false;
            if (pieceHold == 28)
            {
                Destroy(piecesInGame[piecesInGame.Count-1]);
                piecesInGame.RemoveAt(piecesInGame.Count-1);
                SpawnHoldPiece(bag[pieces]);
                pieceHold = bag[pieces-1] + increaseByLevel + RSint;
                pieces++;
                NextPiece();
                executedHold = false;
            }
            else
            {
                Destroy(piecesInGame[piecesInGame.Count-1]);
                piecesInGame.RemoveAt(piecesInGame.Count-1);
                holdPieceUI[pieceHold].SetActive(false);
                SpawnHoldPiece(pieceHold%7);
                pieceHold = bag[pieces-1] + increaseByLevel + RSint;
                NextPiece();
                executedHold = false;
            }
        }
    }
    /// <summary>
    /// Spawns a new Tetris piece.
    /// </summary>
    public void SpawnPiece(int id)
    {
        gravityTiles = 0.0f;
        if (GameEngine.instance.gravity >= 19.99999)
        {
            gravityTiles = 22.0f;
        }
        pieces++;
        if(GameEngine.instance.comboKeepCounter > 0)GameEngine.instance.comboKeepCounter--;
        allowHold = true;
        IHSexecuted = false;
        NextPiece();
        GameObject localGO = GameObject.Instantiate(piecePrefab, transform);
        curPiece = localGO;
        PieceType randPiece = (PieceType)id;
        curPieceController = curPiece.GetComponent<PieceController>();
        curPieceController.SpawnPiece(randPiece);
        piecemovementlocked = false;
        
        piecesInGame.Add(localGO);

        dropCurPiece = StartCoroutine(DropCurPiece());
    }
    /// <summary>
    /// Spawns a new Tetris piece.
    /// </summary>
    public void SpawnHoldPiece(int id)
    {
        gravityTiles = 0.0f;
        if (GameEngine.instance.gravity >= 19.99999)
        {
            gravityTiles = 22.0f;
        }
        GameObject localGO = GameObject.Instantiate(piecePrefab, transform);
        curPiece = localGO;
        PieceType randPiece = (PieceType)id;
        curPieceController = curPiece.GetComponent<PieceController>();
        curPieceController.SpawnPiece(randPiece);
        piecemovementlocked = false;
        if ((GameEngine.instance.Inputs[2] || GameEngine.instance.Inputs[6]) && (!GameEngine.instance.Inputs[1]) && (!GameEngine.instance.Inputs[3])) {IRSCW = true; IRSCCW = false; IRSUD = false;}
        else if ((!GameEngine.instance.Inputs[2] && !GameEngine.instance.Inputs[6]) && (GameEngine.instance.Inputs[1]) && !(GameEngine.instance.Inputs[3])) {IRSCCW = true; IRSCW = false; IRSUD = false;}
        else if ((!GameEngine.instance.Inputs[2] && !GameEngine.instance.Inputs[6]) && (!GameEngine.instance.Inputs[1]) && (GameEngine.instance.Inputs[3])) {IRSCCW = false; IRSCW = false; IRSUD = true;}
        else {IRSCCW = false; IRSCW = false; IRSUD = false;}
        if (GameEngine.instance.RS == RotationSystems.ARS) IARS = true;
        
        piecesInGame.Add(localGO);

        dropCurPiece = StartCoroutine(DropCurPiece());
    }

    public void SpawnDebug(int id)
    {
        GameObject localGO = GameObject.Instantiate(piecePrefab, transform);
        curPiece = localGO;
        PieceType randPiece = (PieceType)id;
        curPieceController = curPiece.GetComponent<PieceController>();
        curPieceController.SpawnPiece(randPiece);

        piecesInGame.Add(localGO);
    }

    /// <summary>
    /// Moves the current piece controlled by the player.
    /// </summary>
    /// <param name="movement">X,Y amount the piece should be moved by</param>
    public void MoveCurPiece(Vector2Int movement)
    {
        if(curPiece == null)
        {
            return;
        }
        curPieceController.MovePiece(movement);
    }
}
