using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using TMPro;

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
    public static List<NetworkBoard> player = new List<NetworkBoard>();
    public BoardController boardController;
    public BoardParticleSystem boardParticles;
    public PiecesController piecesController;
    public int playerID;
    public bool GameOver, IntentionalGameOver;
    public int time, rollTime, rollTimeLimit = 11000, notifDelay, sectionlasttime, coolprevtime;

    public int[] sectionTime = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};

    public SectionState[] cools = new SectionState[21];

    /// <summary>
    /// Section COOL criteria Time 
    /// </summary>
	public static int[] tableTimeCool =
	{
		5200, 5200, 4900, 4500, 4500, 4200, 4200, 3800, 3800, 3800, 3300, 3300, 3300, 2800, 2800, 2200, 1800, 1400, 900, 600, -1
	};

    /// <summary>
    /// Section REGRET criteria Time 
    /// </summary>
    public static int[] tableTimeRegret = 
    {
        9000, 7500, 7500, 6800, 6000, 6000, 5000, 5000, 500, 5000, 4500, 4500, 4500, 4000, 4000, 3400, 3000, 2600, 1700, 800, -1
    };

    public int level;

    public int endingLevel = 2100;

    public int curSect, sectAfter20g;

    // Number of combos | Keeping combo
    public int comboCount, comboKeepCounter;



    public AudioClip readySE, goSE, gradeUp, excellent, coolSE, regretSE;

    public double gradePoints, statGradePoints, gradePointRequirement;

    private int virtualBasePoint;

    public TextMeshPro levelTextRender, nextSecLv, timeCounter, rollTimeCounter, ppsCounter;

    public SpriteRenderer readyGoIndicator, gradeIndicator;

    public Sprite[] gradeSprites;

    public int grade;

    public Sprite readySprite, goSprite;

    public int nextPieces, nextibmblocks;

    public RotationSystems RS;

    public bool TLS, tSpin, ending, coolchecked, previouscool;

    public bool lineFreezingMechanic;

    [Range(0, 1000)]
    public double LockDelay = 50;

    [Range(0, 1000)]
    public double DAS = 15;

    [Range(0, 1000)]
    public double SDF = 6;

    [Range(0, 1000)]
    public double ARE = 41.66666666666666;

    [Range(-1005, 1000)]
    public int AREf = 42 - 300;

    public int frames;

    [Range(0, 1000)]
    public double AREline = 16.66666666666666666;

    public int lineDelayf = 0;

    [Range(0, 1000)]
    public double lineDelay = 25;

    [Range(0, 60)]
    public float gravity = 3/64f;

    public int singles, doubles, triples, tetrises, pentrises, sixtrises, septrises, octrises;

    public int totalLines;

    public int[] lineClonePerPiece = {2147483647,2147483647,20,20,20,20,20,20,20,20,16,16,16,8,8,6,5,4,3,2,2,2};

    public int lineClonePiecesLeft = 20;

    public float percentage = 0.8f;

    public bool paused, FrameStep, framestepped;

    public bool[] Inputs;

    public int tileInvisTime = -1;

    public Vector2 movement;
    [SerializeField] GameObject rolltimeObject;
    
    bool cool, cooldisplayed;
	private void checkCool() {
		// COOL check
		if((level % 100 >= 70) && (coolchecked == false && level < 2001)) {
			int section = level / 100;

			if( (sectionTime[section] <= tableTimeCool[section]) &&
				((previouscool == false) || ((previouscool == true) && (sectionTime[section] <= coolprevtime + 60))) )
			{
				cool = true;
				cools[section] = SectionState.cool;
			}
			else cools[section] = SectionState.missed;
			coolprevtime = sectionTime[section];
			coolchecked = true;
		}

		// COOLиЎЁз¤є
		if((level % 100 >= 82) && (cool == true) && (cooldisplayed == false)) {
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
		int section = levelb / 100;
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
        if(level/100 > curSect)
        {
            sectionlasttime = sectionTime[curSect];
            checkRegret(level - lvlLineIncrement[lines-1]);
            curSect++;
            if (curSect > (endingLevel/100) - 1)
            {
                AREf = (int)ARE - 400;
                ending = true;
            }
            AudioManager.PlayClip(piecesController.levelup);
            if (curSect < 20)
            {
                BackgroundController.bginstance.TriggerBackgroundChange(curSect);
            }
            if(curSect % 5 == 0) NotificationEngine.instance.InstantiateNotification(MenuEngine.instance.notifLangString[(int)MenuEngine.instance.language, 12],Color.white);
            if (gravity >= 12.5)
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
            if(cool == true) {
                previouscool = true;

            } else {
                previouscool = false;
            }

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
            gradePointRequirement *= Math.Abs(1 + (Math.Abs(Math.Floor((double)level / 100) + 1) / 4));
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
            if (value.ReadValue<Vector2>().y > 0.5) {Inputs[0] = true;}
            else {Inputs[0] = false;}
        }
        else if (value.performed)
        {
            if (value.ReadValue<Vector2>().x > 0.5 && Time.timeScale < 100)
                Time.timeScale += 1.0f;
            if (value.ReadValue<Vector2>().x < -0.5 && Time.timeScale > 1)
                Time.timeScale -= 1.0f;
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
        if(ReplayRecord.instance.mode != ReplayModeType.read)
        {
            ReplayRecord.instance.boards++;
            ReplayRecord.instance.movementVector.Add(new List<float[]>());
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
            checkCool();
            if(level > endingLevel) level = endingLevel;
            rolltimeObject.SetActive(ending);
            // musicTime += Time.deltaTime;
            if(notifDelay > 0)notifDelay--;
            if(MenuEngine.instance.curBoard != null)
            {
                int pieceCountHoldRed = piecesController.pieceHold == 28 ? -1 : -2;
                if(time > 0)
                ppsCounter.text = Math.Floor(((double) (piecesController.pieces + pieceCountHoldRed - piecesController.relativeNextPieceCoordinates.Count) / ((double)time/100))*100)/100 + " pieces/second";
            }
            // if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Space)) Inputs[2] = true;
            // if (Input.GetKey(KeyCode.A)) Inputs[3] = true;
            // if (Input.GetKey(KeyCode.C)) Inputs[4] = true;
            // if (Input.GetKeyDown(KeyCode.P) && MenuEngine.instance.curBoard != null) paused = !paused;
            if((paused == false || (FrameStep == true && Inputs[7])) && GameOver == false)
            {
                if (ReplayRecord.instance.mode == ReplayModeType.read && AREf > (int)ARE - 401)
                {
                    Vector2 tempmov;
                    tempmov.x = ReplayRecord.instance.movementVector[playerID][ReplayRecord.instance.frames][0];
                    tempmov.y = ReplayRecord.instance.movementVector[playerID][ReplayRecord.instance.frames][1];
                    movement = tempmov;
                    // Inputs = ReplayRecord.instance.inputs[ReplayRecord.instance.frames];
                    Inputs[0] = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames][0];
                    Inputs[1] = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames][1];
                    Inputs[2] = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames][2];
                    Inputs[3] = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames][3];
                    Inputs[4] = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames][4];
                    Inputs[5] = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames][5];
                    Inputs[6] = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames][6];
                    lineFreezingMechanic = ReplayRecord.instance.switches[playerID][0];
                }
                else if(AREf > (int)ARE - 401)
                {
                    bool[] modInputs = Inputs;
                    modInputs[7] = false;
                    ReplayRecord.instance.inputs[playerID].Add(modInputs);
                    float[] modMovement = { movement.x, movement.y };
                    ReplayRecord.instance.movementVector[playerID].Add(modMovement);
                }
                if (level >= endingLevel && AREf < (int)ARE && AREf > (int)ARE - 400)
                {
                    int whichline = ((AREf - (int)ARE)+400)/10;
                    if(GameEngine.debugMode) Debug.Log(whichline);
                    boardController.DestroyLine(whichline);
                }
                
                if(ending)tileInvisTime = 20 - (rollTime / (400/6*10));
                else tileInvisTime = -1;
                if (AREf == (int)ARE - 399) AudioManager.PlayClip(excellent);
                if(AREf >= 0 && readyGoIndicator.sprite == null && rollTime < rollTimeLimit)time++;
                if(AREf >= 0 && readyGoIndicator.sprite == null && ending && rollTime < rollTimeLimit)
                {
                    rollTime++;
                    if(rollTime >= rollTimeLimit)
                    {
                        AREf = (int)ARE - 1000;
                        Destroy(piecesController.piecesInGame[piecesController.piecesInGame.Count-1]);
                        piecesController.UpdatePieceBag();
                    }
                }
                if (AREf == (int)ARE - 401)
                {

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
                int nextsecint = level < endingLevel ? (curSect + 1) * 100 : endingLevel;
                levelTextRender.text = level.ToString();
                if(curSect < 21)nextSecLv.text = nextsecint.ToString();
                if(!ending)
                {
                    timeCounter.text = TimeConversion.timeCount(time);
                    if(AREf >= 0 && readyGoIndicator.sprite == null && rollTime < rollTimeLimit)sectionTime[curSect]++;
                }
                rollTimeCounter.text = TimeConversion.timeCount(rollTimeLimit - rollTime);
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
            if(AREf == (int)ARE - 200 && level < 100) {AudioManager.PlayClip(readySE); readyGoIndicator.sprite = readySprite;}
            if(AREf == (int)ARE - 100 && level < 100) {AudioManager.PlayClip(goSE); readyGoIndicator.sprite = goSprite;}
            if(AREf == (int)ARE - 1 && level < 100) readyGoIndicator.sprite = null;
            if (sectAfter20g < 1) DAS = 25;
            else if (sectAfter20g < 5) DAS = 15;
            else if (sectAfter20g < 9) DAS = 10;
            else if (sectAfter20g < 13) DAS = 3;
            else DAS = 1;
        }
        else
        {
            readyGoIndicator.sprite = null;
            GameEngine.instance.gameMusic.Stop();
            if(frames%10==9 && frames<400)boardController.DestroyLine(frames/10);
            if(frames<400)boardController.DecayLine(frames/10, 0.1f);
            frames++;
            if(frames > 300)
            {
                transform.localPosition -= new Vector3(0f, 0.005f * (frames-300*frames-300*frames-300), 0f);
                // boardrot = transform.Rotate;
                // boardrot.z -= 0.16f;
                transform.Rotate(new Vector3(0f, 0f, -0.1f - (float)(((frames-300) / 66) * ((frames-400) / 66))));
                if (frames == 301)
                {
                    MenuEngine.instance.audioSource2.PlayOneShot(MenuEngine.instance.topoutSE);
                }
                if (frames == 351)
                {
                    if(player.Count < 2) MenuEngine.instance.audioSource2.Play();
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
                    Destroy(gameObject);
                }
            }
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
            NotificationEngine.instance.InstantiateNotification(MenuEngine.instance.notifLangString[(int)MenuEngine.instance.language, 13], Color.white); notifDelay = 300;
        } 
    }
    public void ShowGradeScore() 
    {
        if (notifDelay == 0) 
        {
            NotificationEngine.instance.InstantiateNotification(MenuEngine.instance.notifLangString[(int)MenuEngine.instance.language, 14], Color.white);
            NotificationEngine.instance.InstantiateNotification(Math.Floor(gradePoints).ToString(), Color.white);
            NotificationEngine.instance.InstantiateNotification("/" + Math.Floor(gradePointRequirement), Color.white);
            notifDelay = 200;
        }
    }
}
