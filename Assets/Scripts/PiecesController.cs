using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

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

public class PiecesController : MonoBehaviour
{


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
        return new int2[] { int2.zero + pos * 2, new int2(1, 0) + pos * 2, new int2(1, 1) + pos * 2, new int2(0, 1) + pos * 2 };
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
    private int2 relativeHoldPieceCoordinate;

    [SerializeField]
    private int holdPieceTextureID, holdPieceID;
    private bool isHeld = false;
    private PieceType holdPieceType;

    [SerializeField]
    private List<int2> relativeNextPieceCoordinates;
    [SerializeField]
    private float[] nextPieceManagerSizes;

    [SerializeField]
    private GameObject nextPieceManagerPrefab;
    private List<NextPieceManager> nextPieceManagers;
    private NextPieceManager holdPieceManager;
    //{ Up, CW, CCW, UD, Hold }
    public bool4x2 PrevInputs;


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
    private GameObject curPiece = null;
    public PieceController curPieceController = null;
    private List<GameObject> availablePieces;
    [SerializeField] private int[] numberToTextureIDs;

    /// <summary>
    /// Called as soon as the instance is enabled. Sets offset data arrays and object pools.
    /// </summary>
    private void Awake()
    {
        UnityEngine.Random.InitState(SeedManager.seed);
        nextPieceManagers = new List<NextPieceManager>();

        JLSTZ_OFFSET_DATA = new int2[5, 4];
        JLSTZ_OFFSET_DATA[0, 0] = int2.zero;
        JLSTZ_OFFSET_DATA[0, 1] = int2.zero;
        JLSTZ_OFFSET_DATA[0, 2] = int2.zero;
        JLSTZ_OFFSET_DATA[0, 3] = int2.zero;

        JLSTZ_OFFSET_DATA[1, 0] = int2.zero;
        JLSTZ_OFFSET_DATA[1, 1] = new int2(1, 0);
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
        I_OFFSET_DATA[1, 1] = new int2(2, 0);
        I_OFFSET_DATA[1, 2] = new int2(3, 0);
        I_OFFSET_DATA[1, 3] = new int2(-1, 0);

        I_OFFSET_DATA[2, 0] = new int2(2, 0);
        I_OFFSET_DATA[2, 1] = new int2(-1, 0);
        I_OFFSET_DATA[2, 2] = new int2(1, 0);
        I_OFFSET_DATA[2, 3] = new int2(0, 0);

        I_OFFSET_DATA[3, 0] = new int2(-1, 0); // 0 to 1: -1,0 - 1,1 = -2,-1
        I_OFFSET_DATA[3, 1] = new int2(1, 1); // 1 to 2: 1,1 - 2,-1 = -1,2
        I_OFFSET_DATA[3, 2] = new int2(2, -1); // 2 to 3: 2,-1 - 0,-2 = 2,1
        I_OFFSET_DATA[3, 3] = new int2(0, -2); // 3 to 0: 0,-2 - -1,0 = 1,-2

        // 2,0 - 0,1 = 2,-1
        // 0,1 - 2,0 = -2,1
        I_OFFSET_DATA[4, 0] = new int2(2, 0); // 0 to 1: 2,0 - 0,-2 = 2,2
        I_OFFSET_DATA[4, 1] = new int2(0, -2); // 1 to 2: 0,-2 - -2,0 = 2,-2
        I_OFFSET_DATA[4, 2] = new int2(-2, 0); // 2 to 3: -2,0 - 0,2 = -2,-2
        I_OFFSET_DATA[4, 3] = new int2(0, 2); // 3 to 0: 0,2 - 2,0 = -2, 2

        O_OFFSET_DATA = new int2[1, 4];
        O_OFFSET_DATA[0, 0] = int2.zero;
        O_OFFSET_DATA[0, 1] = int2.zero;
        O_OFFSET_DATA[0, 2] = int2.zero;
        O_OFFSET_DATA[0, 3] = int2.zero;

    }

    /// <summary>
    /// Called at the first frame instance is enabled. Sets some variables and object pools.
    /// </summary>
    private void Start()
    {
        piecesInGame = new List<GameObject>();
        availablePieces = new List<GameObject>();
        if (board.RS == RotationSystems.ARS)
        {
            JLSTZ_OFFSET_DATA = new int2[5, 4];
            JLSTZ_OFFSET_DATA[0, 0] = new int2(0, 1);
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

            O_OFFSET_DATA = new int2[1, 4];
            O_OFFSET_DATA[0, 0] = int2.zero;
            O_OFFSET_DATA[0, 1] = int2.zero;
            O_OFFSET_DATA[0, 2] = int2.zero;
            O_OFFSET_DATA[0, 3] = int2.zero;

        }
    }

    public void InitiatePieces()
    {
        bag = new List<int>();
        holdPieceManager = Instantiate(nextPieceManagerPrefab, transform).GetComponent<NextPieceManager>();
        holdPieceManager.transform.localPosition = new Vector2(relativeHoldPieceCoordinate.x, relativeHoldPieceCoordinate.y);
        for (int i = 1; i < relativeNextPieceCoordinates.Count + 1; i++)
        {
            GameObject gameObject = Instantiate(nextPieceManagerPrefab, transform);
            nextPieceManagers.Add(gameObject.GetComponent<NextPieceManager>());
            gameObject.transform.localPosition = new Vector2(relativeNextPieceCoordinates[i - 1].x, relativeNextPieceCoordinates[i - 1].y);
        }
        for (int i = 0; i < 16; i++)
        {
            // bag.Add(new List<int> { 0, 1, 2, 3, 4, 5, 6 });

            List<int> bagshuff = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
            Shuffle(bagshuff);
            if (GameEngine.debugMode)
            {
                Debug.Log(bagshuff);
            }

            foreach (var piece in bagshuff)
            {
                bag.Add(piece);
            }
        }
        RefreshNextPieces();
        UpdatePieceBag();
    }

    /// <summary>
    /// Called once game trying to get a tetromino piece.
    /// </summary>
    public void UpdatePieceBag()
    {
        if (bag[pieces] < 7)
        {
            if ((board.spawnDelay < 1 && (!board.ending || board.spawnTicks >= 0)) || executedHold == true)
            {
                board.LockTicks = 0;
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
                    if (!piecemovementlocked)
                    {
                        MoveCurPiece(new int2(0, -1));
                    }
                }
                gravityTiles -= (float)Math.Floor(gravityTiles);
            }
            else
            {
                nextpiecequeued = true;
            }
            if (GameEngine.debugMode)
            {
                Debug.Log("Next random num: " + bag[pieces]);
            }

            executedHold = false;
        }
        if (pieces % 7 == 0)
        {
            List<int> bagshuff = new List<int>() { 0, 1, 2, 3, 4, 5, 6 };
            Shuffle(bagshuff);
            if (GameEngine.debugMode)
            {
                Debug.Log(bagshuff);
            }

            for (int j = 0; j < 7; j++)
            {
                bag.Add(bagshuff[j]);
            }
        }
    }

    private bool IHSexecuted;
    private void NextPiece()
    {
        RefreshNextPieces();
        if (allowHold == true)
        {
            if (board.level >= 600 && board.nextibmblocks < board.nextPieces + 1)
            {
                board.nextibmblocks++;
            }
        }
        // UpdateShownPieces();
        if (!IHSexecuted)
        {
            gameAudio.PlayOneShot(nextpieceSE[bag[lockedPieces + 1]]);
        }
        if (board.level % board.sectionSize < board.sectionSize - 1 && board.level < board.endingLevel && lockedPieces > 0 && allowHold == true)
        {
            board.level++;
            audioBellPlayed = false;
        }
        else if (audioBellPlayed == false && lockedPieces > 1 && allowHold == true)
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
    //     board.spawnTicks += Time.deltaTime * 100;
    // }
    /// <summary>
    /// Called once every frame. Checks for player input.
    /// </summary>
    private void FixedUpdate()
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
            if (!piecemovementlocked)
            {
                gravityTiles += board.gravity;
            }

            if (nextpiecequeued == true)
            {
                board.spawnTicks++;
                if (board.spawnTicks >= (int)Math.Floor(board.spawnDelay))
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
                    board.spawnTicks = 0;
                    board.LockTicks = 0;
                }
                if ((board.Inputs.c0.z || board.Inputs.c1.z) && (!board.Inputs.c0.y) && (!board.Inputs.c0.w)) { IRSCW = true; IRSCCW = false; IRSUD = false; }
                else if ((!board.Inputs.c0.z && !board.Inputs.c1.z) && (board.Inputs.c0.y) && !(board.Inputs.c0.w)) { IRSCCW = true; IRSCW = false; IRSUD = false; }
                else if ((!board.Inputs.c0.z && !board.Inputs.c1.z) && (!board.Inputs.c0.y) && (board.Inputs.c0.w)) { IRSCCW = false; IRSCW = false; IRSUD = true; }
                else { IRSCCW = false; IRSCW = false; IRSUD = false; }
                if (board.Inputs.c1.x)
                {
                    IHS = true;
                }
                else
                {
                    IHS = false;
                }

                if (board.RS == RotationSystems.ARS)
                {
                    IARS = true;
                }
            }
            else { IRSCCW = false; IRSCW = false; IRSUD = false; IARS = false; IHS = false; }
            if (((board.Inputs.c1.x && !PrevInputs.c1.x) || IHS) && !piecemovementlocked && allowHold)
            {
                if (IHS)
                {
                    gameAudio.PlayOneShot(audioIHS);
                    IHSexecuted = true;
                }
                ExecuteHold();
            }
            ProcessMovement();
            //This is temporary.
            if (Input.GetKeyDown(KeyCode.Escape) && board.mode.OnBlockOut())
            {
                MenuEngine.instance.mainPlayer.GameOver = true;
                MenuEngine.instance.mainPlayer.IntentionalGameOver = true;
                if (MenuEngine.instance.mainPlayer.lives == 1)
                {
                    MenuEngine.instance.mainPlayer.frames = 300;
                }
                else
                {
                    curPieceController.SetPiece();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                NotificationEngine.Notify("Oneshot switch is on.");
            }

            ProcessRotation();
            // if (IARS && !piecemovementlocked) curPieceController.RotatePiece(true, false, true);


            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                GameEngine.debugMode = !GameEngine.debugMode;
            }
            if (board.activePiece != null)
            {
                if (!board.CanMovePiece(int2.zero) && !piecemovementlocked)
                {
                    board.SendPieceToFloor();
                }
                ProcessGravity();
            }
            if (board.Inputs.c0.x && !PrevInputs.c0.x && !piecemovementlocked)
            {
                board.SendPieceToFloor();
            }
            PrevInputs = board.Inputs;
            if (!piecemovementlocked || PrevInputs.c0.x)
            {
                PrevInputs.c0.x = board.Inputs.c0.x;
            }
        }
    }

    private void ProcessGravity()
    {
        int tilesCounted = 0;
        if (piecemovementlocked == false)
        {
            while (gravityTiles >= 1)
            {
                if (!board.CanMovePiece(new int2(0, -1 - tilesCounted)))
                {
                    gravityTiles = 0;
                }
                else
                {
                    tilesCounted++;
                    gravityTiles--;
                }
            }
        }

        board.MovePiece(new int2(0, -tilesCounted), true);
    }

    private void ProcessRotation()
    {
        if (((board.Inputs.c0.z && !PrevInputs.c0.z) || (board.Inputs.c1.z && !PrevInputs.c1.z) || IRSCW) && !piecemovementlocked)
        {
            board.RotatePiece(true, true, false);
            if (IRSCW)
            {
                gameAudio.PlayOneShot(audioIRS);
            }
        }
        if (((board.Inputs.c0.y && !PrevInputs.c0.y) || IRSCCW) && !piecemovementlocked)
        {
            board.RotatePiece(false, true, false);
            if (IRSCCW)
            {
                gameAudio.PlayOneShot(audioIRS);
            }
        }
        if (((board.Inputs.c0.w && !PrevInputs.c0.w) || IRSUD) && !piecemovementlocked)
        {
            board.RotatePiece(true, true, true);
            if (IRSUD)
            {
                gameAudio.PlayOneShot(audioIRS);
            }
        }
    }

    private void ProcessMovement()
    {
        if (board.movement.y < -deadzone && !piecemovementlocked)
        {
            gravityTiles += board.gravity * (float)board.SDF * (board.movement.y * -1);
        }
        if (board.movement.x < -deadzone)
        {
            DASfl++;
            if (!piecemovementlocked)
            {
                if (DASfl == 1)
                {
                    MoveCurPiece(new int2(-1, 0));
                }

                if (DASfl > DAStuning && (ARRtuning == 0 || DASfl % ARRtuning == 0))
                {
                    MoveCurPiece(new int2(-1, 0));
                    if (ARRtuning == 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            MoveCurPiece(new int2(-1, 0));
                        }
                    }
                }
            }
        }
        else
        {
            DASfl = 0;
        }

        if (board.movement.x > deadzone)
        {
            DASfr++;
            if (!piecemovementlocked)
            {
                if (DASfr == 1)
                {
                    MoveCurPiece(new int2(1, 0));
                }

                if (DASfr > DAStuning && (ARRtuning == 0 || DASfr % ARRtuning == 0))
                {
                    MoveCurPiece(new int2(1, 0));
                    if (ARRtuning == 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            MoveCurPiece(new int2(1, 0));
                        }
                    }
                }
            }
        }
        else
        {
            DASfr = 0;
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
        if (piecemovementlocked == false && allowHold == true)
        {
            allowHold = false;
            SpawnHoldPiece();
            executedHold = false;
        }
    }

    private void RefreshNextPieces()
    {
        for (int i = 0; i < nextPieceManagers.Count; i++)
        {
            int isHoldEmpty = isHeld ? 1 : 0;
            int isBigMode = board.bigMode ? 7 : 0;
            int textureSel = board.RS == RotationSystems.ARS ? 7 : 0;
            int ibmTextureSel = board.sectAfter20g > 1 ? 14 : 0;
            int combine = textureSel + ibmTextureSel;
            if (i < board.nextPieces)
            {
                nextPieceManagers[i].SetNextPiece(minoPositions[bag[pieces + i + isHoldEmpty] + isBigMode], numberToTextureIDs[bag[pieces + i + isHoldEmpty] + combine], nextPieceManagerSizes[i] * (board.bigMode ? 0.5f : 1));
            }
            else
            {
                nextPieceManagers[i].SetNextPiece(null, 1f);
            }
        }
    }
    ///
    /// <summary>
    /// Spawns a new Tetris piece.
    /// </summary>
    public void SpawnPiece(bool isHold = false)
    {
        if (!isHold)
        {
            allowHold = true;
            pieces++;
        }
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
        if (board.comboKeepCounter > 0)
        {
            board.comboKeepCounter--;
        }

        IHSexecuted = false;
        int isHoldEmpty = !isHeld ? 0 : 1;
        int textureSel = board.RS == RotationSystems.ARS ? 7 : 0;
        int ibmTextureSel = board.sectAfter20g > 1 ? 14 : 0;
        int isBigMode = board.bigMode ? 7 : 0;
        int combine = textureSel + ibmTextureSel;
        int result = bag[lockedPieces + isHoldEmpty];
        board.SpawnPiece(numberToTextureIDs[result] + combine, minoPositions[result + isBigMode], pivotPositions[result + isBigMode], (PieceType)result);
        piecemovementlocked = false;
        NextPiece();
    }
    /// <summary>
    /// Swaps a piece.
    /// </summary>
    public void SpawnHoldPiece()
    {
        if (!IHSexecuted)
        {
            AudioManager.PlayClip("hold");
        }

        piecemovementlocked = true;
        gravityTiles = 1.0f;
        if (board.gravity >= 19.99999)
        {
            gravityTiles = 22.0f;
        }
        int localTextureID = board.activePiece[0].z;
        PieceType storedType = board.curType;
        if (isHeld)
        {
            board.SpawnPiece(holdPieceTextureID, minoPositions[holdPieceID], pivotPositions[holdPieceID], holdPieceType);
        }
        else
        {
            isHeld = true;
            SpawnPiece(true);
        }
        holdPieceType = storedType;
        holdPieceID = bag[lockedPieces];
        holdPieceTextureID = localTextureID;
        holdPieceManager.SetNextPiece(minoPositions[holdPieceID], holdPieceTextureID);
        // curPieceController.SpawnPiece(randPiece, this);
        piecemovementlocked = false;
        if ((board.Inputs.c0.z || board.Inputs.c1.z) && (!board.Inputs.c0.y) && (!board.Inputs.c0.w)) { IRSCW = true; IRSCCW = false; IRSUD = false; }
        else if ((!board.Inputs.c0.z && !board.Inputs.c1.z) && (board.Inputs.c0.y) && !(board.Inputs.c0.w)) { IRSCCW = true; IRSCW = false; IRSUD = false; }
        else if ((!board.Inputs.c0.z && !board.Inputs.c1.z) && (!board.Inputs.c0.y) && (board.Inputs.c0.w)) { IRSCCW = false; IRSCW = false; IRSUD = true; }
        else { IRSCCW = false; IRSCW = false; IRSUD = false; }
        if (board.RS == RotationSystems.ARS)
        {
            IARS = true;
        }
    }
    /// <summary>
    /// Moves the current piece controlled by the player.
    /// </summary>
    /// <param name="movement">X,Y amount the piece should be moved by</param>
    public void MoveCurPiece(int2 movement)
    {
        board.MovePiece(movement, false);
        if (board.bigMode)
        {
            board.MovePiece(movement, false);
        }
    }
}
