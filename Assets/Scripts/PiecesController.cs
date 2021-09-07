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


    public NetworkBoard board;

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
    [SerializeField]
    Vector2Int relativeHoldPieceCoordinate;

    [SerializeField] 
    GameObject holdPieceBuffer;

    [SerializeField] 
    public List<Vector2Int> relativeNextPieceCoordinates;

    [SerializeField] 
    List<GameObject> nextPiecesBuffer;
    // public GameObject[] nextPieceUI;
    // public GameObject[] nextIBMWPieceUI;
    // public GameObject[] nextARSPieceUI;
    // public GameObject[] nextIBMPieceUI;
    // public GameObject[] holdPieceUI;
    //{ Up, CW, CCW, UD, Hold }
    public bool[] PrevInputs;
    

    private bool executedHold = false;
    public bool allowHold = true;
    private int frames;
    private int DASfl, DASfr;
    public int ARRtuning = 1;
    private int DAStuning;

    private bool IRSCW, IRSCCW, IRSUD, IARS, IHS;

    private bool nextpiecequeued = false;
    private bool audioBellPlayed = false;
    public int pieces, lockedPieces;

    public double deadzone = 0.5;
    

    GameObject curPiece = null;
    public PieceController curPieceController = null;
    List<GameObject> availablePieces;


    public bool IsHoldEmpty()
    {
        return holdPieceBuffer == null;
    }
    /// <summary>
    /// Called as soon as the instance is enabled. Sets the singleton and offset data arrays.
    /// </summary>
    private void Awake()
    {
        // holdPieceBuffer = new GameObject();
        GameObject newPrefab = Instantiate(piecePrefab, transform);
        newPrefab.SetActive(false);
        piecePrefab = newPrefab;
        piecePrefab.GetComponent<PieceController>().board = board;
        
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

        if(board.RS == RotationSystems.ARS)
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

    public void InitiatePieces()
    {
        nextPiecesBuffer = new List<GameObject>();
        bag = new List<int>();
        for (int i = 0; i < 16; i++)
        {
            // bag.Add(new List<int> { 0, 1, 2, 3, 4, 5, 6 });
            
            List<int> bagshuff = new List<int>(){0,1,2,3,4,5,6};
            Shuffle(bagshuff);
            if(GameEngine.debugMode) Debug.Log(bagshuff);
            foreach (var piece in bagshuff)
            {
                bag.Add(piece);
            }
        }
        for (int i = 0; i < relativeNextPieceCoordinates.Count -1; i++)
        {
            SpawnNextPiece(bag[i]);
        }
        UpdatePieceBag();
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
                if((board.ARE < 1 && (!board.ending || board.AREf >= 0)) || executedHold == true)
                {
                    board.lineClonePiecesLeft--;
                    if (board.lineClonePiecesLeft == 0)
                    {
                        board.lineClonePiecesLeft = board.lineClonePerPiece[board.curSect];
                        board.boardController.CloneLineToBottom();
                    }
                    else if (board.lineClonePiecesLeft > board.lineClonePerPiece[board.curSect])
                    {
                        board.lineClonePiecesLeft = board.lineClonePerPiece[board.curSect];
                    }
                    SpawnPiece();
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
                if(GameEngine.debugMode) Debug.Log("Next random num: " + bag[pieces]);
                executedHold = false;
            }
        }
        if (pieces % 7 == 0)
        {
            List<int> bagshuff = new List<int>(){0,1,2,3,4,5,6};
            Shuffle(bagshuff);
            if(GameEngine.debugMode) Debug.Log(bagshuff);
            for (int j = 0; j < 7; j++)
            {
                bag.Add(bagshuff[j]);
            }
        }
        bagPieceRetrieved = false;
    }
    bool IHSexecuted;
    // public void UpdateShownPieces()
    // {
    //     int nxtibmPieces = (board.nextibmblocks > board.nextPieces)  ? board.nextPieces : board.nextibmblocks;
    //     for (int i = 0; i < board.nextPieces; i++)
    //     {
    //         if (board.RS == RotationSystems.SRS)
    //         {
    //             if(board.level < 600)
    //             {
    //                 if(pieces > 0)nextPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
    //                 nextPieceUI[i*7+bag[i+pieces]].SetActive(true);
    //             }
    //             else
    //             {
    //                 if(pieces > 0)nextPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
    //                 if(pieces > 0)nextIBMWPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
    //                 if(i>= board.nextPieces - nxtibmPieces)nextIBMWPieceUI[i*7+bag[i+pieces]].SetActive(true);
    //                 else nextPieceUI[i*7+bag[i+pieces]].SetActive(true);
    //             }
    //         }
    //         else if (board.RS == RotationSystems.ARS)
    //         {
    //             if(board.level < 600)
    //             {
    //                 if(pieces > 0)nextARSPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
    //                 nextARSPieceUI[i*7+bag[i+pieces]].SetActive(true);
    //             }
    //             else
    //             {
    //                 if(pieces > 0)nextARSPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
    //                 if(pieces > 0)nextIBMPieceUI[i*7+bag[i+pieces-1]].SetActive(false);
    //                 if(i>= board.nextPieces - nxtibmPieces)nextIBMPieceUI[i*7+bag[i+pieces]].SetActive(true);
    //                 else nextARSPieceUI[i*7+bag[i+pieces]].SetActive(true);
    //             }
    //         }
    //     }
    //     if (pieceHold < 28)holdPieceUI[pieceHold].SetActive(true);
    // }
    private void NextPiece()
    {
        if (allowHold == true)
        {
            SpawnNextPiece(bag[pieces]);
            if (board.level >= 600 && board.nextibmblocks < board.nextPieces + 1)
            {
                board.nextibmblocks++;
            }
        }
        else if(holdPieceBuffer == null) 
            SpawnNextPiece(bag[pieces]);
        // UpdateShownPieces();
        if (!IHSexecuted)
        {
            if(bag[lockedPieces] == 0)
            {
                gameAudio.PlayOneShot(nextpiece2);
            }
            if(bag[lockedPieces] == 1)
            {
                gameAudio.PlayOneShot(nextpiece1);
            }
            if(bag[lockedPieces] == 2)
            {
                gameAudio.PlayOneShot(nextpiece6);
            }
            if(bag[lockedPieces] == 3)
            {
                gameAudio.PlayOneShot(nextpiece3);
            }
            if(bag[lockedPieces] == 4)
            {
                gameAudio.PlayOneShot(nextpiece7);
            }
            if(bag[lockedPieces] == 5)
            {
                gameAudio.PlayOneShot(nextpiece5);
            }
            if(bag[lockedPieces] == 6)
            {
                gameAudio.PlayOneShot(nextpiece4);
            }
        }
        if(board.level % 100 < 99 && board.level < 2100 && lockedPieces > 0 && allowHold == true)
        {
            board.level++;
            audioBellPlayed = false;
        }
        else if(audioBellPlayed == false && lockedPieces > 1 && allowHold == true)
        {
            audioBellPlayed = true;
            gameAudio.PlayOneShot(bell);
        }
        IHSexecuted = false;
    }

    /// <summary>
    /// Called once every frame. Checks for player input.
    /// </summary>
    void FixedUpdate()
    {
        if (board.lineClonePiecesLeft == 0)
        {
            board.lineClonePiecesLeft = board.lineClonePerPiece[board.curSect];
            board.boardController.CloneLineToBottom();
        }
        else if (board.lineClonePiecesLeft > board.lineClonePerPiece[board.curSect])
        {
            board.lineClonePiecesLeft = board.lineClonePerPiece[board.curSect];
        }

        DAStuning = (int)board.DAS;
        if (board.framestepped && !board.GameOver)
        {
            frames++;
            if(piecemovementlocked == false)gravityTiles += board.gravity;
            if (nextpiecequeued == true)
            {
                board.AREf++;
                if (board.AREf >= (int)Math.Floor(board.ARE))
                {
                    nextpiecequeued = false;
                    board.lineClonePiecesLeft--;
                    if (board.lineClonePiecesLeft == 0)
                    {
                        board.lineClonePiecesLeft = board.lineClonePerPiece[board.curSect];
                        board.boardController.CloneLineToBottom();
                    }
                    else if (board.lineClonePiecesLeft > board.lineClonePerPiece[board.curSect])
                    {
                        board.lineClonePiecesLeft = board.lineClonePerPiece[board.curSect];
                    }
                    SpawnPiece();
                    board.AREf = 0;
                }
                if ((board.Inputs[2] || board.Inputs[6]) && (!board.Inputs[1]) && (!board.Inputs[3])) {IRSCW = true; IRSCCW = false; IRSUD = false;}
                else if ((!board.Inputs[2] && !board.Inputs[6]) && (board.Inputs[1]) && !(board.Inputs[3])) {IRSCCW = true; IRSCW = false; IRSUD = false;}
                else if ((!board.Inputs[2] && !board.Inputs[6]) && (!board.Inputs[1]) && (board.Inputs[3])) {IRSCCW = false; IRSCW = false; IRSUD = true;}
                else {IRSCCW = false; IRSCW = false; IRSUD = false;}
                if (board.Inputs[4]) IHS = true;
                else IHS = false;
                if (board.RS == RotationSystems.ARS) IARS = true;
            }
            else {IRSCCW = false; IRSCW = false; IRSUD = false; IARS = false; IHS = false;}
            if (board.movement.y < -deadzone && !piecemovementlocked)
            {
                gravityTiles += (float)board.gravity * (float)board.SDF * (board.movement.y * -1);
            }
            if(!piecemovementlocked && curPieceController != null)
            {
                if (board.movement.x < -deadzone)
                {
                    if (DASfl == 0) MoveCurPiece(Vector2Int.left);
                    DASfl++;
                    if (DASfl > DAStuning && (ARRtuning == 0 || DASfl % ARRtuning == 0))
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
                if (board.movement.x > deadzone)
                {
                    if (DASfr == 0) MoveCurPiece(Vector2Int.right);
                    DASfr++;
                    if (DASfr > DAStuning && (ARRtuning == 0 || DASfr % ARRtuning == 0))
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
            }
            // if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            // {
            //     if(curPieceController != null)
            //     {
            //         return;
            //     }
            //     UpdatePieceBag();
            // }
            if (Input.GetKeyDown(KeyCode.Escape)){
                MenuEngine.instance.yourPlayer.GameOver = true;
                MenuEngine.instance.yourPlayer.IntentionalGameOver = true;
                MenuEngine.instance.yourPlayer.frames = 300;
            }

            if (((board.Inputs[4] && !PrevInputs[4]) || IHS) && !piecemovementlocked && allowHold)
            {
                if (IHS)
                {
                    gameAudio.PlayOneShot(audioIHS);
                    IHSexecuted = true;
                }
                ExecuteHold();
            }
            if (((board.Inputs[2] && !PrevInputs[2]) || (board.Inputs[6] && !PrevInputs[6]) || IRSCW) && !piecemovementlocked)
            {
                curPieceController.RotatePiece(true, true, false);
                if (IRSCW)
                {
                    gameAudio.PlayOneShot(audioIRS);
                }
            }
            if (((board.Inputs[1] && !PrevInputs[1]) || IRSCCW) && !piecemovementlocked)
            {
                curPieceController.RotatePiece(false, true, false);
                if (IRSCCW)
                {
                    gameAudio.PlayOneShot(audioIRS);
                }
            }
            if (((board.Inputs[3] && !PrevInputs[3]) || IRSUD) && !piecemovementlocked)
            {
                curPieceController.RotatePiece(true, false, true);
                if (IRSUD)
                {
                    gameAudio.PlayOneShot(audioIRS);
                }
            }
            if (IARS && !piecemovementlocked) curPieceController.RotatePiece(true, false, true);


            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                GameEngine.debugMode = !GameEngine.debugMode;
            }
            if(curPieceController != null) 
            {
                if (!curPieceController.CanMovePiece(Vector2Int.zero) && !piecemovementlocked) curPieceController.SendPieceToFloor();
                if(piecemovementlocked == false) while (gravityTiles >= 1)
                {
                    if (!curPieceController.CanMovePiece(Vector2Int.down))
                    {
                        gravityTiles = 0;
                    }
                    else
                    {
                        MoveCurPiece(Vector2Int.down);
                        gravityTiles--;
                    }
                }
            }
            if (board.Inputs[0] && !PrevInputs[0] && !piecemovementlocked)
            {
                curPieceController.SendPieceToFloor();
            }
            for (int i = 1; i < 7; i++)
            {
                PrevInputs[i] = board.Inputs[i];
            }
            if (!piecemovementlocked || PrevInputs[0]) PrevInputs[0] = board.Inputs[0];
        }
    }

    /// <summary>
    /// Makes any necessary changes once the game has ended.
    /// </summary>
    public void GameOver()
    {
        board.GameOver = true;
    }

    /// <summary>
    /// Removes the specified piece from the list of current pieces in the game.
    /// </summary>
    /// <param name="pieceToRem">Game Object of the Tetris piece to be removed.</param>
    public void RemovePiece(GameObject pieceToRem)
    {
        piecesInGame.Remove(pieceToRem);
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
        if(piecemovementlocked == false && allowHold == true)
        {
            allowHold = false;
            SpawnHoldPiece();
            executedHold = false;
        }
    }
    public void SpawnNextPiece(int id)
    {
        GameObject localGO = GameObject.Instantiate(piecePrefab, transform);
        localGO.SetActive(true);
        localGO.name = "Piece " + pieces + ": " + (PieceType)id;
        PieceController localpiecectrl = localGO.GetComponent<PieceController>();
        localpiecectrl.ghostContr.gameObject.SetActive(false);
        localpiecectrl.SpawnPiece((PieceType)id, this, relativeNextPieceCoordinates[nextPiecesBuffer.Count]);
        localpiecectrl.isPieceIsInNextQueue = true;
        nextPiecesBuffer.Add(localGO);
        Debug.Log(nextPiecesBuffer.Count + " | " + (relativeNextPieceCoordinates.Count-1));
        if(nextPiecesBuffer.Count > 0)for (int i = 0; i < nextPiecesBuffer.Count; i++)
        {
            nextPiecesBuffer[i].SetActive(i <= board.nextPieces);
        }
        if (nextPiecesBuffer.Count > relativeNextPieceCoordinates.Count-1)
        {
            nextPiecesBuffer.RemoveAt(0);
            for (int index = 0; index < relativeNextPieceCoordinates.Count -1; index++)
            {
                nextPiecesBuffer[index].GetComponent<PieceController>().ForcefullyMovePiece(relativeNextPieceCoordinates[index] - relativeNextPieceCoordinates[index+1]);
                // nextPiecesBuffer[index].transform.localPosition = (Vector2)relativeNextPieceCoordinates[index];
            }
        }
        pieces++;
    }
    /// <summary>
    /// Spawns a new Tetris piece.
    /// </summary>
    public void SpawnPiece()
    {
        allowHold = true;
        // while (nextPiecesBuffer.Count < relativeNextPieceCoordinates.Count -1)
        // {
        //     NextPiece();
        //     pieces++;
        // }
        gravityTiles = 0.0f;
        if (board.gravity >= 19.99999)
        {
            gravityTiles = 22.0f;
        }
        // pieces++;
        if(board.comboKeepCounter > 0)board.comboKeepCounter--;
        IHSexecuted = false;
        GameObject localGO = nextPiecesBuffer[0];
        curPiece = localGO;
        // localGO.GetComponent<PieceController>().ghostContr.gameObject.SetActive(false);
        // PieceType randPiece = (PieceType)id;
        curPieceController = curPiece.GetComponent<PieceController>();
        curPieceController.isPieceIsInNextQueue = false;
        if(curPieceController.ghostContr != null)curPieceController.ghostContr.gameObject.SetActive(true);
        // curPieceController.SpawnPiece(randPiece, this);
        piecemovementlocked = false;
        piecesInGame.Add(localGO);
        NextPiece();
    }
    /// <summary>
    /// Spawns a new Tetris piece.
    /// </summary>
    public void SpawnHoldPiece()
    {
        curPieceController.isPieceIsInNextQueue = true;
        if(curPieceController.ghostContr != null)curPieceController.ghostContr.gameObject.SetActive(false);
        if(curPieceController.rotationIndex == 2) curPieceController.RotatePiece180(true, false);
        if(curPieceController.rotationIndex % 2 == 1) curPieceController.RotatePiece(curPieceController.rotationIndex / 2 == 1, false, false);
        curPieceController.ForcefullyMovePiece(relativeHoldPieceCoordinate - curPieceController.tiles[0].coordinates);
        gravityTiles = 0.0f;
        if (board.gravity >= 19.99999)
        {
            gravityTiles = 22.0f;
        }
        GameObject localGO;
        if (holdPieceBuffer != null)
        {
            localGO = holdPieceBuffer;
        }
        else 
        {
            localGO = nextPiecesBuffer[0];
            NextPiece();
        }
        holdPieceBuffer = curPieceController.gameObject;
        curPiece = localGO;
        curPiece.SetActive(true);
        // PieceType randPiece = (PieceType)id;
        curPieceController = curPiece.GetComponent<PieceController>();
        curPieceController.MovePiece(relativeNextPieceCoordinates[0] - curPieceController.tiles[0].coordinates);
        curPieceController.isPieceIsInNextQueue = false;
        if(curPieceController.ghostContr != null)curPieceController.ghostContr.gameObject.SetActive(true);
        // curPieceController.SpawnPiece(randPiece, this);
        piecemovementlocked = false;
        if ((board.Inputs[2] || board.Inputs[6]) && (!board.Inputs[1]) && (!board.Inputs[3])) {IRSCW = true; IRSCCW = false; IRSUD = false;}
        else if ((!board.Inputs[2] && !board.Inputs[6]) && (board.Inputs[1]) && !(board.Inputs[3])) {IRSCCW = true; IRSCW = false; IRSUD = false;}
        else if ((!board.Inputs[2] && !board.Inputs[6]) && (!board.Inputs[1]) && (board.Inputs[3])) {IRSCCW = false; IRSCW = false; IRSUD = true;}
        else {IRSCCW = false; IRSCW = false; IRSUD = false;}
        if (board.RS == RotationSystems.ARS) IARS = true;
        
        piecesInGame.Add(localGO);
    }

    // public void SpawnDebug(int id)
    // {
    //     GameObject localGO = GameObject.Instantiate(piecePrefab, transform);
    //     curPiece = localGO;
    //     PieceType randPiece = (PieceType)id;
    //     curPieceController = curPiece.GetComponent<PieceController>();
    //     curPieceController.SpawnPiece(randPiece, this);

    //     piecesInGame.Add(localGO);
    // }

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
