using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.UI;
using TMPro;

public class DefaultMode : IMode
{
    public double time, rollTime, rollTimeLimit = 11000, notifDelay, sectionlasttime, coolprevtime;
    public GameObject gameObject;
    // this is odd
    public NativeArray<int> clearedLines;
    public int level, sectionSize = 100;
    public int endingLevel = 2100;

    public int curSect, sectAfter20g;
    // Number of combos | Keeping combo
    public int comboCount, comboKeepCounter;
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
    public double LockDelay = 50, LockDelayf = 0;
    public double DAS = 15;
    public double SDF = 6;
    public double ARE = 41.66666666666666;
    public double AREf = 42 - 300;
    public double AREline = 16.66666666666666666;
    public int lineDelayf = 0;
    public double lineDelay = 25;
    public float gravity = 3/64f;

    public int frames;
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

    public int singles, doubles, triples, tetrises, pentrises, sixtrises, septrises, octrises;
    public int allClears;

    public int totalLines;

    public int[] lineClonePerPiece = {2147483647,2147483647,20,20,20,20,20,20,20,20,16,16,16,8,8,6,5,4,3,2,2,2};

    public int lineClonePiecesLeft = 20;

    public double percentage = 0.8f;
    static int[] lvlLineIncrement = {1, 3, 6, 10, 15, 21, 28, 36, 48, 70, 88, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90};

    public int[] linesFrozen = {0, 0, 0, 6, 4, 0, 0, 0, 8, 0, 0, 12, 16, 0, 0, 0, 19, 0, 0, 0, 10, 14};
    bool cool, cooldisplayed;
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
			AudioManager.PlayClip("cool");
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
			AudioManager.PlayClip("regret");
			cools[section] = SectionState.regret;
		}
	}
    public void OnObjectSpawn()
    {
        throw new System.NotImplementedException();
    }
    public double GetSpawnDelay()
    {
        throw new System.NotImplementedException();
    }

    public double GetLockDelay()
    {
        return LockDelay;
    }

    public double GetLineDropDelay()
    {
        throw new System.NotImplementedException();
    }

    public double GetLineSpawnDelay()
    {
        throw new System.NotImplementedException();
    }
    public double GetDAS()
    {
        double DAS;
        if (sectAfter20g < 1) DAS = 25;
        else if (sectAfter20g < 5) DAS = 15;
        else if (sectAfter20g < 9) DAS = 10;
        else if (sectAfter20g < 13) DAS = 3;
        else DAS = 1;
        return DAS;
    }
    public void OnUpdate(float deltaTime)
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
                AudioManager.PlayClip("step");
            }
            LockDelayf += Time.deltaTime / Time.fixedDeltaTime;
            if (LockDelayf >= LockDelay)
            {
                LockDelayEnable = false;
                SetPiece();
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
                Math.Floor(((double) piecesController.lockedPieces / time)* 100) / 100, SIUnitsConversion.doubleToSITime((LockDelay-LockDelayf)/100), SIUnitsConversion.doubleToSITime(LockDelay/100), maxLockResets - countLockResets, maxLockResets);
        }
        if (ReplayRecord.instance.mode == ReplayModeType.read && AREf > (int)ARE - 401)
        {
            float2 tempmov;
            tempmov = ReplayRecord.instance.movementVector[playerID][ReplayRecord.instance.frames[playerID]];
            movement = new Vector2(tempmov[0], tempmov[1]);
            // Inputs = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames[playerID]];
            Inputs = ReplayRecord.instance.inputs[playerID][ReplayRecord.instance.frames[playerID]];
            lineFreezingMechanic = ReplayRecord.instance.switches[playerID][0];
        }
        else if(AREf > (int)ARE - 401)
        {
            ReplayRecord.instance.inputs[playerID].Add(Inputs);
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
        if (comboKeepCounter == 0)
        {
            comboCount = 0;
        }
    }

    public void OnPieceSpawn(string piece_name)
    {
        throw new System.NotImplementedException();
    }

    public void OnPieceLock(string piece_name)
    {
        throw new System.NotImplementedException();
    }

    public void OnLineClear(int lines, bool spin)
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
            AudioManager.PlayClip("levelup");
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
            AudioManager.PlayClip("gradeUp");
            gradePointRequirement *= Math.Abs(1 + (Math.Abs(Math.Floor((double)level / sectionSize) + 1) / 4));
            gradePointSlider.maxValue = (float)gradePointRequirement;
        }
        gradePointSlider.value = (float)gradePoints;
        totalLines += lines;
        clearedLines[lines-1]++;
    }

    public void OnLineDrop(int lines, bool spin)
    {
        throw new System.NotImplementedException();
    }

    public void OnBlockOut()
    {
        throw new System.NotImplementedException();
    }
}
