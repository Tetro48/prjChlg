using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using TMPro;

public enum RotationSystems {SRS, ARS}
public class GameEngine : MonoBehaviour
{
    public int time;
    public int level;
    public int curSect, sectAfter20g;
    private double musicTime;
    public AudioSource gameAudio;
    public AudioClip[] bgm_1p_lv;
    public string audioPath;
    public AudioClip readySE, goSE;
    public int bgmlv = 1;
    public TextMeshPro levelTextRender, nextSecLv, timeCounter;
    public static GameEngine instance;
    public SpriteRenderer readyGoIndicator;
    public Sprite readySprite, goSprite;
    public int nextPieces, nextibmblocks;
    public RotationSystems RS;
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

    int[] lvlLineIncrement = {1, 3, 6, 10, 15, 21, 28, 36};
    public void LineClears(int lines)
    {
        level += lvlLineIncrement[lines-1];
        if(level > 2100) level = 2100;
        if(level/100 > curSect)
        {
            curSect++;
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
            StartCoroutine(LoadMusic(i));
        }
    }
    private IEnumerator LoadMusic(int lv)
    {
        WWW request;
        if(lv < 6) request = GetAudioFromFile(audioPath, "BGM_1p_lv"+(lv+1)+".wav");
        else request = GetAudioFromFile(audioPath, "BGM_ending2.wav");
        Debug.Log(request);
        yield return request;

        bgm_1p_lv[lv] = request.GetAudioClip();
        bgm_1p_lv[lv].name = "BGM_" + (lv < 6 ? "1p_lv"+(lv+1)+".wav" : "ending2.wav");
        if (lv == 0) gameAudio.clip = bgm_1p_lv[0];
    }
    private WWW GetAudioFromFile(string path, string filename)
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
        if (tableBGMChange[bgmlv-1] != -1 && level >= tableBGMChange[bgmlv-1]) { bgmlv += 1; gameAudio.volume = 1; gameAudio.Stop(); gameAudio.clip = bgm_1p_lv[bgmlv-1]; gameAudio.Play();}
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
    // Update is called once per frame
    void Update()
    {
        if(level > 2100) level = 2100;
        if(time == 1) gameAudio.Play();
        // musicTime += Time.deltaTime;
        FadeoutBGM();
        ChangeBGM();
        // if (Input.GetKey(KeyCode.X) || Input.GetKey(KeyCode.Space)) Inputs[2] = true;
        // if (Input.GetKey(KeyCode.A)) Inputs[3] = true;
        // if (Input.GetKey(KeyCode.C)) Inputs[4] = true;
        // if (Input.GetKeyDown(KeyCode.P) && MenuEngine.instance.curBoard != null) paused = !paused;
        if (Input.GetKeyDown(KeyCode.D)) level = curSect*100+99;
        if((paused == false || (FrameStep == true && Inputs[7])) && MenuEngine.instance.GameOver == false)
        {
            if(AREf >= 0 && readyGoIndicator.sprite == null)time++;
            int nextsecint = level < 2100 ? (curSect + 1) * 100 : 2100;
            levelTextRender.text = level.ToString();
            if(curSect < 21)nextSecLv.text = nextsecint.ToString();
            timeCounter.text = Math.Floor(((double)time/36000)%6).ToString() + Math.Floor(((double)time/3600)%10) + ":" + Math.Floor(((double)time%3600/600)%6) + Math.Floor(((double)time%3600/60)%10) + ":" + Math.Floor((((double)time%60/600)*100)%10) + Math.Floor((((double)time%60/60)*100)%10);
            framestepped = true;
            for (int i = 0; i < 7; i++)
            {
                // if(Inputs[i]>0)Inputs[i]--;
                // if(FrameHoldInputs[i]>0)FrameHoldInputs[i]--;
                // HoldInputs[i] = FrameHoldInputs[i] > 0;
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
    public void ControllerSwap() {if (MenuEngine.instance.curBoard != null) NotificationEngine.instance.InstantiateNotification("Controller is swapped", Color.white);}
}
