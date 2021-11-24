﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
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
    public GameObject minoBlock;
    public List<int2[]> minoPositions = new List<int2[]>
    {
        new int2[] {int2.zero, new int2(1,0), new int2(1,1), new int2(0,1)}, // O piece
        new int2[] {int2.zero, new int2(-1,0), new int2(2,0), new int2(1,0)}, // I piece
        new int2[] {int2.zero, new int2(-1,0), new int2(1,1), new int2(0,1)}, // S piece
        new int2[] {int2.zero, new int2(0,1), new int2(-1, 1), new int2(1,0)}, // Z piece
        new int2[] {int2.zero, new int2(-1,0), new int2(1,1), new int2(1,0)}, // L piece
        new int2[] {int2.zero, new int2(-1,0), new int2(-1, 1), new int2(1,0)}, // J piece
        new int2[] {int2.zero, new int2(-1,0), new int2(0,1), new int2(1,0)}, // T piece

        //big pieces
        
        bigPiece(bigMino(0,0), bigMino(1,0), bigMino(1,1), bigMino(0,1)),
        bigPiece(bigMino(0,0), bigMino(-1,0), bigMino(2,0), bigMino(1,0)),
        bigPiece(bigMino(0,0), bigMino(-1,0), bigMino(1,1), bigMino(0,1)),
        bigPiece(bigMino(0,0), bigMino(0,1), bigMino(-1,1), bigMino(1,0)),
        bigPiece(bigMino(0,0), bigMino(-1,0), bigMino(1,1), bigMino(1,0)),
        bigPiece(bigMino(0,0), bigMino(-1,0), bigMino(-1,1), bigMino(1,0)),
        bigPiece(bigMino(0,0), bigMino(-1,0), bigMino(0,1), bigMino(1,0)),

        
        // new int2[] {int2.zero},
        // new int2[] {int2.zero},
        // new int2[] {int2.zero},
        // new int2[] {int2.zero},
        // new int2[] {int2.zero},
        // new int2[] {int2.zero},
        // new int2[] {int2.zero},
    };
    public List<Vector2> pivotPositions = new List<Vector2>
    {
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, -0.5f),
        new Vector2(0f, 0f),
        new Vector2(0f, 0f),
        new Vector2(0f, 0f),
        new Vector2(0f, 0f),
        new Vector2(0f, 0f),
        new Vector2(1.5f, 1.5f),
        new Vector2(1.5f, -0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),
        new Vector2(0.5f, 0.5f),

    };
    private static int2[] bigMino(int posX, int posY)
    {
        int2 pos = new int2(posX, posY);
        return new int2[] {int2.zero + pos * 2, new int2(1,0) + pos * 2, new int2(1,1) + pos * 2, new int2(0,1) + pos * 2};
    }
    private static int2[] bigPiece(params int2[][] positions)
    {
        int arraySize = positions.Length * 4;
        int2[] result = new int2[arraySize];
        for (int i = 0; i < positions.Length; i++)
        {
            result[i * 4] = positions[i][0];
            result[i * 4 + 1] = positions[i][1];
            result[i * 4 + 2] = positions[i][2];
            result[i * 4 + 3] = positions[i][3];
        }

        return result;
    }
    public int2 scaling;
    public int2[,] JLSTZ_OFFSET_DATA { get; private set; }
    public int2[,] I_OFFSET_DATA { get; private set; }
    public int2[,] O_OFFSET_DATA { get; private set; }
    public List<GameObject> piecesInGame;
    public GameObject pieceToDestroy = null;
    public GameObject sacText, gameOverText;
    public bool piecemovementlocked = false;
    public List<int> bag;
    public float gravityTiles;

    public AudioSource gameAudio;
    public AudioClip[] nextpieceSE;
    public AudioClip audioIRS, audioIHS, bell, levelup, holdSE;
    [SerializeField]
    int2 relativeHoldPieceCoordinate;

    [SerializeField] 
    GameObject holdPieceBuffer;

    [SerializeField] 
    public List<int2> relativeNextPieceCoordinates;

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
        // UnityEngine.Random.InitState(SeedManager.seed);
        // holdPieceBuffer = new GameObject();
        GameObject newPrefab = Instantiate(piecePrefab, transform);
        newPrefab.SetActive(false);
        piecePrefab = newPrefab;
        piecePrefab.GetComponent<PieceController>().board = board;
        
        JLSTZ_OFFSET_DATA = new int2[5, 4];
        JLSTZ_OFFSET_DATA[0, 0] = int2.zero;
        JLSTZ_OFFSET_DATA[0, 1] = int2.zero;
        JLSTZ_OFFSET_DATA[0, 2] = int2.zero;
        JLSTZ_OFFSET_DATA[0, 3] = int2.zero;

        JLSTZ_OFFSET_DATA[1, 0] = int2.zero;
        JLSTZ_OFFSET_DATA[1, 1] = new int2(1,0);
        JLSTZ_OFFSET_DATA[1, 2] = int2.zero;
        JLSTZ_OFFSET_DATA[1, 3] = new int2(-1, 0);

        JLSTZ_OFFSET_DATA[2, 0] = int2.zero;
        JLSTZ_OFFSET_DATA[2, 1] = new int2(1, -1);
        JLSTZ_OFFSET_DATA[2, 2] = int2.zero;
        JLSTZ_OFFSET_DATA[2, 3] = new int2(-1, -1);

        JLSTZ_OFFSET_DATA[3, 0] = int2.zero;
        JLSTZ_OFFSET_DATA[3, 1] = new int2(0, 2);
        JLSTZ_OFFSET_DATA[3, 2] = int2.zero;
        JLSTZ_OFFSET_DATA[3, 3] = new int2(0, 2);

        JLSTZ_OFFSET_DATA[4, 0] = int2.zero;
        JLSTZ_OFFSET_DATA[4, 1] = new int2(1, 2);
        JLSTZ_OFFSET_DATA[4, 2] = int2.zero;
        JLSTZ_OFFSET_DATA[4, 3] = new int2(-1, 2);

        I_OFFSET_DATA = new int2[5, 4];
        I_OFFSET_DATA[0, 0] = int2.zero;
        I_OFFSET_DATA[0, 1] = int2.zero;
        I_OFFSET_DATA[0, 2] = int2.zero;
        I_OFFSET_DATA[0, 3] = int2.zero;

        I_OFFSET_DATA[1, 0] = int2.zero;
        I_OFFSET_DATA[1, 1] = new int2(1,0) * 2;
        I_OFFSET_DATA[1, 2] = new int2(1,0) * 3;
        I_OFFSET_DATA[1, 3] = new int2(-1,0);

        I_OFFSET_DATA[2, 0] = new int2(1,0) * 2;
        I_OFFSET_DATA[2, 1] = new int2(-1,0);
        I_OFFSET_DATA[2, 2] = new int2(1, 0);
        I_OFFSET_DATA[2, 3] = new int2(0, 0);

        I_OFFSET_DATA[3, 0] = new int2(-1,0); // 0 to 1: -1,0 - 1,1 = -2,-1
        I_OFFSET_DATA[3, 1] = new int2(1, 1); // 1 to 2: 1,1 - 2,-1 = -1,2
        I_OFFSET_DATA[3, 2] = new int2(2, -1); // 2 to 3: 2,-1 - 0,-2 = 2,1
        I_OFFSET_DATA[3, 3] = new int2(0, -2); // 3 to 0: 0,-2 - -1,0 = 1,-2

        // 2,0 - 0,1 = 2,-1
        // 0,1 - 2,0 = -2,1
        I_OFFSET_DATA[4, 0] = new int2(1,0) * 2; // 0 to 1: 2,0 - 0,-2 = 2,2
        I_OFFSET_DATA[4, 1] = new int2(0, -2); // 1 to 2: 0,-2 - -2,0 = 2,-2
        I_OFFSET_DATA[4, 2] = new int2(-2, 0); // 2 to 3: -2,0 - 0,2 = -2,-2
        I_OFFSET_DATA[4, 3] = new int2(0, 2); // 3 to 0: 0,2 - 2,0 = -2, 2

        O_OFFSET_DATA = new int2[5, 4];
        O_OFFSET_DATA[0, 0] = int2.zero;
        O_OFFSET_DATA[0, 1] = int2.zero;
        O_OFFSET_DATA[0, 2] = int2.zero;
        O_OFFSET_DATA[0, 3] = int2.zero;

        O_OFFSET_DATA[1, 0] = int2.zero;
        O_OFFSET_DATA[1, 1] = int2.zero;
        O_OFFSET_DATA[1, 2] = int2.zero;
        O_OFFSET_DATA[1, 3] = int2.zero;

        O_OFFSET_DATA[2, 0] = int2.zero;
        O_OFFSET_DATA[2, 1] = int2.zero;
        O_OFFSET_DATA[2, 2] = int2.zero;
        O_OFFSET_DATA[2, 3] = int2.zero;

        O_OFFSET_DATA[3, 0] = int2.zero;
        O_OFFSET_DATA[3, 1] = int2.zero;
        O_OFFSET_DATA[3, 2] = int2.zero;
        O_OFFSET_DATA[3, 3] = int2.zero;

        O_OFFSET_DATA[4, 0] = int2.zero;
        O_OFFSET_DATA[4, 1] = int2.zero;
        O_OFFSET_DATA[4, 2] = int2.zero;
        O_OFFSET_DATA[4, 3] = int2.zero;


    }

    /// <summary>
    /// Called at the first frame instance is enabled. Sets some variables.
    /// </summary>
    private void Start()
    {
        piecesInGame = new List<GameObject>();
        availablePieces = new List<GameObject>();
        if(board.RS == RotationSystems.ARS)
        {
            JLSTZ_OFFSET_DATA = new int2[5, 4];
            JLSTZ_OFFSET_DATA[0, 0] = new int2(0,1);
            JLSTZ_OFFSET_DATA[0, 1] = int2.zero;
            JLSTZ_OFFSET_DATA[0, 2] = int2.zero;
            JLSTZ_OFFSET_DATA[0, 3] = int2.zero;

            JLSTZ_OFFSET_DATA[1, 0] = new int2(-1, 1);
            JLSTZ_OFFSET_DATA[1, 1] = new int2(0, 0);
            JLSTZ_OFFSET_DATA[1, 2] = new int2(-1, 0);
            JLSTZ_OFFSET_DATA[1, 3] = new int2(-2, 0);

            JLSTZ_OFFSET_DATA[2, 0] = new int2(-1, 1);
            JLSTZ_OFFSET_DATA[2, 1] = new int2(-2, 0);
            JLSTZ_OFFSET_DATA[2, 2] = new int2(-1, 0);
            JLSTZ_OFFSET_DATA[2, 3] = new int2(0, 0);

            JLSTZ_OFFSET_DATA[3, 0] = new int2(0, 0);
            JLSTZ_OFFSET_DATA[3, 1] = new int2(0, 0);
            JLSTZ_OFFSET_DATA[3, 2] = new int2(0, 0);
            JLSTZ_OFFSET_DATA[3, 3] = new int2(0, 0);

            JLSTZ_OFFSET_DATA[4, 0] = int2.zero;
            JLSTZ_OFFSET_DATA[4, 1] = int2.zero;
            JLSTZ_OFFSET_DATA[4, 2] = int2.zero;
            JLSTZ_OFFSET_DATA[4, 3] = int2.zero;

            I_OFFSET_DATA = new int2[5, 4];
            I_OFFSET_DATA[0, 0] = int2.zero;
            I_OFFSET_DATA[0, 1] = new int2(0, 0);
            I_OFFSET_DATA[0, 2] = new int2(0, -1);
            I_OFFSET_DATA[0, 3] = new int2(-1, 0);

            I_OFFSET_DATA[1, 0] = new int2(-1, 0);
            I_OFFSET_DATA[1, 1] = int2.zero;
            I_OFFSET_DATA[1, 2] = new int2(1, 1);
            I_OFFSET_DATA[1, 3] = new int2(0, 1);

            I_OFFSET_DATA[2, 0] = new int2(2, 0);
            I_OFFSET_DATA[2, 1] = int2.zero;
            I_OFFSET_DATA[2, 2] = new int2(-2, 1);
            I_OFFSET_DATA[2, 3] = new int2(0, 1);

            I_OFFSET_DATA[3, 0] = new int2(-1, 0);
            I_OFFSET_DATA[3, 1] = new int2(0, 1);
            I_OFFSET_DATA[3, 2] = new int2(1, 0);
            I_OFFSET_DATA[3, 3] = new int2(0, -1);

            I_OFFSET_DATA[4, 0] = new int2(2, 0);
            I_OFFSET_DATA[4, 1] = new int2(0, -2);
            I_OFFSET_DATA[4, 2] = new int2(-2, 0);
            I_OFFSET_DATA[4, 3] = new int2(0, 2);

            O_OFFSET_DATA = new int2[5, 4];
            O_OFFSET_DATA[0, 0] = int2.zero;
            O_OFFSET_DATA[0, 1] = int2.zero;
            O_OFFSET_DATA[0, 2] = int2.zero;
            O_OFFSET_DATA[0, 3] = int2.zero;

            O_OFFSET_DATA[1, 0] = int2.zero;
            O_OFFSET_DATA[1, 1] = int2.zero;
            O_OFFSET_DATA[1, 2] = int2.zero;
            O_OFFSET_DATA[1, 3] = int2.zero;

            O_OFFSET_DATA[2, 0] = int2.zero;
            O_OFFSET_DATA[2, 1] = int2.zero;
            O_OFFSET_DATA[2, 2] = int2.zero;
            O_OFFSET_DATA[2, 3] = int2.zero;

            O_OFFSET_DATA[3, 0] = int2.zero;
            O_OFFSET_DATA[3, 1] = int2.zero;
            O_OFFSET_DATA[3, 2] = int2.zero;
            O_OFFSET_DATA[3, 3] = int2.zero;

            O_OFFSET_DATA[4, 0] = int2.zero;
            O_OFFSET_DATA[4, 1] = int2.zero;
            O_OFFSET_DATA[4, 2] = int2.zero;
            O_OFFSET_DATA[4, 3] = int2.zero;
        }
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
                    board.LockDelayf = 0;
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
                        if(piecemovementlocked == false)MoveCurPiece(new int2(0,-1));
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
        int extraPiece = holdPieceBuffer != null ? 2 : 1;
        if (!IHSexecuted)
        {
            gameAudio.PlayOneShot(nextpieceSE[bag[lockedPieces+extraPiece]]);
        }
        if(board.level % board.sectionSize < board.sectionSize - 1 && board.level < board.endingLevel && lockedPieces > 0 && allowHold == true)
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

    //Transitioning to dynamic timing
    // void Update()
    // {
    //     if(board.framestepped && !board.GameOver && nextpiecequeued)
    //     board.AREf += Time.deltaTime * 100;
    // }
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
                    board.LockDelayf = 0;
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
            if (board.movement.x < -deadzone)
            {
                DASfl++;
                if(!piecemovementlocked)
                {
                    if (DASfl == 1) MoveCurPiece(new int2(-1,0));
                    if (DASfl > DAStuning && (ARRtuning == 0 || DASfl % ARRtuning == 0))
                    {
                        MoveCurPiece(new int2(-1,0));
                        if (ARRtuning == 0)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                MoveCurPiece(new int2(-1,0));
                            }
                        }
                    }
                }
            }
            else DASfl = 0;
            if (board.movement.x > deadzone)
            {
                DASfr++;
                if(!piecemovementlocked)
                {
                    if (DASfr == 1) MoveCurPiece(new int2(1,0));
                    if (DASfr > DAStuning && (ARRtuning == 0 || DASfr % ARRtuning == 0))
                    {
                        MoveCurPiece(new int2(1,0));
                        if (ARRtuning == 0)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                MoveCurPiece(new int2(1,0));
                            }
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
                MenuEngine.instance.yourPlayer.GameOver = true;
                MenuEngine.instance.yourPlayer.IntentionalGameOver = true;
                if(MenuEngine.instance.yourPlayer.lives == 1)MenuEngine.instance.yourPlayer.frames = 300;
                else curPieceController.SetPiece();
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
                curPieceController.RotatePiece(true, true, true, true);
                if (IRSUD)
                {
                    gameAudio.PlayOneShot(audioIRS);
                }
            }
            // if (IARS && !piecemovementlocked) curPieceController.RotatePiece(true, false, true);


            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                GameEngine.debugMode = !GameEngine.debugMode;
            }
            if(curPieceController != null) 
            {
                if (!curPieceController.CanMovePiece(int2.zero) && !piecemovementlocked) curPieceController.SendPieceToFloor();
                if(piecemovementlocked == false) while (gravityTiles >= 1)
                {
                    if (!curPieceController.CanMovePiece(new int2(0,-1)))
                    {
                        gravityTiles = 0;
                    }
                    else
                    {
                        MoveCurPiece(new int2(0,-1));
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
        int incrBigPiece = board.bigMode ? 7 : 0;
        localpiecectrl.SpawnPiece((PieceType)id, this, minoPositions[id + incrBigPiece], pivotPositions[id + incrBigPiece], minoBlock, scaling, id, relativeNextPieceCoordinates[nextPiecesBuffer.Count]);
        localpiecectrl.isPieceIsInNextQueue = true;
        nextPiecesBuffer.Add(localGO);
        if(GameEngine.debugMode) Debug.Log(nextPiecesBuffer.Count + " | " + (relativeNextPieceCoordinates.Count-1));
        if(board.RS == RotationSystems.ARS) localpiecectrl.RotatePiece(true, false, true, false);
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
        gravityTiles = 1.0f;
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
    /// Swaps a piece.
    /// </summary>
    public void SpawnHoldPiece()
    {
        if(!IHSexecuted) AudioManager.PlayClip(holdSE);
        curPieceController.isPieceIsInNextQueue = true;
        if(curPieceController.ghostContr != null)curPieceController.ghostContr.gameObject.SetActive(false);
        if(curPieceController.rotationIndex == 2) curPieceController.RotatePiece(true, true, true);
        if(curPieceController.rotationIndex % 2 == 1) curPieceController.RotatePiece(curPieceController.rotationIndex / 2 == 1, false, false);
        curPieceController.ForcefullyMovePiece(relativeHoldPieceCoordinate - V3ToV2Int(curPieceController.tiles[0].transform.localPosition));
        gravityTiles = 1.0f;
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
        curPieceController.MovePiece(relativeNextPieceCoordinates[0] - V3ToV2Int(curPieceController.tiles[0].transform.localPosition), false);
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

    int2 V3ToV2Int(Vector3 vector3)
    {
        return new int2((int)vector3.x, (int)vector3.y);
    }
    /// <summary>
    /// Moves the current piece controlled by the player.
    /// </summary>
    /// <param name="movement">X,Y amount the piece should be moved by</param>
    public void MoveCurPiece(int2 movement)
    {
        if (curPiece != null && curPieceController != null)
        {
            curPieceController.MovePiece(movement, false);
            if(board.bigMode)curPieceController.MovePiece(movement, false);
        }
    }
}
