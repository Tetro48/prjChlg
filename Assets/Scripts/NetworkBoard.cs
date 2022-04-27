using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Pool;
using UnityEngine.InputSystem;
using Unity.Netcode;
using TMPro;

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

/// <summary>
/// This handles almost everything GameEngine.cs handled before multiplayer update and a bit more.
/// </summary>
public class NetworkBoard : MonoBehaviour
{
    public byte lives = 1;
    public static List<NetworkBoard> player = new List<NetworkBoard>();
    public BoardController boardController;
    public BoardParticleSystem boardParticles;
    public PiecesController piecesController;
    public int playerID;
    public bool GameOver, IntentionalGameOver;
    public double time, rollTime, rollTimeLimit = 11000, notifDelay, sectionlasttime, coolprevtime;

    public double[] sectionTime = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};

    public SectionState[] cools = new SectionState[21];

    /// <summary>
    /// Section COOL criteria Time 
    /// </summary>
	public static double[] tableTimeCool =
	{
		52, 52, 49, 45, 45, 42, 42, 38, 38, 38, 33, 33, 33, 28, 28, 22, 18, 14, 9, 6, -1
	};

    /// <summary>
    /// Section REGRET criteria Time 
    /// </summary>
    public static double[] tableTimeRegret = 
    {
        90, 75, 75, 68, 60, 60, 50, 50, 50, 50, 45, 45, 45, 40, 40, 34, 30, 26, 17, 8, -1
    };
    public IMode mode;
    public IRuleset ruleset;
    public IGrid grid;
    public IRandomizer randomizer;
    public int level, sectionSize = 100;
    public static int highestLevel;

    public int endingLevel = 2100;

    public int curSect, sectAfter20g;

    // Number of combos | Keeping combo
    public int comboCount, comboKeepCounter;



    public AudioClip readySE, goSE, gradeUp, excellent, coolSE, regretSE, hardDropSE, moveSE, rotateSE, audioPieceLock, audioPieceStep, lineClearTopout;

    public double gradePoints, statGradePoints, gradePointRequirement = 100;

    private int virtualBasePoint;

    public TextMeshPro levelTextRender, nextSecLv, timeCounter, rollTimeCounter, ppsCounter;
    public Slider gradePointSlider;

    public SpriteRenderer readyGoIndicator, gradeIndicator;

    public Sprite[] gradeSprites;

    public int grade;

    public Sprite readySprite, goSprite;

    public int nextPieces, nextibmblocks;

    public RotationSystems RS;

    public bool TLS, tSpin, ending, coolchecked, previouscool;

    public bool lineFreezingMechanic, bigMode, oneshot;
    public bool LockDelayEnable;
    public int countLockResets, maxLockResets = 20;
    public double LockDelay = 50, LockTicks = 0;
    public double DAS = 15;
    public double SDF = 6;
    public double spawnDelay = 41.66666666666666;
    public double spawnTicks = 42 - 300;
    public double lineSpawnDelay = 16.66666666666666666;
    public double lineDropTicks = 0;
    public double lineDropDelay = 25;
    public float gravity = 3/64f;

    public int frames;

    public int singles, doubles, triples, tetrises, pentrises, sixtrises, septrises, octrises;
    public int allClears;

    public int totalLines;

    public int[] lineClonePerPiece = {2147483647,2147483647,20,20,20,20,20,20,20,20,16,16,16,8,8,6,5,4,3,2,2,2};

    public int lineClonePiecesLeft = 20;

    public double percentage = 0.8f;

    public bool paused, FrameStep, framestepped;

    public bool4x2 Inputs;
    static int[] lvlLineIncrement = {1, 3, 6, 10, 15, 21, 28, 36, 48, 70, 88, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90};

    public int[] linesFrozen = {0, 0, 0, 6, 4, 0, 0, 0, 8, 0, 0, 12, 16, 0, 0, 0, 19, 0, 0, 0, 10, 14};
    #region Functions

    /*Note: This piece of function code is done in an expression body.

    public var function() => var

    is effectively the same as

    public var function()
    {
        return var
    }

    */
    public bool CanTileMove(int2 endPos) => boardController.IsPosEmpty(endPos) && boardController.IsInBounds(endPos);
    static int2 V3ToInt2(Vector3 vector3) => new int2(Mathf.FloorToInt(vector3.x + 0.5f), Mathf.FloorToInt(vector3.y + 0.5f));
    static Vector2Int int2ToV2Int(int2 integers) => new Vector2Int(integers.x, integers.y);
    #endregion

    #region Piece handling
    [Header("Piece")]
    public int3[] activePiece;
    public float2 pivot {get; private set;}
    public Chunk chunk;
    [SerializeField]
    GameObject tileRotation;
    public PieceType curType;
    public int rotationIndex { get; private set; }
    public bool fullyLocked, harddrop;
    public int tileInvisTime = -1;

    public float2 movement;

    public void Initialize(IMode mode, IRuleset ruleset, IRandomizer randomizer, IGrid grid)
    {
        this.mode = mode;
        this.ruleset = ruleset;
        this.randomizer = randomizer;
        this.grid = grid;
    }
    public void SpawnPiece(int textureID, int2[] tiles, float2 setPivot, PieceType type)
    {
        rotationIndex = 0;
        piecesController.piecemovementlocked = false;
        fullyLocked = false;
        harddrop = false;
        pivot = setPivot + new float2(4,22);
        curType = type;
        activePiece = new int3[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            activePiece[i] = new int3(tiles[i] + new int2(4,22),textureID);
        }
    }
    public void SwapPiece(int3[] tiles, float2 setPivot, PieceType type)
    {
        rotationIndex = 0;
        piecesController.piecemovementlocked = false;
        fullyLocked = false;
        harddrop = false;
        pivot = setPivot + new float2(4,22);
        curType = type;
        activePiece = tiles;
    }
    public bool MovePiece(int2 movement, bool offset)
    {
        for (int i = 0; i < activePiece.Length; i++)
        {
            if (!CanTileMove(movement + activePiece[i].xy))
            {
                Debug.Log("Cant Go there!");
                if(int2ToV2Int(movement) == Vector2Int.down && harddrop == true)
                {
                    SetPiece();
                }
                return false;
            }
        }

        // boardController.UpdateActivePiece(activePiece, true);
        UnisonPieceMove(movement);
        LockTicks = 0;
        if(movement.y >= 0) if(!offset) if(LockDelay > 5 || gravity < 19) AudioManager.PlayClip("move");
        if(!CanMovePiece(new int2(0,-1)))countLockResets++;
        if(countLockResets >= maxLockResets)
        {
            LockTicks = LockDelay;
        }
        if (!CanMovePiece(new int2(0,-1)) && fullyLocked == false)  
        {
            if(LockDelayEnable == false && piecesController.piecemovementlocked == false)  
            {
                LockTicks = 0;  LockDelayEnable = true;
            }
        }
        else LockDelayEnable = false;
        return true;
    }
    public void UnisonPieceMove(int2 movement)
    {
        for (int i = 0; i < activePiece.Length; i++)
        {
            activePiece[i].xy += movement;
        }
        pivot += movement;
    }
    public bool RotateInUnison(bool clockwise, bool UD = false)
    {
        for (int i = 0; i < activePiece.Length; i++)
        {
            activePiece[i].xy = RotateObject(tileRotation, activePiece[i].xy, pivot, clockwise, UD);
        }
        return CanMovePiece(int2.zero);
    }
    public bool CanMovePiece(int2 movement)
    {
        for (int i = 0; i < activePiece.Length; i++)
        {
            if (!CanTileMove(movement + activePiece[i].xy))
            {
                return false;
            }
        }
        return true;
    }
    public static int2 RotateObject(GameObject obj, int2 tilePos, float2 pivotPos, bool clockwise, bool UD = false)
    {
        if(math.any(pivotPos == math.floor(pivotPos)))
        {
            int2 relativePos = tilePos - (int2)pivotPos;
            int2[] rotMatrix = clockwise ? new int2[2] { new int2(0, -1), new int2(1, 0) }
                                            : new int2[2] { new int2(0, 1), new int2(-1, 0) };
            int newXPos = (rotMatrix[0].x * relativePos.x) + (rotMatrix[1].x * relativePos.y);
            int newYPos = (rotMatrix[0].y * relativePos.x) + (rotMatrix[1].y * relativePos.y);
            int2 newPos = new int2(newXPos, newYPos);

            newPos += (int2)pivotPos * (UD ? 2 : 1);
            return newPos;
        }
        int multi = clockwise ? 1 : -1;
        if(UD) multi *= 2;
        obj.transform.position = new float3((float2)tilePos, 0f);
        obj.transform.RotateAround((Vector2)pivotPos, Vector3.forward, -90 * multi);
        obj.transform.Rotate(new Vector3(90f * multi, 0f, 0f), Space.Self);
        return V3ToInt2(obj.transform.position);
    }
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

        tSpin = (curType == PieceType.T && LockDelayEnable);

        // if (UD)
        // {
        //     RotatePiece180(clockwise, true, firstAttempt);
        // }

        if (!shouldOffset)
        {
            int2[,] curOffsetData;
            if(curType == PieceType.O)
            {
                curOffsetData = piecesController.O_OFFSET_DATA;
            }
            else if(curType == PieceType.I)
            {
                curOffsetData = piecesController.I_OFFSET_DATA;
            }
            else
            {
                curOffsetData = piecesController.JLSTZ_OFFSET_DATA;
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
            Debug.Log("Couldn't offset");
            rotationIndex = oldRotationIndex;
        }
    }
    static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }
    bool Offset(int oldRotIndex, int newRotIndex, bool clockwise, bool UD = false)
    {
        int2 offsetVal1, offsetVal2, endOffset;
        int2[,] curOffsetData;
        
        if(curType == PieceType.O)
        {
            curOffsetData = piecesController.O_OFFSET_DATA;
        }
        else if(curType == PieceType.I)
        {
            curOffsetData = piecesController.I_OFFSET_DATA;
        }
        else
        {
            curOffsetData = piecesController.JLSTZ_OFFSET_DATA;
        }

        endOffset = int2.zero;

        bool movePossible = false;

        for (int testIndex = 0; testIndex < curOffsetData.Length; testIndex++)
        {
            offsetVal1 = curOffsetData[testIndex, oldRotIndex];
            offsetVal2 = curOffsetData[testIndex, newRotIndex];
            endOffset = offsetVal1 - offsetVal2;
            if(bigMode) endOffset *= 2;
            if(testIndex == 0)
            {
                RotateInUnison(clockwise, UD);
            }
            // Debug.Log("Test " + testIndex + " out of 4");
            if (CanMovePiece(endOffset))
            {
                movePossible = true;
                // Debug.Log("Success!");
                break;
            }
        }

        if (movePossible)
        {
            MovePiece(endOffset, true);
        }
        else RotateInUnison(!clockwise, UD);
        if(LockDelay > 6 || gravity < 19) AudioManager.PlayClip(rotateSE);
        // else
        // {
        //     Debug.Log("Move impossible");
        // }
        return movePossible;
    }
    public void SetPiece()
    {
        AudioManager.PlayClip(audioPieceLock);
        piecesController.piecemovementlocked = true;
        piecesController.lockedPieces++;
        fullyLocked = true;
        countLockResets = 0;
        for(int i = 0; i < activePiece.Length; i++)
        {
            if (!boardController.SetTile(activePiece[i]))
            {
                if(GameEngine.debugMode) Debug.Log("GAME OVER!");
                GameOver = true;
                piecesController.GameOver();
            }
        }
        if (GameOver == false)
        {
            if (Input.GetKey(KeyCode.E) && GameEngine.debugMode)
            {
                int incrementbyfrozenlines = lineFreezingMechanic ? linesFrozen[curSect] : 0;
                boardController.FillLine(0+incrementbyfrozenlines);
                boardController.FillLine(1+incrementbyfrozenlines);
                boardController.FillLine(2+incrementbyfrozenlines);
                boardController.FillLine(3+incrementbyfrozenlines);
            }
            boardController.CheckLineClears();
            piecesController.UpdatePieceBag();
        }
    }
    public void SendPieceToFloor()
    {
        harddrop = true;
        piecesController.PrevInputs.c0.x = true;
        AudioManager.PlayClip(hardDropSE);
        while (MovePiece(new int2(0,-1), true)) {}
    }
    #endregion

    
    bool cool, cooldisplayed;

    //each mode implements its own.
    public void LineClears(int lines, bool spin)
    {
        mode.OnLineClear(this, lines, spin);
    }
    public void OnMovement(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read)
        {
            movement = value.ReadValue<Vector2>();
            if (movement.y > 0.5) {Inputs.c0.x = true;}
            else {Inputs.c0.x = false;}
        }
        else if (value.performed)
        {
            if (movement.x > 0.5 && Time.timeScale < 10)
                Time.timeScale += 0.1f;
            if (movement.x < -0.5 && Time.timeScale > .1)
                Time.timeScale -= 0.1f;
            GameEngine.instance.gameMusic.pitch = Time.timeScale;
        }
    }
    public void OnCounterclockwise(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read){
        if (value.performed) Inputs.c0.y = true;
        else Inputs.c0.y = false;}
    }
    public void OnClockwise(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read){
        if (value.performed) Inputs.c0.z = true;
        else Inputs.c0.z = false;}
    }
    public void OnClockwise2(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read){
        if (value.performed) Inputs.c1.z = true;
        else Inputs.c1.z = false;}
    }
    public void OnUpsideDown(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read){
        if (value.performed) Inputs.c0.w = true;
        else Inputs.c0.w = false;}
    }
    public void OnHold(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read){
        if (value.performed) Inputs.c1.x = true;
        else Inputs.c1.x = false;}
    }
    public void OnPause(InputAction.CallbackContext value)
    {
        if (value.started && player.Count <= 1)
        {
            paused = !paused;
        }
    }
    public void OnFramestep(InputAction.CallbackContext value)
    {
        if (value.started && player.Count <= 1)
        {
            Inputs.c1.w = true;
        }
        // if (value.performed) HoldInputs.c1.w = true;
        // else HoldInputs.c1.w = false;
    }
    void Awake()
    {
        Inputs = new bool4x2(false);
        UnityEngine.Random.InitState(SeedManager.seed);
        if(ReplayRecord.instance.mode != ReplayModeType.read)
        {
            ReplayRecord.instance.switches[player.Count] = new bool[3] {lineFreezingMechanic, bigMode, oneshot};
        }

        if(player.Count > 0) playerID = player.Count;
        else playerID = 0;
        boardController.playerID = playerID;
        piecesController.playerID = playerID;
        // if(IsOwner)
        {
            MenuEngine.instance.mainPlayer = this;
            MenuEngine.instance.curBoard = gameObject;
        }
        player.Add(this);
    }

    void Start()
    {
        
    }
    
    void FixedUpdate()
    {
        // if(!NetworkObject.IsSpawned) NetworkObject.Spawn();
        if(level > highestLevel && !GameOver) highestLevel = level;
        // if (IsOwner)
        {
            NetworkUpdate();
        }
    }
    // Update is called once per frame
    void NetworkUpdate()
    {
        float deltaTime = Time.deltaTime;
        // chunk.UpdateChunk(new int2(10,40), activePiece, new float[10,40]);
        if(!GameOver)
        {
            if((paused == false || (FrameStep == true && Inputs.c1.w)) && GameOver == false)
            {
                if(LockDelayEnable && !harddrop && !fullyLocked)
                {
                    if(LockTicks == 0 && LockDelay > 4)
                    {
                        AudioManager.PlayClip("step");
                    }
                    LockTicks += deltaTime / Time.fixedDeltaTime;
                    if (LockTicks >= LockDelay)
                    {
                        LockDelayEnable = false;
                        SetPiece();
                    }
                }
                //Notice: Trying to use framestepped bool value as a TAS checker could fail if not properly implemented because built-in pausing exists!
                mode.OnUpdate(deltaTime, this);
                if (LockDelayEnable)
                {
                    LockTicks += deltaTime;
                }
                if (!LockDelayEnable && !piecesController.piecemovementlocked)  
                {
                    if(!CanMovePiece(new int2(0,-1)) && !fullyLocked)  
                    {
                        LockTicks = 0;  LockDelayEnable = true;
                    }
                    else LockDelayEnable = false;
                }
                if(time > 0)
                ppsCounter.text = String.Format("{0} pieces/second\nLock: {1} / {2}\nResets: {3} / {4}",
                    Math.Floor(((double) piecesController.lockedPieces / time)* 100) / 100, SIUnitsConversion.doubleToSITime((LockDelay-LockTicks)/100), SIUnitsConversion.doubleToSITime(LockDelay/100), maxLockResets - countLockResets, maxLockResets);
            
                framestepped = true;
            }
            else if (paused == true)
            {
                framestepped = false;
            }
            Inputs.c1.w = false;
            if(spawnTicks == (int)spawnDelay - 200) {AudioManager.PlayClip(readySE); readyGoIndicator.sprite = readySprite;}
            if(spawnTicks == (int)spawnDelay - 100) {AudioManager.PlayClip(goSE); readyGoIndicator.sprite = goSprite;}
            if(spawnTicks == (int)spawnDelay - 1) 
            {
                if(!GameEngine.instance.gameMusic.isPlaying) GameEngine.instance.gameMusic.Play();
                readyGoIndicator.sprite = null;
            }
        }
        else if (lives < 2)
        {
            readyGoIndicator.sprite = null;
            if(frames%10==9 && frames<400)boardController.DestroyLine(frames/10);
            if(frames<400)boardController.DecayLine(frames/10, 0.1f);
            frames++;
            if(frames == 1)
            GameEngine.instance.gameMusic.Stop();
            if(player.Count > 1 && frames == 1)
            frames = 300;
            if(frames > 300)
            {
                if(ending)
                {
                    transform.position += Vector3.up * Mathf.Log10(frames) * 0.043f;
                }
                if (frames == 301)
                {
                    GameEngine.instance.gameMusic.Stop();
                    if(highestLevel == level) highestLevel = 0;
                    if(!ending)
                    {
                        MenuEngine.instance.audioSource2.PlayOneShot(MenuEngine.instance.topoutSE);
                        Rigidbody rigidbody;
                        rigidbody = gameObject.AddComponent<Rigidbody>();
                        rigidbody.mass = 16;
                        Vector3 explosionPos = new Vector3(transform.position.x + UnityEngine.Random.Range(-20f, 20f), transform.position.y + UnityEngine.Random.Range(-20f, 20f), transform.position.z + UnityEngine.Random.Range(-20f, 20f));
                        rigidbody.AddExplosionForce(150f, explosionPos, 50f, 2f, ForceMode.Impulse);
                        rigidbody.angularDrag = 1f;
                    }
                    Destroy(gameObject.GetComponent<PlayerInput>());
                    Destroy(ppsCounter.gameObject);
                    Destroy(gameObject, 10f);
                }
                if (frames == 351)
                {
                    if(player.Count < 2 && !ending) MenuEngine.instance.audioSource2.Play();
                    else AudioManager.PlayClip(excellent);
                    Debug.Log(player.Count);
                }
                if (frames == 401)
                {
                    MenuEngine.instance.ExtractStatsToNotifications(this);
                    int aliveplayers = 0;
                    for (int i = 0; i < player.Count; i++)
                    {
                        if (!player[i].GameOver)
                        {
                            aliveplayers++;
                        }
                    }
                    if (aliveplayers < 1)
                    {
                        BackgroundController.bginstance.TriggerBackgroundChange(0);
                        if (ReplayRecord.instance.mode == ReplayModeType.write)
                        {
                            ReplayRecord.instance.SaveReplay(DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss"));
                        }
                        player = new List<NetworkBoard>();
                        if(oneshot && curSect > 2)
                        {
                            PlayerPrefs.SetInt("Oneshot", 3);
                            PlayerPrefs.Save();
                        }
                        MenuEngine.instance.starting = true;
                    }
                    else
                    {
                        GameEngine.instance.gameMusic.Play();
                    }
                    GameEngine.ResetMusic();
                    // NetworkObject.Destroy(gameObject);
                }
            }
        }
        else if(paused && !framestepped)
        {
            framestepped = Inputs.c1.w;
        }
        //If you have more than 1 life
        else
        {
            frames++;
            if(frames == 1)
            {
                spawnTicks = (int)spawnDelay - 250;
                if(IntentionalGameOver)Destroy(piecesController.curPieceController.gameObject);
                MenuEngine.instance.audioSource2.PlayOneShot(MenuEngine.instance.topoutSE);
            }
            if(frames<80)
            {
                boardController.DecayLine(39-frames/2, 0.5f);
                if(frames%2==1)
                {
                    if(boardController.TilesInALine(39-frames/2) > 0)AudioManager.PlayClip(lineClearTopout);
                    boardController.DestroyLine(39-frames/2);
                }
            }
            else
            {
                GameOver = false;
                lives--;
                frames = 0;
                piecesController.UpdatePieceBag();
                IntentionalGameOver = false;
            }
            framestepped = false;
            Inputs.c1.w = false;
        }
    }
    public void SpawnFireworks()
    {
        boardParticles.SummonFirework(new Vector2(0f, 10f), new Vector2(10f,10f));
    }
    public void DisconnectGameOver()
    {
        if (MenuEngine.instance.curBoard != null) GameOver = true;
    }
    public void ControllerSwap() 
    {
        if (MenuEngine.instance.curBoard != null && notifDelay == 0) 
        {
            NotificationEngine.Notify(LanguageList.Extract(LangArray.notifications, MenuEngine.instance.language, 13), Color.white); notifDelay = 300;
        } 
    }
    public void ShowGradeScore() 
    {
        if (notifDelay == 0) 
        {
            NotificationEngine.Notify(LanguageList.Extract(LangArray.notifications, MenuEngine.instance.language, 14), Color.white);
            NotificationEngine.Notify(Math.Floor(gradePoints).ToString(), Color.white);
            NotificationEngine.Notify("/" + Math.Floor(gradePointRequirement), Color.white);
            notifDelay = 200;
        }
    }
}
