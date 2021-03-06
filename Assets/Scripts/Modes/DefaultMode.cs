using Discord;
using System;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using Tetro48.Interfaces;

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

//This is a gigantic mess that Tetro48 doesn't really want to mess with much.
public class DefaultMode : IMode
{
    public FixedString64Bytes Name { get; } = "Challenger Mode";
    public FixedString128Bytes Description { get; } = "The default mode of this project. Go as far as you can!";
    public double time, rollTime, rollTimeLimit = 11000, notifDelay, sectionlasttime, coolprevtime;
    // this is odd
    public NativeArray<int> clearedLines = new NativeArray<int>(40, Allocator.Persistent);
    public int level, sectionSize = 100;
    public int endingLevel = 2100;

    public int curSect, sectAfter20g;
    // Number of combos | Keeping combo
    public int comboCount, comboKeepCounter;
    public double gradePoints, statGradePoints, gradePointRequirement = 100;

    private int virtualBasePoint;

    public TextMeshPro levelTextRender, nextSecLv, timeCounter, rollTimeCounter, ppsCounter;
    public Slider gradePointSlider;

    public SpriteRenderer gradeIndicator;

    public Sprite[] gradeSprites;

    public int grade;

    public Sprite readySprite, goSprite;

    public int nextPieces, nextibmblocks;

    public RotationSystems RS;

    public bool TLS, tSpin, ending, coolchecked, previouscool;

    public bool lineFreezingMechanic, bigMode, oneshot;
    public bool LockDelayEnable;
    public int countLockResets, maxLockResets = 20;
    public double LockDelay = 50;
    public double DAS = 15;
    public double SDF = 6;
    public double spawnDelay = 41.66666666666666;
    public double spawnTicks = 42 - 300;
    public double lineSpawnDelay = 16.66666666666666666;
    public int lineDropTicks = 0;
    public double lineDropDelay = 25;
    public float gravity = 3 / 64f;

    public int frames;
    public double[] sectionTime = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

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

    public int[] lineClonePerPiece = { 2147483647, 2147483647, 20, 20, 20, 20, 20, 20, 20, 20, 16, 16, 16, 8, 8, 6, 5, 4, 3, 2, 2, 2 };

    public int lineClonePiecesLeft = 20;

    public double percentage = 0.8f;
    private static readonly int[] lvlLineIncrement = { 1, 3, 6, 10, 15, 21, 28, 36, 48, 70, 88, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90, 90 };

    public int[] linesFrozen = { 0, 0, 0, 6, 4, 0, 0, 0, 8, 0, 0, 12, 16, 0, 0, 0, 19, 0, 0, 0, 10, 14 };
    private bool cool, cooldisplayed;

    /** Line clear?????????????????? point */
    private static int[] tableGradePoint = { 10, 30, 60, 120, 180, 240, 300, 400, 520, 640, 780, 920, 1060, 1200, 1500, 1800, 2100, 2400, 3000, 4000, 5500, 7500, 10000 };
    [SerializeField] private GameObject rolltimeObject;

    /** ?????? point???Combo bonus */
    private static readonly float[,] tableGradeComboBonus =
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
    public int bgmlv = 1;
    private int[] tableBGMFadeout = { 385, 585, 680, 860, 950, 1440, -1, -1 };
    private int[] tableBGMChange = { 400, 600, 700, 900, 1000, 1500, 2100, -1 };
    private AudioSource gameMusic = GameEngine.instance.gameMusic;

    private void FadeoutBGM()
    {
        if (tableBGMFadeout[bgmlv - 1] != -1 && NetworkBoard.highestLevel >= tableBGMFadeout[bgmlv - 1])
        {
            gameMusic.volume -= (Time.deltaTime / 3);
        }
    }

    private void ChangeBGM()
    {
        if (tableBGMChange[bgmlv - 1] != -1 && NetworkBoard.highestLevel >= tableBGMChange[bgmlv - 1])
        {
            bgmlv += 1;
            gameMusic.volume = 1;
            gameMusic.Stop();
            gameMusic.clip = GameEngine.instance.bgm_1p_lv[bgmlv - 1];
            if (bgmlv < 7)
            {
                gameMusic.Play();
            }
        }
    }
    private void checkCool()
    {
        // COOL check
        if ((level % sectionSize >= sectionSize * 0.7) && (coolchecked == false && level <= sectionSize * cools.Length))
        {
            int section = level / sectionSize;

            if ((sectionTime[section] <= tableTimeCool[section]) &&
                ((previouscool == false) || ((previouscool == true) && (sectionTime[section] <= coolprevtime + 1))))
            {
                cool = true;
                cools[section] = SectionState.cool;
            }
            else
            {
                cools[section] = SectionState.missed;
            }

            coolprevtime = sectionTime[section];
            coolchecked = true;
        }

        // COOL????????????
        if ((level % sectionSize >= sectionSize * 0.82) && (cool == true) && (cooldisplayed == false))
        {
            AudioManager.PlayClip("cool");
            // cooldispframe = 180;
            cooldisplayed = true;
            virtualBasePoint += 600;
        }
    }

    /**
	 * REGRET?????? check
	 * @param engine GameEngine
	 * @param levelb Line clear????????????? level
	 */
    private void checkRegret(int levelb)
    {
        int section = levelb / sectionSize;
        if (sectionlasttime > tableTimeRegret[section])
        {

            virtualBasePoint -= 600;

            // regretdispframe = 180;
            AudioManager.PlayClip("regret");
            cools[section] = SectionState.regret;
        }
    }
    public void OnObjectSpawn(Transform transformRef)
    {
        throw new System.NotImplementedException();
    }
    public double GetSpawnDelay()
    {
        return spawnDelay;
    }

    public double GetLockDelay()
    {
        return LockDelay;
    }

    public double GetLineDropDelay()
    {
        return lineDropDelay;
    }

    public double GetLineSpawnDelay()
    {
        return lineSpawnDelay;
    }
    public double GetDAS()
    {
        double DAS;
        if (sectAfter20g < 1)
        {
            DAS = 25;
        }
        else if (sectAfter20g < 5)
        {
            DAS = 15;
        }
        else if (sectAfter20g < 9)
        {
            DAS = 10;
        }
        else if (sectAfter20g < 13)
        {
            DAS = 3;
        }
        else
        {
            DAS = 1;
        }

        return DAS;
    }
    //why is this necessary???
    public Activity GetDiscordActivity() => new Activity
    {
        State = "Playing a default mode.",
        Details = $"Level {level}/{endingLevel}",
        Assets = {
                LargeImage = "icon"
            },
        Timestamps = {
            Start = DateTime.Now.Ticks / 10000000 - (int)time,
        }
    };
    public void OnUpdate(float deltaTime, NetworkBoard board)
    {

        throw new NotImplementedException();
        // a ref???
        ref double LockTicks = ref board.LockTicks;

        checkCool();
        if (level > endingLevel)
        {
            level = endingLevel;
        }

        rolltimeObject.SetActive(ending);
        if (notifDelay > 0)
        {
            notifDelay--;
        }

        if (ending && spawnTicks >= 0)
        {
            board.tileInvisTime = 20 - ((int)rollTime / (400 / 6 * 10));
        }
        else
        {
            board.tileInvisTime = -1;
        }

        if (spawnTicks == (int)spawnDelay - 399)
        {
            AudioManager.PlayClip("excellent");
        }

        if (spawnTicks >= 0)
        {
            time += deltaTime;
        }

        if (spawnTicks >= 0 && ending && rollTime < rollTimeLimit)
        {
            rollTime += deltaTime;
            if (rollTime >= rollTimeLimit)
            {
                rollTime = rollTimeLimit;
                spawnTicks = (int)spawnDelay - 1000;
            }
        }
        if (spawnTicks == (int)spawnDelay - 401)
        {
            board.lives = 1;
            //This will stop the further execution of everything else.
            board.GameOver = true;
        }
        if (spawnTicks < (int)spawnDelay - 401)
        {
            if (spawnTicks % 10 == 0)
            {
                board.SpawnFireworks();
            }

            if (spawnTicks % 50 == 0 && grade < gradeSprites.Length - 1)
            {
                grade++;
                gradeIndicator.sprite = gradeSprites[grade];
                AudioManager.PlayClip("gradeUp");
            }
        }
        /*this could be:
        int nextsecint;
        if ((curSect + 1) * sectionSize > endingLevel) nextsecint = endingLevel;
        else if (level < endingLevel) nextsecint = (curSect + 1) * sectionSize;
        else nextsecint = endingLevel;
        up is a likely lowered one, down is written like what!?*/
        int nextsecint = (curSect + 1) * sectionSize > endingLevel ? endingLevel : level < endingLevel ? (curSect + 1) * sectionSize : endingLevel;

        levelTextRender.text = level.ToString();
        if (curSect < 21)
        {
            nextSecLv.text = nextsecint.ToString();
        }

        if (!ending)
        {
            timeCounter.text = TimeConversion.doubleFloatTimeCount(time);
            if (spawnTicks >= 0 && rollTime < rollTimeLimit)
            {
                sectionTime[curSect] += deltaTime;
            }
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

    public void OnLineClear(NetworkBoard boardRef, int lines, bool spin)
    {
        if (lines > 0)
        {
            comboKeepCounter = 2;
            if (lines > 1)
            {
                comboCount++;
                if (comboCount >= 2)
                {
                    int cmbse = comboCount - 2;
                    if (cmbse > GameEngine.instance.comboSE.Length - 1)
                    {
                        cmbse = GameEngine.instance.comboSE.Length - 1;
                    }

                    AudioManager.PlayClip(GameEngine.instance.comboSE[cmbse]);
                }
            }
        }
        if (level < endingLevel)
        {
            level += lvlLineIncrement[lines - 1];
        }
        if (level > endingLevel)
        {
            level = endingLevel;
        }

        if (level / sectionSize > curSect || level > endingLevel)
        {
            sectionlasttime = sectionTime[curSect];
            checkRegret(level - lvlLineIncrement[lines - 1]);
            curSect++;
            if (curSect > (endingLevel / sectionSize) - 1)
            {
                spawnTicks = spawnDelay - 400;
                ending = true;
            }
            AudioManager.PlayClip("levelup");
            if (curSect < 20)
            {
                BackgroundController.bginstance.TriggerBackgroundChange(curSect);
            }
            if (curSect % 5 == 0)
            {
                NotificationEngine.Notify(LanguageList.Extract(LangArray.notifications, MenuEngine.instance.language, 12), Color.white);
            }

            if (gravity >= 10)
            {
                spawnDelay *= percentage;
                lineSpawnDelay *= percentage;
                lineDropDelay *= percentage;
                LockDelay *= percentage;
                sectAfter20g++;
                //It's there to avoid locking right after a piece spawned.
                if (LockDelay < 1)
                {
                    LockDelay = 1.000001d;
                }
                if (gravity < 19.99999)
                {
                    gravity *= 4;
                }
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
            if (lines == 1)
            {
                virtualBasePoint += 10;
            }

            if (lines == 2)
            {
                virtualBasePoint += 20;
            }

            if (lines == 3)
            {
                virtualBasePoint += 30;
            }

            if (lines == 4)
            {
                virtualBasePoint += 50;
            }

            if (lines == 5)
            {
                virtualBasePoint += 70;
            }

            if (lines >= 6)
            {
                virtualBasePoint += 100 + (lines - 6) * 40;
            }
        }
        int basepoint = tableGradePoint[lines - 1];
        basepoint += virtualBasePoint;
        virtualBasePoint = 0;

        int indexcombo = comboCount - 1;
        if (indexcombo < 0)
        {
            indexcombo = 0;
        }

        if (indexcombo > 9)
        {
            indexcombo = 9;
        }

        float combobonus = tableGradeComboBonus[lines - 1, indexcombo];

        int levelbonus = 1 + (level / 250);

        float point = basepoint * combobonus * levelbonus;
        //point multiplication, huh?
        if (sectAfter20g >= 21)
        {
            point *= 10;
        }
        else if (sectAfter20g > 19)
        {
            point *= 5;
        }
        else if (sectAfter20g > 18)
        {
            point *= 2;
        }

        gradePoints += point;
        statGradePoints += point;
        while (gradePoints >= gradePointRequirement)
        {
            gradePoints -= gradePointRequirement;
            if (grade < gradeSprites.Length - 1)
            {
                grade++;
            }

            gradeIndicator.sprite = gradeSprites[grade];
            AudioManager.PlayClip("gradeUp");
            //turns out this has some quirk. it'll likely go off to near infinity.
            gradePointRequirement *= Math.Abs(1 + (Math.Abs(Math.Floor((double)level / sectionSize) + 1) / 4));
            gradePointSlider.maxValue = (float)gradePointRequirement;
        }
        gradePointSlider.value = (float)gradePoints;
        totalLines += lines;
        clearedLines[lines - 1]++;
    }

    public void OnLineDrop(NetworkBoard boardRef, int lines, bool spin)
    {
        throw new System.NotImplementedException();
    }

    public void OnBlockOut()
    {
        throw new System.NotImplementedException();
    }

    public int GetResets()
    {
        return 20;
    }

    public int GetBGMType()
    {
        return bgmlv;
    }

    public bool BeforeStart() => true;
}

