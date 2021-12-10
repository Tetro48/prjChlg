using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.InputSystem;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using TMPro;
using Unity.Entities;

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

/// <summary>
/// This handles almost everything GameEngine.cs handled before multiplayer update and a bit more.
/// </summary>
public class NetworkBoard : NetworkBehaviour
{
    EntityManager entityManager;
    BlobAssetStore blobAssetStore;
    public int lives = 1;
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

    public SpriteRenderer readyGoIndicator, gradeIndicator;

    public Sprite[] gradeSprites;

    public int grade;

    public Sprite readySprite, goSprite;

    public int nextPieces, nextibmblocks;

    public RotationSystems RS;

    public bool TLS, tSpin, ending, coolchecked, previouscool;

    public bool lineFreezingMechanic, bigMode;
    public bool LockDelayEnable;
    [Range(0,25)]
    public int countLockResets, maxLockResets = 20;

    [Range(0, 1000)]
    public double LockDelay = 50, LockDelayf = 0;

    [Range(0, 1000)]
    public double DAS = 15;

    [Range(0, 1000)]
    public double SDF = 6;

    [Range(0, 1000)]
    public double ARE = 41.66666666666666;

    [Range(-1005, 1000)]
    public double AREf = 42 - 300;

    public int frames;

    [Range(0, 1000)]
    public double AREline = 16.66666666666666666;

    public int lineDelayf = 0;

    [Range(0, 1000)]
    public double lineDelay = 25;

    [Range(0, 60)]
    public float gravity = 3/64f;

    public int singles, doubles, triples, tetrises, pentrises, sixtrises, septrises, octrises, allClears;

    public int totalLines;

    public int[] lineClonePerPiece = {2147483647,2147483647,20,20,20,20,20,20,20,20,16,16,16,8,8,6,5,4,3,2,2,2};

    public int lineClonePiecesLeft = 20;

    public double percentage = 0.8f;

    public bool paused, FrameStep, framestepped;

    public bool[] Inputs;

    #region Piece handling
    public int3[] activePiece {get; set;}
    public float2 pivot {get; private set;}
    [SerializeField]
    Entity tileRotation;

    static Vector2Int int2ToV2Int(int2 integers)
    {return new Vector2Int(integers.x, integers.y);}
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

        UnisonPieceMove(movement);
        LockDelayf = 0;
        if(movement.y >= 0) if(!offset) if(LockDelay > 5 || gravity < 19) AudioManager.PlayClip(moveSE);
        if(!CanMovePiece(new int2(0,-1)))countLockResets++;
        if(countLockResets >= maxLockResets)
        {
            LockDelayf = LockDelay;
        }
        if (!CanMovePiece(new int2(0,-1)) && fullyLocked == false)  
        {
            if(LockDelayEnable == false && piecesController.piecemovementlocked == false)  
            {
                LockDelayf = 0;  LockDelayEnable = true;
            }
        }
        else LockDelayEnable = false;
        return true;
    }
    public void UnisonPieceMove(int2 movement)
    {
        boardController.UpdateActivePiece(activePiece, true);
        for (int i = 0; i < activePiece.Length; i++)
        {
            activePiece[i].xy += movement;
        }
        pivot += movement;
        boardController.UpdateActivePiece(activePiece);
    }
    public bool RotateInUnison(bool clockwise, bool UD = false)
    {
        boardController.UpdateActivePiece(activePiece, true);
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
    public bool CanTileMove(int2 endPos)
    {
        return boardController.IsPosEmpty(endPos) && boardController.IsInBounds(endPos);
    }
    public static int2 RotateObject(Entity obj, int2 tilePos, Vector2 pivotPos, bool clockwise, bool UD = false)
    {
        // int2 relativePos = V3ToInt2(obj.transform.localPosition) - originPos;
        // int2[] rotMatrix = clockwise ? new int2[2] { new int2(0, -1), new int2(1, 0) }
        //                                    : new int2[2] { new int2(0, 1), new int2(-1, 0) };
        // int newXPos = (rotMatrix[0].x * relativePos.x) + (rotMatrix[1].x * relativePos.y);
        // int newYPos = (rotMatrix[0].y * relativePos.x) + (rotMatrix[1].y * relativePos.y);
        // int2 newPos = new int2(newXPos, newYPos);

        // newPos += originPos;
        // UpdatePosition(obj, newPos);
        int multi = clockwise ? 1 : -1;
        if(UD) multi *= 2;
        obj.transform.position = new float3((float2)tilePos, 0f);
        obj.transform.RotateAround(pivotPos, Vector3.forward, -90 * multi);
        obj.transform.Rotate(new Vector3(90f * multi, 0f, 0f), Space.Self);
        return V3ToInt2(obj.transform.position);
    }
    static int2 V3ToInt2(Vector3 vector3)
    {
        return new int2(Mathf.FloorToInt(vector3.x + 0.5f), Mathf.FloorToInt(vector3.y + 0.5f));
    }
    public PieceType curType;
    public int rotationIndex { get; private set; }
    bool fullyLocked, harddrop;
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

        for (int testIndex = 0; testIndex < 5; testIndex++)
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
        piecesController.PrevInputs[0] = true;
        AudioManager.PlayClip(hardDropSE);
        while (MovePiece(new int2(0,-1), true)) {}
    }
    #endregion

    public int tileInvisTime = -1;

    public float2 movement;
    [SerializeField] Entity rolltimeObject;
    
    bool cool, cooldisplayed;
	private void checkCool() {
		// COOL check
		if((level % sectionSize >= sectionSize * 0.7) && (coolchecked == false && level <= sectionSize * cools.Length)) {
			int section = level / sectionSize;

			if( (sectionTime[section] <= tableTimeCool[section]) &&
				((previouscool == false) || ((previouscool == true) && (sectionTime[section] <= coolprevtime + 1))) )
			{
				cool = true;
				cools[section] = SectionState.cool;
			}
			else cools[section] = SectionState.missed;
			coolprevtime = sectionTime[section];
			coolchecked = true;
		}

		// COOLиЎЁз¤є
		if((level % sectionSize >= sectionSize * 0.82) && (cool == true) && (cooldisplayed == false)) {
			AudioManager.PlayClip(coolSE);
			// cooldispframe = 180;
			cooldisplayed = true;
			virtualBasePoint += 600;
		}
	}

	/**
	 * REGRETгЃ® check
	 * @param engine GameEngine
	 * @param levelb Line clearе‰ЌгЃ® level
	 */
	private void checkRegret(int levelb) {
		int section = levelb / sectionSize;
		if(sectionlasttime > tableTimeRegret[section]) {

			virtualBasePoint -= 600;

			// regretdispframe = 180;
			AudioManager.PlayClip(regretSE);
			cools[section] = SectionState.regret;
		}
	}

	/** Line clear時に入る段位 point */
	static int[] tableGradePoint = {10, 30, 60, 120, 180, 240, 300, 400, 520, 640, 780, 920, 1060, 1200, 1500, 1800, 2100, 2400, 3000, 4000, 5500, 7500, 10000};

	/** 段位 pointのCombo bonus */
	private static float[,] tableGradeComboBonus =
	{
		{1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f,1.0f},
		{1.0f,1.2f,1.2f,1.4f,1.4f,1.4f,1.4f,1.5f,1.5f,2.0f},
		{1.0f,1.4f,1.5f,1.6f,1.7f,1.8f,1.9f,2.0f,2.1f,2.5f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
		{1.0f,1.5f,1.8f,2.0f,2.2f,2.3f,2.4f,2.5f,2.6f,3.0f},
	};
    static int[] lvlLineIncrement = {1, 3, 6, 10, 15, 21, 28, 36, 48, 70, 88, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90};

    public int[] linesFrozen = {0, 0, 0, 6, 4, 0, 0, 0, 8, 0, 0, 12, 16, 0, 0, 0, 19, 0, 0, 0, 10, 14};
    public void LineClears(int lines, bool spin)
    {
        if (lines > 0)
        {
            comboKeepCounter = 2;
            if (lines > 1)
            {
                comboCount++;
                if(comboCount >= 2) {
                    int cmbse = comboCount - 2;
                    if(cmbse > GameEngine.instance.comboSE.Length-1) cmbse = GameEngine.instance.comboSE.Length-1;
                    AudioManager.PlayClip(GameEngine.instance.comboSE[cmbse]);
                }
            }
        }
        if(level < endingLevel)
        {
            level += lvlLineIncrement[lines-1];
        }
        if(level > endingLevel) level = endingLevel;
        if(level/sectionSize > curSect || level > endingLevel)
        {
            sectionlasttime = sectionTime[curSect];
            checkRegret(level - lvlLineIncrement[lines-1]);
            curSect++;
            if (curSect > (endingLevel/sectionSize) - 1)
            {
                AREf = ARE - 400;
                ending = true;
            }
            AudioManager.PlayClip(piecesController.levelup);
            if (curSect < 20)
            {
                BackgroundController.bginstance.TriggerBackgroundChange(curSect);
            }
            if(curSect % 5 == 0) NotificationEngine.Notify(LanguageList.Extract(LangArray.notifications, MenuEngine.instance.language, 12),Color.white);
            if (gravity >= 10)
            {
                ARE *= percentage;
                AREline *= percentage;
                lineDelay *= percentage;
                LockDelay *= percentage;
                sectAfter20g++;
                if(LockDelay < 1)
                {
                    LockDelay = 1.000001d;
                }
                if (gravity < 19.99999) gravity *= 4;
            }
            else
            {
                gravity *= 4;
            }
            // COOLг‚’еЏ–гЃЈгЃ¦гЃџг‚‰
            previouscool = cool;

            cool = false;
            coolchecked = false;
            cooldisplayed = false;
        }
        if (spin)
        {
            if (lines == 1) virtualBasePoint += 10;
            if (lines == 2) virtualBasePoint += 20;
            if (lines == 3) virtualBasePoint += 30;
            if (lines == 4) virtualBasePoint += 50;
            if (lines == 5) virtualBasePoint += 70;
            if (lines >= 6) virtualBasePoint += 100 + (lines - 6) * 40;
        }
		int basepoint = tableGradePoint[lines - 1];
        basepoint += virtualBasePoint;
        virtualBasePoint = 0;

        int indexcombo = comboCount - 1;
        if (indexcombo < 0) indexcombo = 0;
        if (indexcombo > 9) indexcombo = 9;
        float combobonus = tableGradeComboBonus[lines - 1, indexcombo];
	
		int levelbonus = 1 + (level / 250);
	
		float point = basepoint * combobonus * levelbonus;
        if (sectAfter20g >= 21) point *= 10;
        else if (sectAfter20g > 19) point *= 5;
        else if (sectAfter20g > 18) point *= 2;
		gradePoints += point;
		statGradePoints += point;
        while (gradePoints >= gradePointRequirement)
        {
			gradePoints -= gradePointRequirement;
            if (grade < gradeSprites.Length - 1) grade++;
            gradeIndicator.sprite = gradeSprites[grade];
            AudioManager.PlayClip(gradeUp);
            gradePointRequirement *= Math.Abs(1 + (Math.Abs(Math.Floor((double)level / sectionSize) + 1) / 4));
        }
        totalLines += lines;
        if (lines == 1) singles++;
        if (lines == 2) doubles++;
        if (lines == 3) triples++;
        if (lines == 4) tetrises++;
        if (lines == 5) pentrises++;
        if (lines == 6) sixtrises++;
        if (lines == 7) septrises++;
        if (lines > 7) octrises++;
    }
    public void OnMovement(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read)
        {
            movement = value.ReadValue<Vector2>();
            if (movement.y > 0.5) {Inputs[0] = true;}
            else {Inputs[0] = false;}
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
        if (value.performed) Inputs[1] = true;
        else Inputs[1] = false;}
    }
    public void OnClockwise(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read){
        if (value.performed) Inputs[2] = true;
        else Inputs[2] = false;}
    }
    public void OnClockwise2(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read){
        if (value.performed) Inputs[6] = true;
        else Inputs[6] = false;}
    }
    public void OnUpsideDown(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read){
        if (value.performed) Inputs[3] = true;
        else Inputs[3] = false;}
    }
    public void OnHold(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read){
        if (value.performed) Inputs[4] = true;
        else Inputs[4] = false;}
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
            Inputs[7] = true;
        }
        // if (value.performed) HoldInputs[7] = true;
        // else HoldInputs[7] = false;
    }
    void Awake()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        blobAssetStore = new BlobAssetStore();
        UnityEngine.Random.InitState(SeedManager.seed);
        if(ReplayRecord.instance.mode != ReplayModeType.read)
        {
            ReplayRecord.instance.boards++;
            ReplayRecord.instance.movementVector.Add(new List<float2>());
            ReplayRecord.instance.inputs.Add(new List<bool[]>());
            bool[] tempSwitches = {false, false};
            ReplayRecord.instance.switches.Add(tempSwitches);
        }

        if(player.Count > 0) playerID = player.Count;
        else playerID = 0;
        boardController.playerID = playerID;
        piecesController.playerID = playerID;
        if(IsOwner)
        {
            MenuEngine.instance.yourPlayer = this;
            MenuEngine.instance.curBoard = gameObject;
        }
        player.Add(this);
    }
    void Destroy(Entity entity)
    {
        entityManager.DestroyEntity(entity);
    }
    void Instantiate(Entity entity)
    {
        entityManager.Instantiate(entity);
    }

    [ClientRpc]
    void ReceiveInputsClientRpc(ClientRpcParams rpcParams = default)
    {

    }

    [ServerRpc]
    void SendInputsServerRpc(ServerRpcParams rpcParams = default)
    {

    }
    
    void FixedUpdate()
    {
        if(level > highestLevel && !GameOver) highestLevel = level;
        if (IsOwner)
        {
            NetworkUpdate();
        }
    }
    // Update is called once per frame
    void NetworkUpdate()
    {
        
        if (AREf == (-400 + ARE)+1)  transform.position = new Vector3(0.0f, 18f, 0.0f);
        if (AREf == (-400 + ARE)+2)  transform.position = new Vector3(0.0f, 16f, 0.0f);
        if (AREf == (-400 + ARE)+3)  transform.position = new Vector3(0.0f, 14f, 0.0f);
        if (AREf == (-400 + ARE)+4)  transform.position = new Vector3(0.0f, 12f, 0.0f);
        if (AREf == (-400 + ARE)+5)  transform.position = new Vector3(0.0f, 10f, 0.0f);
        if (AREf == (-400 + ARE)+6)  transform.position = new Vector3(0.0f, 8f, 0.0f);
        if (AREf == (-400 + ARE)+7)  transform.position = new Vector3(0.0f, 6f, 0.0f);
        if (AREf == (-400 + ARE)+8)  transform.position = new Vector3(0.0f, 4f, 0.0f);
        if (AREf == (-400 + ARE)+9)  transform.position = new Vector3(0.0f, 2f, 0.0f);
        if (AREf == (-400 + ARE)+10)  transform.position = new Vector3(0.0f, 0f, 0.0f);
        if (AREf == (-400 + ARE)+11)  transform.position = new Vector3(0.0f, -0.8f, 0.0f);
        if (AREf == (-400 + ARE)+12)  transform.position = new Vector3(0.0f, -1.4f, 0.0f);
        if (AREf == (-400 + ARE)+13)  transform.position = new Vector3(0.0f, -2f, 0.0f);
        if (AREf == (-400 + ARE)+14)  transform.position = new Vector3(0.0f, -1.3f, 0.0f);
        if (AREf == (-400 + ARE)+15)  transform.position = new Vector3(0.0f, -0.7f, 0.0f);
        if (AREf == (-400 + ARE)+16)  transform.position = new Vector3(0.0f, 0f, 0.0f);
        if(!GameOver)
        {
            if(framestepped && activePiece != null)
            {
                if (!LockDelayEnable && !piecesController.piecemovementlocked)  
                {
                    if(!CanMovePiece(new int2(0,-1)) && !fullyLocked)  
                    {
                        LockDelayf = 0;  LockDelayEnable = true;
                    }
                    else LockDelayEnable = false;
                }
            
                if(LockDelayEnable && !harddrop && !fullyLocked)
                {
                    if(LockDelayf == 0 && LockDelay > 4)
                    {
                        AudioManager.PlayClip(audioPieceStep);
                    }
                    LockDelayf += Time.deltaTime / Time.fixedDeltaTime;
                    if (LockDelayf >= LockDelay)
                    {
                        LockDelayEnable = false;
                        SetPiece();
                    }
                }
            }
            checkCool();
            if(level > endingLevel) level = endingLevel;
            rolltimeObject.SetActive(ending);
            // musicTime += Time.deltaTime;
            if(notifDelay > 0)notifDelay--;
            if(MenuEngine.instance.curBoard != null)
            {
                if(time > 0)
                ppsCounter.text = String.Format("{0} pieces/second\nLock: {1} / {2}\nResets: {3} / {4}",
                    Math.Floor(((double) piecesController.lockedPieces / time)* 100) / 100, Math.Floor((LockDelay - LockDelayf) * 1000) / 100 + "ms", Math.Floor(LockDelay * 1000) / 100 + "ms", maxLockResets - countLockResets, maxLockResets);
            }
            // if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Space)) Inputs[2] = true;
            // if (Input.GetKey(KeyCode.A)) Inputs[3] = true;
            // if (Input.GetKey(KeyCode.C)) Inputs[4] = true;
            // if (Input.GetKeyDown(KeyCode.P) && MenuEngine.instance.curBoard != null) paused = !paused;
            if((paused == false || (FrameStep == true && Inputs[7])) && GameOver == false)
            {
                if (ReplayRecord.instance.mode == ReplayModeType.read && AREf > (int)ARE - 401)
                {
                    float2 tempmov;
                    tempmov = ReplayRecord.instance.movementVector[playerID][ReplayRecord.instance.frames[playerID]];
                    movement = new Vector2(tempmov[0], tempmov[1]);
                    // Inputs = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames[playerID]];
                    for (int i = 0; i < 7; i++)
                    {
                        Inputs[i] = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames[playerID]][i];
                    }
                    lineFreezingMechanic = ReplayRecord.instance.switches[playerID][0];
                }
                else if(AREf > (int)ARE - 401)
                {
                    bool[] modInputs = new bool[8];
                    for (int i = 0; i < 8; i++)
                    {
                        modInputs[i] = Inputs[i];
                    }
                    modInputs[7] = false;
                    ReplayRecord.instance.inputs[playerID].Add(modInputs);
                    float2 modMovement = new float2(movement.x, movement.y);
                    ReplayRecord.instance.movementVector[playerID].Add(modMovement);
                }
                if (level >= endingLevel && AREf < (int)ARE && AREf > (int)ARE - 400)
                {
                    double whichline = ((AREf - ARE)+400)/10;
                    if(GameEngine.debugMode) Debug.Log(whichline);
                    boardController.DestroyLine((int)whichline);
                }
                
                if(ending && AREf >= 0)tileInvisTime = 20 - ((int)rollTime / (400/6*10));
                else tileInvisTime = -1;
                if (AREf == (int)ARE - 399) AudioManager.PlayClip(excellent);
                if(AREf >= 0 && readyGoIndicator.sprite == null && rollTime < rollTimeLimit)time += Time.deltaTime;
                if(AREf >= 0 && readyGoIndicator.sprite == null && ending && rollTime < rollTimeLimit)
                {
                    rollTime += Time.deltaTime;
                    if(rollTime >= rollTimeLimit)
                    {
                        rollTime = rollTimeLimit;
                        AREf = (int)ARE - 1000;
                        Destroy(piecesController.piecesInGame[piecesController.piecesInGame.Count-1]);
                        piecesController.UpdatePieceBag();
                    }
                }
                if (AREf == (int)ARE - 401)
                {
                    lives = 1;
                    GameOver = true;
                }
                if(AREf < (int)ARE - 401)
                {
                    if(AREf % 10 == 0) SpawnFireworks();
                    if(AREf % 50 == 0 && grade < gradeSprites.Length - 1)
                    {
                        grade++;
                        gradeIndicator.sprite = gradeSprites[grade];
                        AudioManager.PlayClip(gradeUp);
                    }
                }
                int nextsecint = (curSect + 1) * sectionSize > endingLevel ? endingLevel : level < endingLevel ? (curSect + 1) * sectionSize : endingLevel;
                levelTextRender.text = level.ToString();
                if(curSect < 21)nextSecLv.text = nextsecint.ToString();
                if(!ending)
                {
                    timeCounter.text = TimeConversion.doubleFloatTimeCount(time);
                    if(AREf >= 0 && readyGoIndicator.sprite == null && rollTime < rollTimeLimit)sectionTime[curSect] += Time.deltaTime;
                }
                rollTimeCounter.text = TimeConversion.doubleFloatTimeCount(rollTimeLimit - rollTime);
                framestepped = true;
                // for (int i = 0; i < 7; i++)
                // {
                //     // if(Inputs[i]>0)Inputs[i]--;
                //     // if(FrameHoldInputs[i]>0)FrameHoldInputs[i]--;
                //     // HoldInputs[i] = FrameHoldInputs[i] > 0;
                // } 
                if (comboKeepCounter == 0)
                {
                    comboCount = 0;
                }
            }
            else if (paused == true)
            {
                framestepped = false;
            }
            Inputs[7] = false;
            if(AREf == (int)ARE - 200) {AudioManager.PlayClip(readySE); readyGoIndicator.sprite = readySprite;}
            if(AREf == (int)ARE - 100) {AudioManager.PlayClip(goSE); readyGoIndicator.sprite = goSprite;}
            if(AREf == (int)ARE - 1) 
            {
                if(!GameEngine.instance.gameMusic.isPlaying) GameEngine.instance.gameMusic.Play();
                readyGoIndicator.sprite = null;
            }
            if (sectAfter20g < 1) DAS = 25;
            else if (sectAfter20g < 5) DAS = 15;
            else if (sectAfter20g < 9) DAS = 10;
            else if (sectAfter20g < 13) DAS = 3;
            else DAS = 1;
        }
        else if (lives <= 1)
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
            framestepped = Inputs[7];
        }
        //If you have more than 1 life
        else
        {
            frames++;
            if(frames == 1)
            {
                AREf = (int)ARE - 250;
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
            Inputs[7] = false;
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
