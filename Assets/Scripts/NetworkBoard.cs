using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/*
    Project Challenger, a challenging block stacking game.
    Copyright (C) 2022-2023, Aymir

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
    public TextMeshPro earnedScoreText;
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
    public double gravity = 3 / 64f;

    public int frames;

    public int[] clearedLinesArray = new int[8];
    public int allClears;

    public int totalLines;

    public int[] lineClonePerPiece = { 2147483647, 2147483647, 20, 20, 20, 20, 20, 20, 20, 20, 16, 16, 16, 8, 8, 6, 5, 4, 3, 2, 2, 2 };

    public int lineClonePiecesLeft = 20;

    public double percentage = 0.8f;

    public bool paused, FrameStep, framestepped;

    public bool4x2 Inputs;
    private static int[] lvlLineIncrement = { 1, 3, 6, 10, 15, 21, 28, 36, 48, 70, 88, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90 };

    public int[] linesFrozen = { 0, 0, 0, 6, 4, 0, 0, 0, 8, 0, 0, 12, 16, 0, 0, 0, 19, 0, 0, 0, 10, 14 };
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
    private static int2 V3ToInt2(Vector3 vector3) => new int2(Mathf.FloorToInt(vector3.x + 0.5f), Mathf.FloorToInt(vector3.y + 0.5f));
    private static Vector2Int int2ToV2Int(int2 integers) => new Vector2Int(integers.x, integers.y);
    #endregion

    #region Piece handling
    [Header("Piece")]
    public int3[] activePiece;
    private int3[] ghostPiece;
    public float2 pivot { get; private set; }
    [SerializeField]
    private GameObject tileRotation;
    public PieceType curType;
    public int rotationIndex { get; private set; }
    public bool fullyLocked, harddrop;
    public int tileInvisTime = -1;

    public float2 movement;

    public BoardRenderer boardRenderer;

    public void RenderPiece(int3[] piece, float alpha = 1f, bool offset = true)
    {
        for (int i = 0; i < piece.Length; i++)
        {
            float2 smoothfall_offset = new float2(0, (float)-piecesController.gravityTiles);
            if (!CanMovePiece(new int2(0, -1)))
            {
                smoothfall_offset = float2.zero;
            }
            if (!offset) smoothfall_offset.y = 0;
            boardRenderer.RenderBlock(piece[i].xy + smoothfall_offset, piece[i].z, 0, 1, alpha);
        }
    }
    private void DropGhostPiece()
    {
        if (ghostPiece.Length == 0)
        {
            return;
        }
        int2 movement = int2.zero;
        bool ghostFloor = false;
        while (!ghostFloor)
        {
            movement.y--;
            for (int i = 0; i < ghostPiece.Length; i++)
            {
                if (!CanTileMove(movement + ghostPiece[i].xy))
                {
                    ghostFloor = true;
                    movement.y++;
                }
            }
        }
        for (int i = 0; i < ghostPiece.Length; i++)
        {
            ghostPiece[i].y += movement.y;
        }
    }

    private void ProcessGhostPiece()
    {
        ghostPiece = (int3[]) activePiece.Clone();
        DropGhostPiece();
    }
    public void SpawnPiece(int textureID, int2[] tiles, float2 setPivot, PieceType type)
    {
        rotationIndex = 0;
        piecesController.piecemovementlocked = false;
        fullyLocked = false;
        harddrop = false;
        pivot = setPivot + new float2(4, 22);
        curType = type;
        activePiece = new int3[tiles.Length];
        for (int i = 0; i < tiles.Length; i++)
        {
            activePiece[i] = new int3(tiles[i] + new int2(4, 22), textureID);
        }
        ghostPiece = (int3[]) activePiece.Clone();
        DropGhostPiece();
    }
    public void SwapPiece(int3[] tiles, float2 setPivot, PieceType type)
    {
        rotationIndex = 0;
        piecesController.piecemovementlocked = false;
        fullyLocked = false;
        harddrop = false;
        pivot = setPivot + new float2(4, 22);
        curType = type;
        activePiece = tiles;
    }
    public bool MovePiece(int2 movement, bool offset)
    {
        if (activePiece.Length == 0 || fullyLocked)
        {
            return false;
        }
        for (int i = 0; i < activePiece.Length; i++)
        {
            if (!CanTileMove(movement + activePiece[i].xy))
            {
                Debug.Log("Cant Go there!");
                if (int2ToV2Int(movement) == Vector2Int.down && harddrop == true)
                {
                    SetPiece();
                }
                return false;
            }
        }

        // boardController.UpdateActivePiece(activePiece, true);
        UnisonPieceMove(movement);
        LockTicks = 0;
        if (movement.y >= 0)
        {
            if (!offset)
            {
                if (LockDelay > 5 || gravity < 19)
                {
                    AudioManager.PlayClip("move");
                }
            }
        }

        if (!CanMovePiece(new int2(0, -1)))
        {
            countLockResets++;
        }

        if (countLockResets >= maxLockResets)
        {
            LockTicks = LockDelay;
        }
        if (!CanMovePiece(new int2(0, -1)) && fullyLocked == false)
        {
            if (LockDelayEnable == false && piecesController.piecemovementlocked == false)
            {
                LockTicks = 0; LockDelayEnable = true;
            }
        }
        else
        {
            LockDelayEnable = false;
        }

        ghostPiece = activePiece.Clone() as int3[];
        DropGhostPiece();
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
            activePiece[i].xy = RotateObject(activePiece[i].xy, pivot, clockwise, UD);
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
    public static int2 RotateObject(int2 tilePos, float2 pivotPos, bool clockwise, bool UD = false)
    {
        int multi = clockwise ? 1 : -1;
        if (UD)
        {
            multi *= 2;
        }
        float rads = math.radians(90 * multi);
        float2 tilePos_f2 = (float2)tilePos;
        tilePos_f2 -= pivotPos;
        tilePos_f2 = tilePos_f2.Rotate(-rads);
        tilePos_f2 += pivotPos;
        return (int2)math.floor(tilePos_f2+0.1f);
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
        if (UD)
        {
            rotationIndex += clockwise ? 1 : -1;
        }

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
            if (curType == PieceType.O)
            {
                curOffsetData = piecesController.O_OFFSET_DATA;
            }
            else if (curType == PieceType.I)
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
            if (bigMode)
            {
                endOffset *= 2;
            }
            MovePiece(endOffset, true);
            return;
        }

        bool canOffset = Offset(oldRotationIndex, rotationIndex, clockwise, UD);

        if (!canOffset)
        {
            Debug.Log("Couldn't offset");
            rotationIndex = oldRotationIndex;
        }
    }

    private static int Mod(int x, int m)
    {
        return (x % m + m) % m;
    }

    private bool Offset(int oldRotIndex, int newRotIndex, bool clockwise, bool UD = false)
    {
        int2 offsetVal1, offsetVal2, endOffset;
        int2[,] curOffsetData;

        if (curType == PieceType.O)
        {
            curOffsetData = piecesController.O_OFFSET_DATA;
        }
        else if (curType == PieceType.I)
        {
            curOffsetData = piecesController.I_OFFSET_DATA;
        }
        else
        {
            curOffsetData = piecesController.JLSTZ_OFFSET_DATA;
        }

        endOffset = int2.zero;

        bool movePossible = false;

        for (int testIndex = 0; testIndex < curOffsetData.GetLength(0); testIndex++)
        {
            offsetVal1 = curOffsetData[testIndex, oldRotIndex];
            offsetVal2 = curOffsetData[testIndex, newRotIndex];
            endOffset = offsetVal1 - offsetVal2;
            if (bigMode)
            {
                endOffset *= 2;
            }

            if (testIndex == 0)
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
        else
        {
            RotateInUnison(!clockwise, UD);
        }

        if (LockDelay > 6 || gravity < 19)
        {
            AudioManager.PlayClip(rotateSE);
        }
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
        for (int i = 0; i < activePiece.Length; i++)
        {
            if (!boardController.SetTile(activePiece[i]))
            {
                if (GameEngine.debugMode)
                {
                    Debug.Log("GAME OVER!");
                }

                GameOver = true;
                piecesController.GameOver();
            }
        }
        if (GameOver == false)
        {
            if (Input.GetKey(KeyCode.E) && GameEngine.debugMode)
            {
                int incrementbyfrozenlines = lineFreezingMechanic ? linesFrozen[curSect] : 0;
                boardController.FillLine(0 + incrementbyfrozenlines);
                boardController.FillLine(1 + incrementbyfrozenlines);
                boardController.FillLine(2 + incrementbyfrozenlines);
                boardController.FillLine(3 + incrementbyfrozenlines);
            }
            boardController.CheckLineClears();
            piecesController.UpdatePieceBag();
        }
    }
    public void SendPieceToFloor()
    {
        if (activePiece == null || fullyLocked)
        {
            return;
        }
        harddrop = true;
        piecesController.PrevInputs.c0.x = true;
        AudioManager.PlayClip(hardDropSE);
        while (MovePiece(new int2(0, -1), true)) { }
    }
    #endregion

    [Header("UI?")]
    [SerializeField] GameObject rolltimeObject;
    
    private int cooldispframe = 0;
    private bool cool, cooldisplayed;
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
			cooldispframe = 300;
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
                spawnTicks = spawnDelay - 400;
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
                spawnDelay *= percentage;
                lineSpawnDelay *= percentage;
                lineDropDelay *= percentage;
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
            previouscool = cool;

            cool = false;
            coolchecked = false;
            cooldisplayed = false;
        }
        if (virtualBasePoint >= 600)
        {
            AudioManager.PlayClip("b2b_continue");
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
	
		float levelbonus = 1f + (level / 250f);
	
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
            gradePointRequirement *= 1 + Math.Min((Math.Floor((double)level / sectionSize) + 1) / 4, 2f);
            gradePointSlider.maxValue = (float)gradePointRequirement;
        }
        gradePointSlider.value = (float)gradePoints;
        totalLines += lines;

        #region Floating score text
        var sysRand = new System.Random();
        var random = new Unity.Mathematics.Random((uint)sysRand.Next(int.MinValue, int.MaxValue));
        TextMeshPro scoreText = Instantiate(earnedScoreText);
        scoreText.transform.localPosition = new Vector3(activePiece[0].x, activePiece[0].y, 0);
        scoreText.gameObject.SetActive(true);
        scoreText.text = string.Format("+ {0:.#}", point);
        Rigidbody2D rigidbody;
        rigidbody = scoreText.gameObject.AddComponent<Rigidbody2D>();
        rigidbody.mass = 1;
        rigidbody.AddForce(random.NextFloat2Direction() * 1000);
        rigidbody.angularDrag = 1f;
        Destroy(scoreText.gameObject, 2f);
        #endregion

        if (lines > clearedLinesArray.Length)
        {
            clearedLinesArray[clearedLinesArray.Length]++;
        }
        else
        {
            clearedLinesArray[lines-1]++;
        }
    }
    public void OnGameQuit()
    {
        GameOver = true;
        IntentionalGameOver = true;
        lives = 1;
        MenuEngine.instance.mainPlayer.frames = 300;
        // else
        // {
        //     curPieceController.SetPiece();
        // }
    }
    public void OnMovement(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read)
        {
            movement = value.ReadValue<Vector2>();
            if (movement.y > 0.5) { Inputs.c0.x = true; }
            else { Inputs.c0.x = false; }
        }
        else if (value.performed)
        {
            if (movement.x > 0.5 && Time.timeScale < 10)
            {
                Time.timeScale += 0.1f;
            }

            if (movement.x < -0.5 && Time.timeScale > .1)
            {
                Time.timeScale -= 0.1f;
            }

            GameEngine.instance.gameMusic.pitch = Time.timeScale;
        }
    }
    public void OnCounterclockwise(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read)
        {
            if (value.performed)
            {
                Inputs.c0.y = true;
            }
            else
            {
                Inputs.c0.y = false;
            }
        }
    }
    public void OnClockwise(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read)
        {
            if (value.performed)
            {
                Inputs.c0.z = true;
            }
            else
            {
                Inputs.c0.z = false;
            }
        }
    }
    public void OnClockwise2(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read)
        {
            if (value.performed)
            {
                Inputs.c1.z = true;
            }
            else
            {
                Inputs.c1.z = false;
            }
        }
    }
    public void OnUpsideDown(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read)
        {
            if (value.performed)
            {
                Inputs.c0.w = true;
            }
            else
            {
                Inputs.c0.w = false;
            }
        }
    }
    public void OnHold(InputAction.CallbackContext value)
    {
        if (ReplayRecord.instance.mode != ReplayModeType.read)
        {
            if (value.performed)
            {
                Inputs.c1.x = true;
            }
            else
            {
                Inputs.c1.x = false;
            }
        }
    }
    public void OnPause(InputAction.CallbackContext value)
    {
        if (value.started && player.Count <= 1 && !paused)
        {
            MenuEngine.instance.isGamePaused = true;
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

    private void Awake()
    {
        Inputs = new bool4x2(false);
        UnityEngine.Random.InitState(SeedManager.seed);
        if (ReplayRecord.instance.mode != ReplayModeType.read)
        {
            ReplayRecord.instance.switches[player.Count] = new bool[3] { lineFreezingMechanic, bigMode, oneshot };
        }

        if (player.Count > 0)
        {
            playerID = player.Count;
        }
        else
        {
            playerID = 0;
        }

        boardController.playerID = playerID;
        piecesController.playerID = playerID;
        // if(IsOwner)
        {
            MenuEngine.instance.mainPlayer = this;
            MenuEngine.instance.curBoard = gameObject;
        }
        player.Add(this);
    }

    // This is only for rendering purposes.
    private void Update()
    {
        if (!fullyLocked)
        {
            RenderPiece(activePiece);
            if (ghostPiece != null && (TLS || curSect == 0))
            {
                RenderPiece(ghostPiece, 0.5f, false);
            }
        }
    }
    private void FixedUpdate()
    {
        // if(!NetworkObject.IsSpawned) NetworkObject.Spawn();
        if (level > highestLevel && !GameOver)
        {
            highestLevel = level;
        }
        // if (IsOwner)
        {
            NetworkUpdate();
        }
    }

    // Update is called once per frame
    private void NetworkUpdate()
    {
        if (!GameOver)
        {
            if(framestepped && activePiece != null)
            {
                if (!LockDelayEnable)  
                {
                    if(!CanMovePiece(new int2(0,-1)) && !fullyLocked)  
                    {
                        LockTicks = 0;  LockDelayEnable = true;
                    }
                    else LockDelayEnable = false;
                }
            
                if(LockDelayEnable && !harddrop && !fullyLocked)
                {
                    if(LockTicks == 0 && LockDelay > 4)
                    {
                        AudioManager.PlayClip(audioPieceStep);
                    }
                    LockTicks += Time.deltaTime / Time.fixedDeltaTime;
                    if (LockTicks >= LockDelay)
                    {
                        LockDelayEnable = false;
                        SetPiece();
                    }
                }
            }
            cooldispframe--;
            if (cool) levelTextRender.outlineColor = Color.yellow;
            else levelTextRender.outlineColor = Color.black;
            if(cooldispframe % 4 >= 2 && cooldispframe > 0) levelTextRender.color = Color.yellow;
            else levelTextRender.color = Color.white;
            checkCool();
            if(level > endingLevel) level = endingLevel;
            rolltimeObject.SetActive(ending);
            // musicTime += Time.deltaTime;
            if(notifDelay > 0)notifDelay--;
            if(MenuEngine.instance.curBoard != null)
            {
                if(time > 0)
                ppsCounter.text = String.Format("{0} pieces/second\nLock: {1} / {2}\nResets: {3} / {4}",
                    Math.Floor(((double) piecesController.lockedPieces / time)* 100) / 100, SIUnitsConversion.doubleToSITime((LockDelay-LockTicks)/100), SIUnitsConversion.doubleToSITime(LockDelay/100), maxLockResets - countLockResets, maxLockResets);
            }
            // if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Space)) Inputs.c0.z = true;
            // if (Input.GetKey(KeyCode.A)) Inputs.c0.w = true;
            // if (Input.GetKey(KeyCode.C)) Inputs.c1.x = true;
            // if (Input.GetKeyDown(KeyCode.P) && MenuEngine.instance.curBoard != null) paused = !paused;
            if((paused == false || (FrameStep == true && Inputs.c1.w)) && GameOver == false)
            {
                if (ReplayRecord.instance.mode == ReplayModeType.read && spawnTicks > (int)spawnDelay - 401)
                {
                    float2 tempmov;
                    tempmov = ReplayRecord.instance.movementVector[playerID][ReplayRecord.instance.frames[playerID]];
                    movement = new Vector2(tempmov[0], tempmov[1]);
                    // Inputs = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames[playerID]];
                    Inputs = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames[playerID]];
                    lineFreezingMechanic = ReplayRecord.instance.switches[playerID][0];
                }
                else if(spawnTicks > (int)spawnDelay - 401)
                {
                    ReplayRecord.instance.inputs[playerID].Add(Inputs);
                    float2 modMovement = new float2(movement.x, movement.y);
                    ReplayRecord.instance.movementVector[playerID].Add(modMovement);
                }
                if (level >= endingLevel && spawnTicks < (int)spawnDelay && spawnTicks > (int)spawnDelay - 400)
                {
                    double whichline = ((spawnTicks - spawnDelay)+400)/10;
                    if(GameEngine.debugMode) Debug.Log(whichline);
                    boardController.DestroyLine((int)whichline);
                }
                
                if(ending && spawnTicks >= 0)tileInvisTime = 20 - ((int)rollTime / (400/6*10));
                else tileInvisTime = -1;
                if (spawnTicks == (int)spawnDelay - 399) AudioManager.PlayClip(excellent);
                if(spawnTicks >= 0 && readyGoIndicator.sprite == null && rollTime < rollTimeLimit)time += Time.deltaTime;
                if(spawnTicks >= 0 && readyGoIndicator.sprite == null && ending && rollTime < rollTimeLimit)
                {
                    rollTime += Time.deltaTime;
                    if(rollTime >= rollTimeLimit)
                    {
                        rollTime = rollTimeLimit;
                        spawnTicks = (int)spawnDelay - 1000;
                        piecesController.UpdatePieceBag();
                    }
                }
                if (spawnTicks == (int)spawnDelay - 401)
                {
                    lives = 1;
                    GameOver = true;
                }
                if(spawnTicks < (int)spawnDelay - 401)
                {
                    if(spawnTicks % 10 == 0) SpawnFireworks();
                    if(spawnTicks % 50 == 0 && grade < gradeSprites.Length - 1)
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
                    if(spawnTicks >= 0 && readyGoIndicator.sprite == null && rollTime < rollTimeLimit)sectionTime[curSect] += Time.deltaTime;
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
            Inputs.c1.w = false;
            if (spawnTicks == (int)spawnDelay - 200) { AudioManager.PlayClip(readySE); readyGoIndicator.sprite = readySprite; }
            if (spawnTicks == (int)spawnDelay - 100) { AudioManager.PlayClip(goSE); readyGoIndicator.sprite = goSprite; }
            if (spawnTicks == (int)spawnDelay - 1)
            {
                if (!GameEngine.instance.gameMusic.isPlaying)
                {
                    GameEngine.instance.gameMusic.Play();
                }

                readyGoIndicator.sprite = null;
            }
            if (sectAfter20g < 1) DAS = 25;
            else if (sectAfter20g < 5) DAS = 15;
            else if (sectAfter20g < 9) DAS = 10;
            else if (sectAfter20g < 13) DAS = 3;
            else DAS = 1;
        }
        else if (lives < 2)
        {
            readyGoIndicator.sprite = null;
            if (frames % 10 == 9 && frames < 400)
            {
                boardController.DestroyLine(frames / 10);
            }

            if (frames < 400)
            {
                boardController.DecayLine(frames / 10, 0.1f);
            }

            frames++;
            if (frames == 1)
            {
                GameEngine.instance.gameMusic.Stop();
            }

            if (player.Count > 1 && frames == 1)
            {
                frames = 300;
            }

            if (frames > 300)
            {
                if (ending)
                {
                    transform.position += Vector3.up * Mathf.Log10(frames) * 0.043f;
                }
                if (frames == 301)
                {
                    GameEngine.instance.gameMusic.Stop();
                    if (highestLevel == level)
                    {
                        highestLevel = 0;
                    }

                    if (!ending)
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
                    if (player.Count < 2 && !ending)
                    {
                        MenuEngine.instance.audioSource2.Play();
                    }
                    else
                    {
                        AudioManager.PlayClip(excellent);
                    }

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
                        if (oneshot && curSect > 2)
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
        else if (paused && !framestepped)
        {
            framestepped = Inputs.c1.w;
        }
        //If you have more than 1 life
        else
        {
            frames++;
            if (frames == 1)
            {
                spawnTicks = (int)spawnDelay - 250;
                if (IntentionalGameOver)
                {
                    Destroy(piecesController.curPieceController.gameObject);
                }

                MenuEngine.instance.audioSource2.PlayOneShot(MenuEngine.instance.topoutSE);
            }
            if (frames < 80)
            {
                boardController.DecayLine(39 - frames / 2, 0.5f);
                if (frames % 2 == 1)
                {
                    if (boardController.TilesInALine(39 - frames / 2) > 0)
                    {
                        AudioManager.PlayClip(lineClearTopout);
                    }

                    boardController.DestroyLine(39 - frames / 2);
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
        boardParticles.SummonFirework(new Vector2(0f, 10f), new Vector2(10f, 10f));
    }
    public void DisconnectGameOver()
    {
        if (MenuEngine.instance.curBoard != null)
        {
            GameOver = true;
        }
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
