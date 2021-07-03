using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Discord;
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

public enum Language {English, Русский, 日本語};

[RequireComponent(typeof(AudioSource))]
public class MenuEngine : MonoBehaviour
{
    public Language language;
    public Discord.Discord discord;
    public static MenuEngine instance;
    public int frames = 0;
    public int menu = 0;
    public bool quitting = false;
    public bool startGame = false;
    public bool GameOver = false;
    public bool IntentionalGameOver = false;
    public bool starting;
    public bool pressedSettingsMenu, pressedInputsMenu;
    public bool pressedBack;

    public string[] gradeStringConversion = {"9","8","7","6","5","4","3","2","1","S1","S2","S3","S4","S5","S6","S7","S8","S9","GM"};

    public TextMeshProUGUI framerateCounter;

    public TMP_Dropdown resDropdown;

    public AudioSource audioSource, audioSource2, mainMenuMusic, audioSourceConfirmation;
    public AudioClip clip, topoutSE;
    public GameObject inGameBoard, curBoard;
    public AudioClip ModeOK;
    public GameObject mainMenu, settingsMenu, inputsMenu, rotationSystemsMenu, customModeSettingsMenu, preferencesMenu, tuningMenu, imgprjchlg, mobileInput;
    public GameObject[] supposedToBeAPartOfBoard, mainMenuGUI, settingsMenuGUI, settingsMenuGUIpart, inputsMenuGUI, rotationSystemsMenuGUI, customModeSettingsMenuGUI, preferencesMenuGUI, tuningMenuGUI;
    public RectTransform mainMenuMovement, settingsMovement,settingsGUI1PartMovement, inputsMovement, rotationSystemsMovement, customModeSettingsMovement, preferencesMovement, tuningMovement;
    public RectTransform[] mainMenuGUIMovement, settingsGUIMovement, settingsGUIPartMovement, inputsGUIMovement, rotationSystemsGUIMovement, customModeSettingsGUIMovement, preferencesGUIMovement, tuningGUIMovement;
    public TextMeshProUGUI[] mainMenuGUIText, settingsGUIText, inputsGUIText;
    Vector3 boardpos, boardrot, posMM, posS, posSGUI1P, posI, posRS, posCMS, posP, posT;
    public Vector3[] posMMGUI, posSGUI, posSGUIP, posIGUI, posRSGUI, posCMSGUI, posPGUI, posTGUI;
    private float fallingdegree;
    Resolution[] resolutions;
    public float reswidth;

    public String[,] mainMenuLangString = 
    {
        {"Play!", "Settings", "Quit"},
        {"Играть!", "Настройки", "Выйти"},
        {"Purei!", "Settingu", "Shu-ryo- suru"},

    }, settingsLangString = 
    {
        {"Resolution:", "Inputs", "Rotation Systems", "Custom Mode settings", "Preferences settings", "Tuning", "< Back"},
        {"Разрешение:", "Вводы", "Системы вращения", "Настройки режима", "Настройки предпотчении", "Тьюнинг", "< Назад"},
        {"Kaizo-do", "Nyu-ryoku", "Kaiten shisutemu", "Kasutmumo-do no settei", "Purifarensu settei", "Chu-ningu", "< Bakku"},

    }, inputsLangString = 
    {
        {"", "", "", "", "", "", ""},
        {"", "", "", "", "", "", ""},
        {"", "", "", "", "", "", ""},

    }, notifLangString =
    {
        //
        {"Singles: ", "Doubles: ", "Triples: ", "Tetrises: ", "lines: ", "Total ", "Pieces: ", "Grade: ", "Total grade score:", "Level: ", "Gravity: ", "Time: ", "500 level part complete!", "Controller is swapped", "Grade score: ", "Starting up!"},
        {"Одиночные: ", "Двойные: ", "Тройные: ", "Тетрисы: ", "линии: ", "Всего ", "Фигур: ", "Оценка: ", "Общий счет оценки:", "Уровень: ", "Гравитация: ", "Время: ", "Достигнуто часть 500 уровней!", "Контроллер заменен", "Счет оценки: ", "Начинаем!"},
        {"", "", "", "", "", "", "", "", "", "", "", "", "", "", "", ""},
    };
    Language previousLang;
    public void QuitGame()
    {
        if (platformCompat() || Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) quitting = true;
    }
    public void TriggerGameOver()
    {
        GameOver = true;
        starting = false;
    }
    public void PlayGame()
    {
        if(!starting && !startGame) {startGame = true;  NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 15], Color.white);}
    }
    public void Settings() { if (!starting && !pressedSettingsMenu) {pressedSettingsMenu = true; menu++;} }
    
    public void Inputs() { if (!starting && !pressedInputsMenu && !pressedSettingsMenu && !pressedBack) {pressedInputsMenu = true; menu++;} }
    public void Back()
    {
        if (!pressedBack && !pressedSettingsMenu && !pressedInputsMenu)
        {
            pressedBack = true;
            menu--;
            if (menu > 2)
            {
                menu -= (menu - 2);
            }
        }
    }
    bool platformLogged = false;
    public bool platformCompat()
    {
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            if(!platformLogged) Debug.Log("Windows platform");
            platformLogged = true;
            return true;
        }
        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
        {
            if(!platformLogged) Debug.Log("macOS platform");
            platformLogged = true;
            return true;
        }
        if (Application.platform == RuntimePlatform.LinuxPlayer || Application.platform == RuntimePlatform.LinuxEditor)
        {
            if(!platformLogged) Debug.Log("Linux platform");
            platformLogged = true;
            return true;
        }
        if(!platformLogged) Debug.Log("Other platform");
        if(!platformLogged) Debug.Log("Some functions are disabled for compatibility.");
        platformLogged = true;
        return false;
    }
    public bool alreadystarted;
    void Awake()
    {
        if (platformCompat()) discord = new Discord.Discord(836600860976349192, (UInt64)Discord.CreateFlags.Default);
        Application.targetFrameRate = Screen.currentResolution.refreshRate * 4;
        alreadystarted = true;
        instance = this;
    }
    void indicatorActivity(bool set, int upTo)
    {
        Debug.Log(supposedToBeAPartOfBoard.Length);
        int length = supposedToBeAPartOfBoard.Length < upTo ? supposedToBeAPartOfBoard.Length : upTo;
        for (int i = 0; i < length; i++)
        {
            Debug.Log("ind:"+i);
            supposedToBeAPartOfBoard[i].SetActive(set);
        }
    }
    public bool drpcSwitch;
    private int resRefreshrates = 0;
    void Start()
    {
        if(!platformCompat()) {reswidth = 1f; settingsMovement.position += new Vector3(0f,47.77f * (Screen.height / 1080.0f),0f);}
        GameObject[] objs = GameObject.FindGameObjectsWithTag("menuenginerelated");
        GameObject[] canvasobjs = GameObject.FindGameObjectsWithTag("Canvas");
        GameObject[] gameoverseobjs = GameObject.FindGameObjectsWithTag("GameOverSE");
        GameObject[] buttonseobjs = GameObject.FindGameObjectsWithTag("ButtonSE");

        if(canvasobjs.Length > 1)
        {
            for (int i = 0; i < canvasobjs.Length; i++)
            {
                if (i % 2 == 1) Destroy(canvasobjs[i/2+1]);
            }
        }
        if(gameoverseobjs.Length > 1)
        {
            for (int i = 0; i < gameoverseobjs.Length; i++)
            {
                if (i % 2 == 1) Destroy(gameoverseobjs[i/2+1]);
            }
        }
        if(buttonseobjs.Length > 1)
        {
            for (int i = 0; i < buttonseobjs.Length; i++)
            {
                if (i % 2 == 1) Destroy(buttonseobjs[i/2+1]);
            }
        }
        if (objs.Length > 1)
        {
            for (int i = 0; i < objs.Length; i++)
            {
                if (i % 2 == 1) Destroy(objs[i/2+1]);
            }
        }

        DontDestroyOnLoad(objs[0]);
        DontDestroyOnLoad(canvasobjs[0]);
        DontDestroyOnLoad(gameoverseobjs[0]);
        DontDestroyOnLoad(buttonseobjs[0]);
        // imgbg.SetActive(true);
        imgprjchlg.SetActive(true);
        if (platformCompat())
        {
            resolutions = Screen.resolutions;
            resDropdown.ClearOptions();
            List<string> options = new List<string>();

            int currentResolutionIndex = 1;
            bool matchedrefrrate = true;
            for (int j = 0; j < resolutions.Length && matchedrefrrate == true; j++)
            {
                if (j > 1)
                {
                    if (resolutions[1].refreshRate == resolutions[j].refreshRate)
                    {
                        matchedrefrrate = false;
                    }
                    resRefreshrates++;
                }
            }
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + "x" + resolutions[i].height;
                if (resolutions[i].refreshRate == 60) options.Add(option);
                if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height && resolutions[i].refreshRate == 60)
                {
                    currentResolutionIndex = i/resRefreshrates;
                    reswidth = (float)(resolutions[i/resRefreshrates].width / 1920);
                }
            }
            resDropdown.AddOptions(options);
            resDropdown.value = currentResolutionIndex;
            resDropdown.RefreshShownValue();
            mobileInput.SetActive(false);
        }
        else
        {
            reswidth = (float)(Screen.width / 1920.0);
            settingsMenuGUI[0].SetActive(false);
            settingsMenuGUIpart[0].SetActive(false);
            if(Application.platform != RuntimePlatform.Android && Application.platform != RuntimePlatform.IPhonePlayer)mainMenuGUI[2].SetActive(false);
        }
        starting = true;
        if (Application.systemLanguage == SystemLanguage.Russian)
        {
            language = Language.Русский;
        }
        if (Application.systemLanguage == SystemLanguage.Japanese)
        {
            language = Language.日本語;
        }
        posMM = mainMenuMovement.position;
        posMM.x -= (float)(500.0 * reswidth);
        mainMenuMovement.position = posMM;
        posS = settingsMovement.position;
        posS.x -= (float)(500.0 * reswidth);
        settingsMovement.position = posS;
        posSGUI1P = settingsGUI1PartMovement.position;
        posSGUI1P.x -= (float)(500.0 * reswidth);
        settingsGUI1PartMovement.position = posSGUI1P;
        posI = inputsMovement.position;
        posI.x -= (float)(500.0 * reswidth);
        inputsMovement.position = posI;
    }
    public void UpdateLang()
    {
        for (int mmguiIndex = 0; mmguiIndex < mainMenuGUI.Length; mmguiIndex++)
        {
            mainMenuGUIText[mmguiIndex].text = mainMenuLangString[(int)language, mmguiIndex];
        }
        for (int sguiIndex = 0; sguiIndex < settingsMenuGUI.Length; sguiIndex++)
        {
            settingsGUIText[sguiIndex].text = settingsLangString[(int)language, sguiIndex];
        }
    }
    public void SetResolution (int index)
    {
        if (platformCompat())
        {
            Resolution resolution = resolutions[(resRefreshrates-1)+index*resRefreshrates];
            Screen.SetResolution(resolution.width, resolution.height, true);
            reswidth = (float)(resolution.width / 1920.0);
        }
    }
    // void OnDestroy()
    // {
    //     discord.Dispose();
    // }
    void OnApplicationQuit()
    {
        if (platformCompat())discord.Dispose();
    }
    double framerate;
    [SerializeField] List<double> frameratebuffer;
    // rough framerate measurement
    void Update()
    {
        double rawframetime = Time.deltaTime;
        frameratebuffer.Add(rawframetime);
        if (frameratebuffer.Count > 100)
        {
            frameratebuffer.RemoveAt(0);
        }
        double result = 0;
        for (int fr = 0; fr < frameratebuffer.Count; fr++)
        {
            result += frameratebuffer[fr];
        }
        framerate = 1.0 / (result/frameratebuffer.Count);
    }
    void FixedUpdate()
    {
        if (previousLang != language)
        {
            previousLang = language;
            UpdateLang();
        }
        if (platformCompat())
        {
            var activityManager = discord.GetActivityManager();
            int rpclvl = GameEngine.instance.level < 2100 ? (GameEngine.instance.curSect + 1) * 100 : 2100;
            var activity = new Discord.Activity {
                Details = curBoard != null ? "Level " + GameEngine.instance.level + " | " + rpclvl + (GameEngine.instance.level > 800 ? ". Struggling." : string.Empty) : null,
                State = !Application.genuine ? "The game is tampered" : framerate > 2600 ? "Suspiciously high framerate" : framerate < 10 ? "Suspiciously low framerate" : IntentionalGameOver ? "Exiting..." : GameOver ? "Topped out" : curBoard != null && GameEngine.instance.paused && !GameEngine.instance.FrameStep ? "Paused" : curBoard != null && GameEngine.instance.replay.mode ? "Currently replaying" : curBoard != null && GameEngine.instance.paused && GameEngine.instance.FrameStep ? "Currently playing (Framestepping)" : curBoard != null ? "Currently playing" : quitting ? "Quitting" : menu == 1 ? "Currently in settings menu" :"Currently in main menu",
                Assets = {
                    LargeImage = "icon"
                }
            };
            activityManager.UpdateActivity(activity, (res) => {
            });
            if(drpcSwitch)discord.RunCallbacks();
        }
        var MMf = mainMenuGUI.Length * 8;
        var SBf = settingsMenuGUI.Length * 8;
        var IPf = inputsMenuGUI.Length * 8;
        var SBVf = settingsGUIMovement.Length * 8;
        var MMSBf = (MMf + SBf);
        var SBIPf = (SBf + IPf);
        var MMSf = (MMf + 50);
        var MMSCf = (MMSf + SBf);
        var MMSCVf = (MMSf + SBVf);
        if(curBoard == null && Input.GetKeyDown(KeyCode.Escape))
        {
            if (menu > 0)
            {
                Back();
                audioSourceConfirmation.Play();
            }
            else
            {
                QuitGame();
                audioSourceConfirmation.Play();
            }
        }
        reswidth = (float)(Screen.width / 1920.0);
        framerateCounter.text = (Math.Floor((framerate)*100)/100) + " FPS";
        if (GameOver)
        {
            GameEngine.instance.readyGoIndicator.sprite = null;
            GameEngine.instance.gameMusic.Stop();
            if(frames%10==9 && frames<400)BoardController.instance.DestroyLine(frames/10);
            if(frames<400)BoardController.instance.DecayLine(frames/10, 0.1f);
            frames++;
            if(frames > 300)
            {
                boardpos = this.transform.position;
                boardpos.y -= (float)(0.1f + (((frames-300) / 33) * ((frames-300) / 33)));
                this.transform.position = boardpos;
                // boardrot = curBoard.transform.Rotate;
                // boardrot.z -= 0.16f;
                curBoard.transform.Rotate(new Vector3(0f, 0f, -0.1f - (float)(((frames-300) / 66) * ((frames-400) / 66))));
                if (frames == 301)
                {
                    audioSource2.PlayOneShot(topoutSE);
                }
                if (frames == 351)
                {
                    audioSource2.Play();
                }
                if (frames == 400)
                {
                    // SceneManager.LoadScene("MenuScene");
                }
                if (frames == 401)
                {
                    int pieceCountHoldRed = PiecesController.instance.pieceHold == 28 ? 0 : -1;
                    if(GameEngine.instance.singles > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 0] + GameEngine.instance.singles, Color.white); // Singles
                    if(GameEngine.instance.doubles > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 1] + GameEngine.instance.doubles, Color.white); // Doubles
                    if(GameEngine.instance.triples > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 2] + GameEngine.instance.triples, Color.white); // Triples
                    if(GameEngine.instance.tetrises > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 3] + GameEngine.instance.tetrises, Color.white); // Tetrises
                    if(GameEngine.instance.pentrises > 0) NotificationEngine.instance.InstantiateNotification("5 " + notifLangString[(int)language, 4] + GameEngine.instance.pentrises, Color.white); // 5 lines
                    if(GameEngine.instance.sixtrises > 0) NotificationEngine.instance.InstantiateNotification("6 " + notifLangString[(int)language, 4] + GameEngine.instance.sixtrises, Color.white); // 6 lines
                    if(GameEngine.instance.septrises > 0) NotificationEngine.instance.InstantiateNotification("7 " + notifLangString[(int)language, 4] + GameEngine.instance.septrises, Color.white); // 7 lines
                    if(GameEngine.instance.octrises > 0) NotificationEngine.instance.InstantiateNotification("8+ " + notifLangString[(int)language, 4] + GameEngine.instance.octrises, Color.white); // 8+ lines
                    if(GameEngine.instance.totalLines > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 5] + notifLangString[(int)language, 4] + GameEngine.instance.totalLines, Color.white); // Total lines
                    if(PiecesController.instance.pieces > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 6] + (PiecesController.instance.pieces + pieceCountHoldRed), Color.white); // Pieces
                    NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 7] + gradeStringConversion[GameEngine.instance.grade], Color.white); // Grade
                    if(GameEngine.instance.statGradePoints > 0) {NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 8], Color.white); // Total grade score
                    NotificationEngine.instance.InstantiateNotification(Math.Floor(GameEngine.instance.statGradePoints).ToString(), Color.white);}
                    NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 9] + GameEngine.instance.level + "/" + (GameEngine.instance.level < 2100 ? (GameEngine.instance.curSect + 1) * 100 : 2100), Color.white); // Level
                    NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 10] + GameEngine.instance.gravity, Color.white); // Gravity
                    NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 11] + GameEngine.instance.timeCounter.text, Color.white);
                    frames = 0;
                    GameOver = false;
                    IntentionalGameOver = false;
                    starting = true;
                    this.transform.position = Vector3.zero;
                    BackgroundController.bginstance.TriggerBackgroundChange(0);
                    if (!GameEngine.instance.replay.mode)
                    {
                        GameEngine.instance.replay.SaveReplay("1");
                    }
                }
            }
        }
        if (quitting || pressedSettingsMenu || startGame)
        {
            frames++;
            if (frames == 1)
            {
                mainMenu.SetActive(true);
                audioSource.PlayOneShot(clip);
            }
            else if(frames % 8 == 7 && frames < MMf -1)
            {
                if(mainMenuGUI[(frames-1)/8].activeSelf)audioSource.PlayOneShot(clip);
            }
            if(frames < MMf + 1)
            {
                posMMGUI[(frames-1)/8] = mainMenuGUIMovement[(frames-1)/8].position;
                posMMGUI[(frames-1)/8].x -= (float)(62.5 * reswidth);
                mainMenuGUIMovement[(frames-1)/8].position = posMMGUI[(frames-1)/8];
            }
            if (startGame && frames > MMf + 10)
            {
                if (frames == MMf + 11)
                {
                    GameEngine.instance.replay.Reset();
                    if (GameEngine.instance.replay.mode)
                    {
                        GameEngine.instance.replay.LoadReplay("1");
                    }
                    mainMenu.SetActive(false);
                    GameEngine.instance.gradePoints = 0;
                    GameEngine.instance.gradePointRequirement = 100;
                    GameEngine.instance.statGradePoints = 0;
                    // SceneManager.LoadScene("GameScene");
                    GameObject board = GameObject.Instantiate(inGameBoard, transform);
                    curBoard = board;
                    this.transform.position = new Vector3(0.0f, 20f, 0.0f);
                    indicatorActivity(true, 6);
                    mainMenuMusic.Stop();
                    PiecesController.instance.UpdateShownPieces();
                }
                if (frames == MMf + 12)   this.transform.position = new Vector3(0.0f, 18f, 0.0f);
                if (frames == MMf + 13)   this.transform.position = new Vector3(0.0f, 16f, 0.0f);
                if (frames == MMf + 14)   this.transform.position = new Vector3(0.0f, 14f, 0.0f);
                if (frames == MMf + 15)   this.transform.position = new Vector3(0.0f, 12f, 0.0f);
                if (frames == MMf + 16)   this.transform.position = new Vector3(0.0f, 10f, 0.0f);
                if (frames == MMf + 17)   this.transform.position = new Vector3(0.0f, 8f, 0.0f);
                if (frames == MMf + 18)   this.transform.position = new Vector3(0.0f, 6f, 0.0f);
                if (frames == MMf + 19)   this.transform.position = new Vector3(0.0f, 4f, 0.0f);
                if (frames == MMf + 20)   this.transform.position = new Vector3(0.0f, 2f, 0.0f);
                if (frames == MMf + 21)   this.transform.position = new Vector3(0.0f, 0f, 0.0f);
                if (frames == MMf + 22)   this.transform.position = new Vector3(0.0f, -0.8f, 0.0f);
                if (frames == MMf + 23)   this.transform.position = new Vector3(0.0f, -1.4f, 0.0f);
                if (frames == MMf + 24)   this.transform.position = new Vector3(0.0f, -2f, 0.0f);
                if (frames == MMf + 25)   this.transform.position = new Vector3(0.0f, -1.3f, 0.0f);
                if (frames == MMf + 26)   this.transform.position = new Vector3(0.0f, -0.7f, 0.0f);
                if (frames == MMf + 27)   this.transform.position = new Vector3(0.0f, 0f, 0.0f);
                // if (frames == 23)
                // {
                //     SceneManager.UnloadSceneAsync("MenuScene");
                // }
                if (frames > MMf + 27)
                {
                    // PiecesController.instance.bagPieceResult=Random.Range(0,7);
                    startGame = false;
                    frames = 0;
                    // imgbg.SetActive(false);
                    // imgprjchlg.SetActive(false);
                    GameEngine.instance.paused = false;
                    PiecesController.instance.UpdatePieceBag();
                }
            }
            if (pressedSettingsMenu && frames > MMSf && menu == 1)
            {
                if(frames % 8 == 0)
                {
                    mainMenu.SetActive(false);
                    if(settingsMenuGUI[((frames-1) - MMSf)/8].activeSelf)audioSource.PlayOneShot(clip);
                }
                if (frames == MMSf + 1)
                {
                    settingsMenu.SetActive(true);
                }
                if (frames < MMSCf + 1)
                {
                    posSGUI[((frames-1) - MMSf)/8] = settingsGUIMovement[((frames-1) - MMSf)/8].position;
                    posSGUI[((frames-1) - MMSf)/8].x += (float)(62.5 * reswidth);
                    settingsGUIMovement[((frames-1) - MMSf)/8].position = posSGUI[((frames-1) - MMSf)/8];
                    if((frames-1) < MMSf + settingsGUIPartMovement.Length * 8)
                    {
                        posSGUIP[((frames-1) - MMSf)/8] = settingsGUIPartMovement[((frames-1) - MMSf)/8].position;
                        posSGUIP[((frames-1) - MMSf)/8].x += (float)(125.0 * reswidth);
                        settingsGUIPartMovement[((frames-1) - MMSf)/8].position = posSGUIP[((frames-1) - MMSf)/8];
                    }
                }
                else
                {
                    pressedSettingsMenu = false;
                    mainMenu.SetActive(false);
                    frames = 0;
                }
            }
            if (frames > MMf + 65 && quitting)
            {
                discord.Dispose();
                Application.Quit();
            }
        }
        if (pressedBack)
        {
            frames++;
            if (menu == 0)
            {
                if (frames == 1)
                {
                    mainMenu.SetActive(true);
                    audioSource.PlayOneShot(clip);
                }
                else if(frames % 8 == 0)
                {
                    audioSource.PlayOneShot(clip);
                }
                if (frames < SBf + 1)
                {
                    posSGUI[(frames-1)/8] = settingsGUIMovement[(frames-1)/8].position;
                    posSGUI[(frames-1)/8].x -= (float)(62.5 * reswidth);
                    settingsGUIMovement[(frames-1)/8].position = posSGUI[(frames-1)/8];
                    if(frames < settingsMenuGUIpart.Length * 8 +1)
                    {
                        posSGUIP[(frames-1)/8] = settingsGUIPartMovement[(frames-1)/8].position;
                        posSGUIP[(frames-1)/8].x -= (float)(125.0 * reswidth);
                        settingsGUIPartMovement[(frames-1)/8].position = posSGUIP[(frames-1)/8];
                    }
                }
                else if(frames < MMSBf + 1)
                {
                    posMMGUI[((frames-1) - SBf)/8] = mainMenuGUIMovement[((frames-1) - SBf)/8].position;
                    posMMGUI[((frames-1) - SBf)/8].x += (float)(62.5 * reswidth);
                    mainMenuGUIMovement[((frames-1) - SBf)/8].position = posMMGUI[((frames-1) - SBf)/8];
                }
                else
                {
                    settingsMenu.SetActive(false);
                    pressedBack = false;
                    frames = 0;
                }
            }
            if (menu == 1)
            {
                if (frames == 1)
                {
                    settingsMenu.SetActive(true);
                    audioSource.PlayOneShot(clip);
                }
                else if(frames % 8 == 0)
                {
                    audioSource.PlayOneShot(clip);
                }
                if (frames < IPf + 1)
                {
                    posIGUI[(frames-1)/8] = inputsGUIMovement[(frames-1)/8].position;
                    posIGUI[(frames-1)/8].x -= (float)(62.5 * reswidth);
                    inputsGUIMovement[(frames-1)/8].position = posIGUI[(frames-1)/8];
                }
                else if (frames < SBIPf + 1)
                {
                    posSGUI[((frames-1) - IPf)/8] = settingsGUIMovement[((frames-1) - IPf)/8].position;
                    posSGUI[((frames-1) - IPf)/8].x += (float)(62.5 * reswidth);
                    settingsGUIMovement[((frames-1) - IPf)/8].position = posSGUI[((frames-1) - IPf)/8];
                    if(frames < settingsMenuGUIpart.Length * 8 +1 + IPf)
                    {
                        posSGUIP[((frames-1) - IPf)/8] = settingsGUIPartMovement[((frames-1) - IPf)/8].position;
                        posSGUIP[((frames-1) - IPf)/8].x += (float)(125.0 * reswidth);
                        settingsGUIPartMovement[((frames-1) - IPf)/8].position = posSGUIP[((frames-1) - IPf)/8];
                    }
                }
                else
                {
                    inputsMenu.SetActive(false);
                    pressedBack = false;
                    frames = 0;
                }
            }
        }
        if (pressedInputsMenu)
        {
            frames++;
            if (frames == 1)
            {
                inputsMenu.SetActive(true);
                audioSource.PlayOneShot(clip);
            }
            else if(frames % 8 == 0)
            {
                audioSource.PlayOneShot(clip);
            }
            if (frames < SBf + 1)
            {
                posSGUI[(frames-1)/8] = settingsGUIMovement[(frames-1)/8].position;
                posSGUI[(frames-1)/8].x -= (float)(62.5 * reswidth);
                settingsGUIMovement[(frames-1)/8].position = posSGUI[(frames-1)/8];
                if(frames < settingsMenuGUIpart.Length * 8 +1)
                {
                    posSGUIP[(frames-1)/8] = settingsGUIPartMovement[(frames-1)/8].position;
                    posSGUIP[(frames-1)/8].x -= (float)(125.0 * reswidth);
                    settingsGUIPartMovement[(frames-1)/8].position = posSGUIP[(frames-1)/8];
                }
            }
            else if (frames < SBIPf + 1)
            {
                posIGUI[((frames-1) - SBf)/8] = inputsGUIMovement[((frames-1) - SBf)/8].position;
                posIGUI[((frames-1) - SBf)/8].x += (float)(62.5 * reswidth);
                inputsGUIMovement[((frames-1) - SBf)/8].position = posIGUI[((frames-1) - SBf)/8];
            }
            else
            {
                pressedInputsMenu = false;
                settingsMenu.SetActive(false);
                frames = 0;
            }
        }
        if (starting)
        {
            GameEngine.instance.time = 0;
            GameEngine.instance.rollTime = 0;
            GameEngine.instance.level = 0;
            GameEngine.instance.curSect = 0;
            GameEngine.instance.sectAfter20g = 0;
            GameEngine.instance.ARE = 41.66666666666666;
            GameEngine.instance.AREf = 42 - 300;
            GameEngine.instance.paused = true;
            GameEngine.instance.DAS = 25;
            GameEngine.instance.AREline = 16.66666666666666666;
            GameEngine.instance.nextibmblocks = 0;
            GameEngine.instance.LockDelay = 50;
            GameEngine.instance.lineDelayf = 0;
            GameEngine.instance.lineDelay = 25;
            GameEngine.instance.gravity = 3/64f;
            GameEngine.instance.singles = 0;
            GameEngine.instance.doubles = 0;
            GameEngine.instance.triples = 0;
            GameEngine.instance.tetrises = 0;
            GameEngine.instance.pentrises = 0;
            GameEngine.instance.sixtrises = 0;
            GameEngine.instance.septrises = 0;
            GameEngine.instance.octrises = 0;
            GameEngine.instance.totalLines = 0;
            GameEngine.instance.lineClonePiecesLeft = 2147483647;
            GameEngine.instance.grade = 0;
            GameEngine.instance.gradeIndicator.sprite = GameEngine.instance.gradeSprites[0];
            GameEngine.instance.bgmlv = 1;
            GameEngine.instance.timeCounter.text = "00:00:00";
            GameEngine.instance.nextSecLv.text = "100";
            GameEngine.instance.levelTextRender.text = "0";
            GameEngine.instance.ending = false;
            GameEngine.instance.sectionTime = new int[21];
            Destroy(curBoard);
            GameEngine.instance.gameMusic.Stop();
            GameEngine.instance.gameMusic.clip = GameEngine.instance.bgm_1p_lv[0];
            GameEngine.instance.gameMusic.volume = 1f;
            GameEngine.instance.tileInvisTime = -1;
            if (!mainMenuMusic.isPlaying)mainMenuMusic.Play();
            frames++;
            // if (SceneManager.GetActiveScene().name != "MenuScene")SceneManager.LoadScene("MenuScene");
            // imgbg.SetActive(true);
            // imgprjchlg.SetActive(true);
            indicatorActivity(false, 7);
            mainMenu.SetActive(true);
            if (frames == 1)
            {
                // mainMenu.SetActive(true);
                audioSource.PlayOneShot(clip);
            }
            else if(frames % 17 == 16 && frames < mainMenuGUI.Length * 17 - 1)
            {
                if(mainMenuGUI[(frames-1)/17].activeSelf)audioSource.PlayOneShot(clip);
            }
            if(frames < mainMenuGUI.Length * 17 + 1)
            {
                posMMGUI[(frames-1)/17] = mainMenuGUIMovement[(frames-1)/17].position;
                posMMGUI[(frames-1)/17].x += (float)(29.411764705882352941176470588235 * reswidth);
                mainMenuGUIMovement[(frames-1)/17].position = posMMGUI[(frames-1)/17];
            }
            else
            {
                starting = false;
                frames = 0;
            }
        }
    }
}
