using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Discord;
using TMPro;
using MLAPI;
using MLAPI.Messaging;

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
    public static List<GameObject> players;
    public static List<NetworkBoard> playersComponent;
    public NetworkBoard yourPlayer;
    public Discord.Discord discord;
    public static MenuEngine instance;
    public int frames = 0;
    public int menu = 0;
    public bool quitting = false;
    public bool startGame = false;
    public bool GameOver = false;
    public bool IntentionalGameOver = false;
    public bool starting;
    public bool pressedSubMenu;
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
        {"プレイ!", "セッチング", "終了する"},

    }, settingsLangString = 
    {
        {"Resolution:", "Inputs", "Rotation Systems", "Custom Mode settings", "Preferences settings", "Tuning", "< Back"},
        {"Разрешение:", "Вводы", "Системы вращения", "Настройки режима", "Настройки предпотчении", "Тьюнинг", "< Назад"},
        {"解像度", "入力", "回転システム", "カスタムモードの設定", "プリファレンス設定", "チューニング", "< バック"},

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
        {"シングル：", "ダブル：", "トリプル", "テトリス：", "行：", "合計", "ピース", "成績：", "総合成績スコア", "レベル：", "時間", "", "", "", "", ""},
    };
    Language previousLang;
    // public void InstantiatePlayer()
    // {
    //     GameObject newBoard = Instantiate(inGameBoard, transform);
    //     players.Add(newBoard);
    // }
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
    public void SubMenu(int subMenuContext)
    {
        if (!starting && !pressedSubMenu) 
        {
            pressedSubMenu = true; 
            menu = subMenuContext + 1;
        }
    }
    
    public void Back()
    {
        if (!pressedBack && !pressedSubMenu)
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
        players = new List<GameObject>();
        playersComponent = new List<NetworkBoard>();
        Application.targetFrameRate = Screen.currentResolution.refreshRate * 4;
        alreadystarted = true;
        instance = this;
    }
    public bool drpcSwitch;

    [SerializeField] void SwitchDRPC()
    {
        drpcSwitch = !drpcSwitch;
        if (!drpcSwitch)
            discord.Dispose();
        else discord = new Discord.Discord(836600860976349192, (UInt64)Discord.CreateFlags.Default);
    }
    private int resRefreshrates = 0;
    void Start()
    {
        if(!platformCompat()) {reswidth = 1f; settingsMovement.position += new Vector3(0f,47.77f * (Screen.height / 1080.0f),0f);}
        GameObject[] objs = GameObject.FindGameObjectsWithTag("menuenginerelated");
        GameObject[] canvasobjs = GameObject.FindGameObjectsWithTag("Canvas");
        GameObject[] gameoverseobjs = GameObject.FindGameObjectsWithTag("GameOverSE");
        GameObject[] buttonseobjs = GameObject.FindGameObjectsWithTag("ButtonSE");

        // if(canvasobjs.Length > 1)
        // {
        //     for (int i = 0; i < canvasobjs.Length; i++)
        //     {
        //         if (i % 2 == 1) Destroy(canvasobjs[i/2+1]);
        //     }
        // }
        // if(gameoverseobjs.Length > 1)
        // {
        //     for (int i = 0; i < gameoverseobjs.Length; i++)
        //     {
        //         if (i % 2 == 1) Destroy(gameoverseobjs[i/2+1]);
        //     }
        // }
        // if(buttonseobjs.Length > 1)
        // {
        //     for (int i = 0; i < buttonseobjs.Length; i++)
        //     {
        //         if (i % 2 == 1) Destroy(buttonseobjs[i/2+1]);
        //     }
        // }
        // if (objs.Length > 1)
        // {
        //     for (int i = 0; i < objs.Length; i++)
        //     {
        //         if (i % 2 == 1) Destroy(objs[i/2+1]);
        //     }
        // }

        // DontDestroyOnLoad(objs[0]);
        // DontDestroyOnLoad(canvasobjs[0]);
        // DontDestroyOnLoad(gameoverseobjs[0]);
        // DontDestroyOnLoad(buttonseobjs[0]);
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
            if(!Application.isMobilePlatform)mainMenuGUI[2].SetActive(false);
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
        posRS = rotationSystemsMovement.position;
        posRS.x -= (float)(500.0 * reswidth);
        rotationSystemsMovement.position = posRS;
        posCMS = customModeSettingsMovement.position;
        posCMS.x -= (float)(500.0 * reswidth);
        customModeSettingsMovement.position = posCMS;
        posP = preferencesMovement.position;
        posP.x -= (float)(500.0 * reswidth);
        preferencesMovement.position = posP;
        posT = tuningMovement.position;
        posT.x -= (float)(500.0 * reswidth);
        tuningMovement.position = posT;
    }
    public void ExtractStatsToNotifications(NetworkBoard board)
    {
        int pieceCountHoldRed = board.piecesController.pieceHold == 28 ? 0 : -1;
        if(board.singles > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 0] + board.singles, Color.white); // Singles
        if(board.doubles > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 1] + board.doubles, Color.white); // Doubles
        if(board.triples > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 2] + board.triples, Color.white); // Triples
        if(board.tetrises > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 3] + board.tetrises, Color.white); // Tetrises
        if(board.pentrises > 0) NotificationEngine.instance.InstantiateNotification("5 " + notifLangString[(int)language, 4] + board.pentrises, Color.white); // 5 lines
        if(board.sixtrises > 0) NotificationEngine.instance.InstantiateNotification("6 " + notifLangString[(int)language, 4] + board.sixtrises, Color.white); // 6 lines
        if(board.septrises > 0) NotificationEngine.instance.InstantiateNotification("7 " + notifLangString[(int)language, 4] + board.septrises, Color.white); // 7 lines
        if(board.octrises > 0) NotificationEngine.instance.InstantiateNotification("8+ " + notifLangString[(int)language, 4] + board.octrises, Color.white); // 8+ lines
        if(board.totalLines > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 5] + notifLangString[(int)language, 4] + board.totalLines, Color.white); // Total lines
        if(board.piecesController.pieces > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 6] + (board.piecesController.pieces + pieceCountHoldRed), Color.white); // Pieces
        NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 7] + gradeStringConversion[board.grade], Color.white); // Grade
        if(board.statGradePoints > 0) {NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 8], Color.white); // Total grade score
        NotificationEngine.instance.InstantiateNotification(Math.Floor(board.statGradePoints).ToString(), Color.white);}
        NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 9] + board.level + "/" + (board.level < 2100 ? (board.curSect + 1) * 100 : 2100), Color.white); // Level
        NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 10] + board.gravity, Color.white); // Gravity
        NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 11] + board.timeCounter.text, Color.white);
    }
    public void UpdateLang()
    {
        if(language == Language.日本語)
        {
            NotificationEngine.instance.InstantiateNotification("Notice! Japanese translation is not perfect.");
            NotificationEngine.instance.InstantiateNotification("通知！ 日本語の翻訳は完璧ではありません。");
        }
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
        if (platformCompat() && drpcSwitch)discord.Dispose();
    }
    double framerate;
    [SerializeField] List<double> frameratebuffer;
    // rough framerate measurement
    void Update()
    {
        double rawframetime = Time.deltaTime / Time.timeScale;
        frameratebuffer.Add(rawframetime);
        if (frameratebuffer.Count > 10)
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
        if(drpcSwitch) if (platformCompat() && yourPlayer != null)
        {
            var activityManager = discord.GetActivityManager();
            int rpclvl = yourPlayer.level < yourPlayer.endingLevel ? (yourPlayer.curSect + 1) * 100 : yourPlayer.endingLevel;
            var activity = new Activity
            {
                Details = yourPlayer.ending ? "Roll time left: " + yourPlayer.rollTimeCounter.text 
                : curBoard != null ? "Level " + yourPlayer.level + " | " + rpclvl + (yourPlayer.level > 800 ? ". Struggling." : string.Empty) : null,

                State = !Application.genuineCheckAvailable ? "The game is tampered" : framerate > 2600 ? "Suspiciously smooth" : framerate < 10 ? "Performance issues" 
                : yourPlayer.IntentionalGameOver ? "Exiting..." : yourPlayer.GameOver ? "Topped out" : curBoard != null && yourPlayer.paused && !yourPlayer.FrameStep ? "Paused" 
                : curBoard != null && ReplayRecord.instance.mode == ReplayModeType.read ? "Currently replaying" 
                : curBoard != null && yourPlayer.paused && yourPlayer.FrameStep ? "Currently playing (Framestepping)" 
                : curBoard != null ? "Currently playing" : quitting ? "Quitting" : menu == 1 ? "Currently in settings menu" : "Currently in main menu",
                Assets = {
                    LargeImage = "icon"
                }
            };
            activityManager.UpdateActivity(activity, (res) => {
            });
            if(drpcSwitch)discord.RunCallbacks();
        }
        // Dealing with varied lengths.
        var MMf = mainMenuGUI.Length * 8;
        var SBf = settingsMenuGUI.Length * 8;
        var IPf = inputsMenuGUI.Length * 8;
        var RSf = rotationSystemsMenuGUI.Length * 8;
        var CMSf = customModeSettingsMenuGUI.Length * 8;
        var PRf = preferencesMenuGUI.Length * 8;
        var TUf = tuningMenuGUI.Length * 8;
        var SBVf = settingsGUIMovement.Length * 8;
        var MMSBf = MMf + SBf;
        var SBIPf = SBf + IPf;
        var SBRSf = SBf + RSf;
        var SBCMSf = SBf + CMSf;
        var SBPRf = SBf + PRf;
        var SBTUf = SBf + TUf;
        var MMSf = MMf + 50;
        var MMSCf = MMSf + SBf;
        var MMSCVf = MMSf + SBVf;

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
                    frames = 0;
                    GameOver = false;
                    IntentionalGameOver = false;
                    starting = true;
                    this.transform.position = Vector3.zero;
                    BackgroundController.bginstance.TriggerBackgroundChange(0);
                    if (ReplayRecord.instance.mode == ReplayModeType.write)
                    {
                        ReplayRecord.instance.SaveReplay(DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss"));
                    }
                }
            }
        }
        if (quitting || (pressedSubMenu && menu == 1) || startGame)
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
                    MLAPI.NetworkManager.Singleton.StartHost();
                    UnityEngine.Random.InitState(SeedManager.NewSeed());
                    ReplayRecord.instance.Reset();
                    ReplayRecord.seed = SeedManager.seed;
                    
                    if (ReplayRecord.instance.mode == ReplayModeType.read)
                    {
                        ReplayRecord.instance.LoadReplay("1");
                        UnityEngine.Random.InitState(ReplayRecord.seed);
                    }
                    mainMenu.SetActive(false);
                    GameEngine.instance.gradePoints = 0;
                    GameEngine.instance.gradePointRequirement = 100;
                    GameEngine.instance.statGradePoints = 0;
                    // SceneManager.LoadScene("GameScene");

                    GameObject board = GameObject.Instantiate(inGameBoard, transform);
                    curBoard = board;
                    NetworkBoard netBoard = curBoard.GetComponent<NetworkBoard>();
                    board.transform.position = new Vector3(0.0f, 20f, 0.0f);
                    mainMenuMusic.Stop();
                    // netBoard.piecesController.UpdateShownPieces();
                }
                if (frames == MMf + 12)   curBoard.transform.position = new Vector3(0.0f, 18f, 0.0f);
                if (frames == MMf + 13)   curBoard.transform.position = new Vector3(0.0f, 16f, 0.0f);
                if (frames == MMf + 14)   curBoard.transform.position = new Vector3(0.0f, 14f, 0.0f);
                if (frames == MMf + 15)   curBoard.transform.position = new Vector3(0.0f, 12f, 0.0f);
                if (frames == MMf + 16)   curBoard.transform.position = new Vector3(0.0f, 10f, 0.0f);
                if (frames == MMf + 17)   curBoard.transform.position = new Vector3(0.0f, 8f, 0.0f);
                if (frames == MMf + 18)   curBoard.transform.position = new Vector3(0.0f, 6f, 0.0f);
                if (frames == MMf + 19)   curBoard.transform.position = new Vector3(0.0f, 4f, 0.0f);
                if (frames == MMf + 20)   curBoard.transform.position = new Vector3(0.0f, 2f, 0.0f);
                if (frames == MMf + 21)   curBoard.transform.position = new Vector3(0.0f, 0f, 0.0f);
                if (frames == MMf + 22)   curBoard.transform.position = new Vector3(0.0f, -0.8f, 0.0f);
                if (frames == MMf + 23)   curBoard.transform.position = new Vector3(0.0f, -1.4f, 0.0f);
                if (frames == MMf + 24)   curBoard.transform.position = new Vector3(0.0f, -2f, 0.0f);
                if (frames == MMf + 25)   curBoard.transform.position = new Vector3(0.0f, -1.3f, 0.0f);
                if (frames == MMf + 26)   curBoard.transform.position = new Vector3(0.0f, -0.7f, 0.0f);
                if (frames == MMf + 27)   curBoard.transform.position = new Vector3(0.0f, 0f, 0.0f);
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
                    NetworkBoard netBoard = curBoard.GetComponent<NetworkBoard>();
                    netBoard.piecesController.UpdatePieceBag();
                }
            }
            if (pressedSubMenu && frames > MMSf && menu == 1)
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
                    pressedSubMenu = false;
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
            if (inputsMenu.activeSelf)
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
            if (rotationSystemsMenu.activeSelf)
            {
                if (frames == 1)
                {
                    settingsMenu.SetActive(true);
                    audioSource.PlayOneShot(clip);
                }
                else if (frames % 8 == 0)
                {
                    audioSource.PlayOneShot(clip);
                }
                if (frames < RSf + 1)
                {
                    posRSGUI[(frames - 1) / 8] = rotationSystemsGUIMovement[(frames - 1) / 8].position;
                    posRSGUI[(frames - 1) / 8].x -= (float)(62.5 * reswidth);
                    rotationSystemsGUIMovement[(frames - 1) / 8].position = posRSGUI[(frames - 1) / 8];
                }
                else if (frames < SBRSf + 1)
                {
                    posSGUI[((frames - 1) - RSf) / 8] = settingsGUIMovement[((frames - 1) - RSf) / 8].position;
                    posSGUI[((frames - 1) - RSf) / 8].x += (float)(62.5 * reswidth);
                    settingsGUIMovement[((frames - 1) - RSf) / 8].position = posSGUI[((frames - 1) - RSf) / 8];
                    if (frames < settingsMenuGUIpart.Length * 8 + 1 + RSf)
                    {
                        posSGUIP[((frames - 1) - RSf) / 8] = settingsGUIPartMovement[((frames - 1) - RSf) / 8].position;
                        posSGUIP[((frames - 1) - RSf) / 8].x += (float)(125.0 * reswidth);
                        settingsGUIPartMovement[((frames - 1) - RSf) / 8].position = posSGUIP[((frames - 1) - RSf) / 8];
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
        if (pressedSubMenu)
        {
            // Inputs menu
            if (menu == 2)
            {
                frames++;
                if (frames == 1)
                {
                    inputsMenu.SetActive(true);
                    audioSource.PlayOneShot(clip);
                }
                else if (frames % 8 == 0)
                {
                    audioSource.PlayOneShot(clip);
                }
                if (frames < SBf + 1)
                {
                    posSGUI[(frames - 1) / 8] = settingsGUIMovement[(frames - 1) / 8].position;
                    posSGUI[(frames - 1) / 8].x -= (float)(62.5 * reswidth);
                    settingsGUIMovement[(frames - 1) / 8].position = posSGUI[(frames - 1) / 8];
                    if (frames < settingsMenuGUIpart.Length * 8 + 1)
                    {
                        posSGUIP[(frames - 1) / 8] = settingsGUIPartMovement[(frames - 1) / 8].position;
                        posSGUIP[(frames - 1) / 8].x -= (float)(125.0 * reswidth);
                        settingsGUIPartMovement[(frames - 1) / 8].position = posSGUIP[(frames - 1) / 8];
                    }
                }
                else if (frames < SBIPf + 1)
                {
                    posIGUI[((frames - 1) - SBf) / 8] = inputsGUIMovement[((frames - 1) - SBf) / 8].position;
                    posIGUI[((frames - 1) - SBf) / 8].x += (float)(62.5 * reswidth);
                    inputsGUIMovement[((frames - 1) - SBf) / 8].position = posIGUI[((frames - 1) - SBf) / 8];
                }
                else
                {
                    pressedSubMenu = false;
                    settingsMenu.SetActive(false);
                    frames = 0;
                }
            }

            // Rotation Systems menu
            if (menu == 3)
            {
                frames++;
                if (frames == 1)
                {
                    inputsMenu.SetActive(true);
                    audioSource.PlayOneShot(clip);
                }
                else if (frames % 8 == 0)
                {
                    audioSource.PlayOneShot(clip);
                }
                if (frames < SBf + 1)
                {
                    posSGUI[(frames - 1) / 8] = settingsGUIMovement[(frames - 1) / 8].position;
                    posSGUI[(frames - 1) / 8].x -= (float)(62.5 * reswidth);
                    settingsGUIMovement[(frames - 1) / 8].position = posSGUI[(frames - 1) / 8];
                    if (frames < settingsMenuGUIpart.Length * 8 + 1)
                    {
                        posSGUIP[(frames - 1) / 8] = settingsGUIPartMovement[(frames - 1) / 8].position;
                        posSGUIP[(frames - 1) / 8].x -= (float)(125.0 * reswidth);
                        settingsGUIPartMovement[(frames - 1) / 8].position = posSGUIP[(frames - 1) / 8];
                    }
                }
                else if (frames < SBRSf + 1)
                {
                    posRSGUI[((frames - 1) - SBf) / 8] = rotationSystemsGUIMovement[((frames - 1) - SBf) / 8].position;
                    posRSGUI[((frames - 1) - SBf) / 8].x += (float)(62.5 * reswidth);
                    rotationSystemsGUIMovement[((frames - 1) - SBf) / 8].position = posRSGUI[((frames - 1) - SBf) / 8];
                }
                else
                {
                    pressedSubMenu = false;
                    settingsMenu.SetActive(false);
                    frames = 0;
                }
            }
        }
        if (starting)
        {
            if (!mainMenuMusic.isPlaying)mainMenuMusic.Play();
            frames++;
            // if (SceneManager.GetActiveScene().name != "MenuScene")SceneManager.LoadScene("MenuScene");
            // imgbg.SetActive(true);
            // imgprjchlg.SetActive(true);
            mainMenu.SetActive(true);
            if (frames == 1)
            {
                // GameEngine.instance.time = 0;
                // GameEngine.instance.rollTime = 0;
                // GameEngine.instance.level = 0;
                // GameEngine.instance.curSect = 0;
                // GameEngine.instance.sectAfter20g = 0;
                // GameEngine.instance.ARE = 41.66666666666666;
                // GameEngine.instance.AREf = 42 - 300;
                // GameEngine.instance.paused = true;
                // GameEngine.instance.DAS = 25;
                // GameEngine.instance.AREline = 16.66666666666666666;
                // GameEngine.instance.nextibmblocks = 0;
                // GameEngine.instance.LockDelay = 50;
                // GameEngine.instance.lineDelayf = 0;
                // GameEngine.instance.lineDelay = 25;
                // GameEngine.instance.gravity = 3 / 64f;
                // GameEngine.instance.singles = 0;
                // GameEngine.instance.doubles = 0;
                // GameEngine.instance.triples = 0;
                // GameEngine.instance.tetrises = 0;
                // GameEngine.instance.pentrises = 0;
                // GameEngine.instance.sixtrises = 0;
                // GameEngine.instance.septrises = 0;
                // GameEngine.instance.octrises = 0;
                // GameEngine.instance.totalLines = 0;
                // GameEngine.instance.lineClonePiecesLeft = 2147483647;
                // GameEngine.instance.grade = 0;
                // GameEngine.instance.bgmlv = 1;
                // GameEngine.instance.timeCounter.text = "00:00:00";
                // GameEngine.instance.nextSecLv.text = "100";
                // GameEngine.instance.levelTextRender.text = "0";
                // GameEngine.instance.ending = false;
                // GameEngine.instance.sectionTime = new int[21];
                // Destroy(curBoard);
                // GameEngine.instance.gameMusic.Stop();
                // GameEngine.instance.gameMusic.clip = GameEngine.instance.bgm_1p_lv[0];
                // GameEngine.instance.gameMusic.volume = 1f;
                // GameEngine.instance.tileInvisTime = -1;
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
    // [ServerRpc]
    // void StartServerUp()
    // {
        
    // }
}
