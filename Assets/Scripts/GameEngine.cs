using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
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

public enum RotationSystems {SRS, ARS}
public class GameEngine : MonoBehaviour
{
    public int time, rollTime, rollTimeLimit = 6600, notifDelay;
    public int level;
    public int endingLevel = 2100;
    public int curSect, sectAfter20g;
    // Number of combos | Keeping combo
    public int comboCount, comboKeepCounter;
    public AudioClip[] comboSE;
    private double musicTime;
    public AudioSource gameAudio;
    public AudioClip[] bgm_1p_lv;
    public string audioPath;
    public AudioClip readySE, goSE, gradeUp, excellent;
    public int bgmlv = 1;
    public double gradePoints, statGradePoints, gradePointRequirement;
    private int virtualBasePoint;
    public TextMeshPro levelTextRender, nextSecLv, timeCounter, rollTimeCounter, ppsCounter;
    public static GameEngine instance;
    public SpriteRenderer readyGoIndicator, gradeIndicator;
    public Sprite[] gradeSprites;
    public int grade;
    public Sprite readySprite, goSprite;
    public int nextPieces, nextibmblocks;
    public RotationSystems RS;
    public bool TLS;
    public bool tSpin;
    public bool ending;
    public bool lineFreezingMechanic;
    public double LockDelay = 50;    
    public double DAS = 15;
    public double SDF = 6;
    public double ARE = 41.66666666666666;
    public int AREf = 42 - 300;
    public double AREline = 16.66666666666666666;
    public int lineDelayf = 0;
    public double lineDelay = 25;
    public float gravity = 3/64f;
    public int singles, doubles, triples, tetrises, pentrises, sixtrises, septrises, octrises;
    public int totalLines;
    public int[] lineClonePerPiece = {2147483647,2147483647,20,20,20,20,20,20,20,20,16,16,16,8,8,6,5,4,3,2,2,2};
    public int lineClonePiecesLeft = 20;

    public float percentage = 0.8f;

    public bool paused, FrameStep, framestepped;
    public bool[] Inputs;
    public bool[] HoldInputs;
    public Vector2 movement;


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
                    if(cmbse > 20) cmbse = 20;
                    gameAudio.PlayOneShot(comboSE[cmbse]);
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
            curSect++;
            if (curSect > (endingLevel/100) - 1)
            {
                AREf = (int)ARE - 400;
                ending = true;
            }
            PiecesController.instance.gameAudio.PlayOneShot(PiecesController.instance.levelup);
            if (curSect < 20)
            {
                BackgroundController.bginstance.TriggerBackgroundChange(curSect);
            }
            if(curSect % 5 == 0) NotificationEngine.instance.InstantiateNotification(MenuEngine.instance.notifLangString[(int)MenuEngine.instance.language, 12],Color.white);
            if (gravity >= 12.5)
            {
                ARE = (double)(ARE * percentage);
                AREline = (double)(AREline * percentage);
                lineDelay = (double)(lineDelay * percentage);
                LockDelay = (double)(LockDelay * percentage);
                sectAfter20g++;
                if(LockDelay < 1)
                {
                    LockDelay = 1.000001d;
                }
                if (gravity >= 19.99999) gravity = gravity * 4;
            }
            else
            {
                gravity = gravity * 4;
            }
        }
        if (spin)
        {
            if(lines == 1)virtualBasePoint += 10;
            if(lines == 2)virtualBasePoint += 20;
            if(lines == 3)virtualBasePoint += 30;
            if(lines == 4)virtualBasePoint += 50;
            if(lines == 5)virtualBasePoint += 70;
            if(lines >= 6)virtualBasePoint += (100 + (lines-6)*40);
        }
		int basepoint = tableGradePoint[lines - 1];
        basepoint += virtualBasePoint;
        virtualBasePoint = 0;

        int indexcombo = comboCount - 1;
        if(indexcombo < 0) indexcombo = 0;
        float combobonus = tableGradeComboBonus[lines - 1, indexcombo];
	
		int levelbonus = 1 + (level / 250);
	
		float point = (basepoint * combobonus) * levelbonus;
        if(sectAfter20g >= 21) point *= 10;
        else if(sectAfter20g > 19) point *= 5;
        else if(sectAfter20g > 18) point *= 2;
		gradePoints += point;
		statGradePoints += point;
        while(gradePoints >= gradePointRequirement)
        {
			gradePoints -= gradePointRequirement;
            if(grade<18)grade++;
            gradeIndicator.sprite = gradeSprites[grade];
            gameAudio.PlayOneShot(gradeUp);
            gradePointRequirement *= Math.Abs(1 + ((Math.Abs(Math.Floor((double)level /100)+1)/4)));
        }
        totalLines += lines;
        if(lines == 1) singles++;
        if(lines == 2) doubles++;
        if(lines == 3) triples++;
        if(lines == 4) tetrises++;
        if(lines == 5) pentrises++;
        if(lines == 6) sixtrises++;
        if(lines == 7) septrises++;
        if(lines > 7) octrises++;
    }

    // // Start is called before the first frame update
    // private void Awake()
    // {
    // }
    void Start()
    {
        // DontDestroyOnLoad(this);
        instance = this;
        audioPath = "file:///" + Application.dataPath + "/BGM/";
        Debug.Log(audioPath);
        for (int i = 0; i < 7; i++)
        {
            StartCoroutine(LoadLevelMusic(i));
        }
        StartCoroutine(LoadMainMenuMusic());
    }
    private IEnumerator LoadLevelMusic(int lv)
    {
        WWW request;
        if(lv < 6) request = GetAudioFromFile(audioPath, "BGM_1p_lv"+(lv+1)+".wav");
        else request = GetAudioFromFile(audioPath, "BGM_ending2.wav");
        Debug.Log(request);
        yield return request;

        bgm_1p_lv[lv] = request.GetAudioClip();
        bgm_1p_lv[lv].name = "BGM_" + (lv < 6 ? "1p_lv"+(lv+1)+"" : "ending2");
        if (lv == 0) gameAudio.clip = bgm_1p_lv[0];
    }
    AudioClip MMmusic;
    private IEnumerator LoadMainMenuMusic()
    {
        WWW request = GetAudioFromFile(audioPath, "BGM_player_data.wav");
        Debug.Log(request);
        yield return request;

        MMmusic = request.GetAudioClip();
        MMmusic.name = ("BGM_player_data");
        MenuEngine.instance.mainMenuMusic.clip = MMmusic;
        MenuEngine.instance.mainMenuMusic.Play();
    }
    public WWW GetAudioFromFile(string path, string filename)
    {
        string audioToLoad = string.Format(path + "{0}", filename);
        WWW request = new WWW(audioToLoad);
        return request;
    }

    int[] tableBGMFadeout = {385,585,680,860,950,-1,-1};
    int[] tableBGMChange  = {400,600,700,900,1000,2100,-1};
    void FadeoutBGM()
    {
	    if (tableBGMFadeout[bgmlv-1] != -1 && level >= tableBGMFadeout[bgmlv-1])
        {
            gameAudio.volume -= (Time.deltaTime / 3);
        }
    }
    void ChangeBGM()
    {
        if (tableBGMChange[bgmlv-1] != -1 && level >= tableBGMChange[bgmlv-1]) { bgmlv += 1; gameAudio.volume = 1; gameAudio.Stop(); gameAudio.clip = bgm_1p_lv[bgmlv-1]; if(bgmlv < 7)gameAudio.Play();}
    }
    public void OnMovement(InputAction.CallbackContext value)
    {
        movement = value.ReadValue<Vector2>();
        if (value.ReadValue<Vector2>().y > 0.5) {Inputs[0] = true; HoldInputs[0] = true;}
        else {Inputs[0] = false; HoldInputs[0] = false;}
    }
    public void OnCounterclockwise(InputAction.CallbackContext value)
    {
        if (value.performed) {HoldInputs[1] = true; Inputs[1] = true;}
        else {HoldInputs[1] = false; Inputs[1] = false;}
    }
    public void OnClockwise(InputAction.CallbackContext value)
    {
        if (value.performed) {HoldInputs[2] = true; Inputs[2] = true;}
        else {HoldInputs[2] = false; Inputs[2] = false;}
    }
    public void OnClockwise2(InputAction.CallbackContext value)
    {
        if (value.performed) {HoldInputs[6] = true; Inputs[6] = true;}
        else {HoldInputs[6] = false; Inputs[6] = false;}
    }
    public void OnUpsideDown(InputAction.CallbackContext value)
    {
        if (value.performed) {HoldInputs[3] = true; Inputs[3] = true;}
        else {HoldInputs[3] = false; Inputs[3] = false;}
    }
    public void OnHold(InputAction.CallbackContext value)
    {
        if (value.performed) {HoldInputs[4] = true; Inputs[4] = true;}
        else {HoldInputs[4] = false; Inputs[4] = false;}
    }
    public void OnPause(InputAction.CallbackContext value)
    {
        if (value.started && MenuEngine.instance.curBoard != null)
        {
            paused = !paused;
        }
    }
    public void OnFramestep(InputAction.CallbackContext value)
    {
        if (value.started)
        {
            Inputs[7] = true;
        }
        if (value.performed) HoldInputs[7] = true;
        else HoldInputs[7] = false;
    }
    public string timeCount(int time)
    {
        return Math.Floor(((double)time/60000)%6).ToString() + Math.Floor(((double)time/6000)%10) + ":" + Math.Floor(((double)time%6000/1000)%6) + Math.Floor(((double)time%6000/100)%10) + ":" + Math.Floor((((double)time%100/1000)*100)%10) + Math.Floor((((double)time%100/100)*100)%10);
    }
    public void SpawnFireworks()
    {
        BoardParticleSystem.instance.SummonFirework(new Vector2(0f, 10f), new Vector2(10f,10f));
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        
        if(level > endingLevel) level = endingLevel;
        if(time == 1 || (AREf == -1 && ending)) gameAudio.Play();
        if (ending) MenuEngine.instance.supposedToBeAPartOfBoard[6].SetActive(true);
        else MenuEngine.instance.supposedToBeAPartOfBoard[6].SetActive(false);
        // musicTime += Time.deltaTime;
        FadeoutBGM();
        ChangeBGM();
        if(notifDelay > 0)notifDelay--;
        if(MenuEngine.instance.curBoard != null){int pieceCountHoldRed = PiecesController.instance.pieceHold == 28 ? -1 : -2;
        if(time > 0)ppsCounter.text = Math.Floor(((double)(PiecesController.instance.pieces + pieceCountHoldRed) / ((double)time/100))*100)/100 + " pieces/second";}
        // if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Space)) Inputs[2] = true;
        // if (Input.GetKey(KeyCode.A)) Inputs[3] = true;
        // if (Input.GetKey(KeyCode.C)) Inputs[4] = true;
        // if (Input.GetKeyDown(KeyCode.P) && MenuEngine.instance.curBoard != null) paused = !paused;
        if (Input.GetKeyDown(KeyCode.F)) SpawnFireworks();
        if (Input.GetKeyDown(KeyCode.D)) level = curSect*100+99;
        if (Input.GetKeyDown(KeyCode.H)) {gradePoints += gradePointRequirement; statGradePoints += gradePointRequirement;}
        if((paused == false || (FrameStep == true && Inputs[7])) && MenuEngine.instance.GameOver == false)
        {
            if (level >= endingLevel && AREf < (int)ARE)
            {
                int whichline = ((AREf - (int)ARE)+240)/6;
                Debug.Log(whichline);
                BoardController.instance.DestroyLine(whichline);
            }
            if (AREf == (int)ARE - 399) gameAudio.PlayOneShot(excellent);
            if(AREf >= 0 && readyGoIndicator.sprite == null && rollTime < rollTimeLimit)time++;
            if(AREf >= 0 && readyGoIndicator.sprite == null && ending && rollTime < rollTimeLimit)rollTime++;
            int nextsecint = level < endingLevel ? (curSect + 1) * 100 : endingLevel;
            levelTextRender.text = level.ToString();
            if(curSect < 21)nextSecLv.text = nextsecint.ToString();
            if(!ending)timeCounter.text = timeCount(time);
            rollTimeCounter.text = timeCount(rollTimeLimit - rollTime);
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
        if(AREf == (int)ARE - 200 && level < 100) {gameAudio.PlayOneShot(readySE); readyGoIndicator.sprite = readySprite;}
        if(AREf == (int)ARE - 100 && level < 100) {gameAudio.PlayOneShot(goSE); readyGoIndicator.sprite = goSprite;}
        if(AREf == (int)ARE - 1 && level < 100) readyGoIndicator.sprite = null;
        if (sectAfter20g < 1) DAS = 25;
        else if (sectAfter20g < 5) DAS = 15;
        else if (sectAfter20g < 9) DAS = 10;
        else if (sectAfter20g < 13) DAS = 3;
        else DAS = 1;
    }
    public void DisconnectGameOver()
    {
        if (MenuEngine.instance.curBoard != null) MenuEngine.instance.GameOver = true;
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
            NotificationEngine.instance.InstantiateNotification(""+ Math.Floor(gradePoints), Color.white);
            NotificationEngine.instance.InstantiateNotification("/" + Math.Floor(gradePointRequirement), Color.white);
            notifDelay = 200;
        }
    }
}
