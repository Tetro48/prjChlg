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
    public TextMeshPro levelTextRender, nextSecLv, timeCounter, rollTimeCounter;
    public static GameEngine instance;
    public SpriteRenderer readyGoIndicator, gradeIndicator;
    public Sprite[] gradeSprites;
    public int grade;
    public Sprite readySprite, goSprite;
    public int nextPieces, nextibmblocks;
    public RotationSystems RS;
    public bool ending;
    public double LockDelay = 30;    
    public double DAS = 15;
    public double SDF = 6;
    public double ARE = 25;
    public int AREf = 25 - 180;
    public double AREline = 10;
    public int lineDelayf = 0;
    public double lineDelay = 15;
    public float gravity = 5/64f;
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
	static int[] tableGradePoint = {10, 30, 60, 120, 180, 240, 300, 400};

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
	};
    static int[] lvlLineIncrement = {1, 3, 6, 10, 15, 21, 28, 36};
    public void LineClears(int lines)
    {
        if (lines > 0)
        {
            comboKeepCounter = 2;
            if (lines > 1)
            {
                comboCount++;
            }
            if(comboCount >= 2) {
                int cmbse = comboCount - 2;
                if(cmbse > 20) cmbse = 20;
                gameAudio.PlayOneShot(comboSE[cmbse]);
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
                AREf = (int)ARE - 240;
                ending = true;
            }
            PiecesController.instance.gameAudio.PlayOneShot(PiecesController.instance.levelup);
            if (curSect < 20)
            {
                BackgroundController.bginstance.TriggerBackgroundChange(curSect);
            }
            if(curSect % 5 == 0) NotificationEngine.instance.InstantiateNotification("500 level part complete!",Color.white);
            if (gravity >= 19.99999)
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
            }
            else
            {
                gravity = gravity * 4;
            }
        }
		int basepoint = tableGradePoint[lines - 1];
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
        return Math.Floor(((double)time/36000)%6).ToString() + Math.Floor(((double)time/3600)%10) + ":" + Math.Floor(((double)time%3600/600)%6) + Math.Floor(((double)time%3600/60)%10) + ":" + Math.Floor((((double)time%60/600)*100)%10) + Math.Floor((((double)time%60/60)*100)%10);
    }
    // Update is called once per frame
    void Update()
    {
        
        if(level > endingLevel) level = endingLevel;
        if(time == 1 || (AREf == -1 && ending)) gameAudio.Play();
        if (ending) MenuEngine.instance.supposedToBeAPartOfBoard[5].SetActive(true);
        else MenuEngine.instance.supposedToBeAPartOfBoard[5].SetActive(false);
        // musicTime += Time.deltaTime;
        FadeoutBGM();
        ChangeBGM();
        if(notifDelay > 0)notifDelay--;
        // if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Space)) Inputs[2] = true;
        // if (Input.GetKey(KeyCode.A)) Inputs[3] = true;
        // if (Input.GetKey(KeyCode.C)) Inputs[4] = true;
        // if (Input.GetKeyDown(KeyCode.P) && MenuEngine.instance.curBoard != null) paused = !paused;
        if (Input.GetKeyDown(KeyCode.D)) level = curSect*100+99;
        if((paused == false || (FrameStep == true && Inputs[7])) && MenuEngine.instance.GameOver == false)
        {
            if (level >= endingLevel && AREf < (int)ARE)
            {
                int whichline = (AREf+240)/6;
                Debug.Log(whichline);
                BoardController.instance.DestroyLine(whichline);
            }
            if (AREf == (int)ARE - 239) gameAudio.PlayOneShot(excellent);
            if(AREf >= 0 && readyGoIndicator.sprite == null)time++;
            if(AREf >= 0 && readyGoIndicator.sprite == null && ending)rollTime++;
            int nextsecint = level < endingLevel ? (curSect + 1) * 100 : endingLevel;
            levelTextRender.text = level.ToString();
            if(curSect < 21)nextSecLv.text = nextsecint.ToString();
            timeCounter.text = timeCount(time);
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
        if(AREf == ARE - 120 && level < 100) {gameAudio.PlayOneShot(readySE); readyGoIndicator.sprite = readySprite;}
        if(AREf == ARE - 60 && level < 100) {gameAudio.PlayOneShot(goSE); readyGoIndicator.sprite = goSprite;}
        if(AREf == ARE - 1 && level < 100) readyGoIndicator.sprite = null;
        if (curSect < 5) DAS = 15;
        else if (curSect < 9) DAS = 9;
        else if (curSect < 13) DAS = 6;
        else if (curSect < 17) DAS = 2;
        else DAS = 1;
    }
    public void DisconnectGameOver()
    {
        if (MenuEngine.instance.curBoard != null) MenuEngine.instance.GameOver = true;
    }
    public void ControllerSwap() {if (MenuEngine.instance.curBoard != null && notifDelay == 0) {NotificationEngine.instance.InstantiateNotification("Controller is swapped", Color.white); notifDelay = 300;} }
    public void ShowGradeScore() {if (notifDelay == 0) {NotificationEngine.instance.InstantiateNotification("Grade score: ", Color.white); NotificationEngine.instance.InstantiateNotification(""+ Math.Floor(gradePoints), Color.white); NotificationEngine.instance.InstantiateNotification("/" + Math.Floor(gradePointRequirement), Color.white); notifDelay = 120;} }
}
