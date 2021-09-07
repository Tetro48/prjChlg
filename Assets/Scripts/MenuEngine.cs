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
    public NetworkBoard yourPlayer;
    public Discord.Discord discord;
    public static MenuEngine instance;
    public int frames = 0;
    public int menu = 0, prevMenu;
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

    [SerializeField] double[] UITimeDeltas;

    [SerializeField]
    GameObject[] menuSectors;
    public AudioSource audioSource, audioSource2, mainMenuMusic, audioSourceConfirmation;
    public AudioClip clip, topoutSE;
    public GameObject inGameBoard, curBoard;
    public AudioClip ModeOK;
    public GameObject mainMenu, settingsMenu, inputsMenu, rotationSystemsMenu, imgprjchlg, mobileInput;
    public GameObject[] mainMenuGUI, settingsMenuGUI, settingsMenuGUIpart, inputsMenuGUI, rotationSystemsMenuGUI, customModeSettingsMenuGUI, preferencesMenuGUI, tuningMenuGUI;
    public RectTransform mainMenuMovement, settingsMovement,settingsGUI1PartMovement, inputsMovement, rotationSystemsMovement, customModeSettingsMovement, preferencesMovement, tuningMovement;
    public RectTransform[] mainMenuGUIMovement, settingsGUIMovement, settingsGUIPartMovement, inputsGUIMovement, rotationSystemsGUIMovement, customModeSettingsGUIMovement, preferencesGUIMovement, tuningGUIMovement;
    public TextMeshProUGUI[] mainMenuGUIText, settingsGUIText, inputsGUIText;
    Vector3 boardpos, boardrot, posMM, posS, posSGUI1P, posI, posRS, posCMS, posP, posT;
    public Vector3[] posMMGUI, posSGUI, posSGUIP, posIGUI, posRSGUI, posCMSGUI, posPGUI, posTGUI;
    Resolution[] resolutions;
    public float reswidth;

    /// <summary>
    /// How long does each button takes to move
    /// </summary>
    public double buttonMovementInSeconds;

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
    public void InstantiatePlayer(double LockDelay = 50, double ARE = 41.6666666, double AREline = 16.6666666, double lineDelay = 25, float gravity = 3/64f, int nextPieces = 7)
    {
        GameObject newBoard = Instantiate(inGameBoard, transform);
        NetworkBoard component = newBoard.GetComponent<NetworkBoard>();
        NetworkBoard.player.Add(component);
        component.LockDelay = LockDelay;
        component.ARE = ARE;
        component.AREline = AREline;
        component.gravity = gravity;
        component.nextPieces = nextPieces;
        component.piecesController.InitiatePieces();
    }
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
    
    public void ChangeMenu(int menuType)
    {
        /*
        Pseudocode
        
        if menuType is same as menu or when button is already pressed or when starting up then do nothing aka return
        if menuType more than menu then pressedSubMenu equals true
        else then pressedBack equals true and prevMenu equals menu
        menu equals menuType
        menuSector array MenuType SetActive true
        */

        if (menuType == menu || pressedBack || pressedSubMenu || starting) return;
        if (menuType > menu) pressedSubMenu = true;
        else {pressedBack = true; prevMenu = menu;}
        menu = menuType;
        menuSectors[menuType].SetActive(true);
    }

    bool CheckUIScroll(int timeIndex, bool side, int count)
    {
        double variable1 = UITimeDeltas[timeIndex];
        double variable2 = buttonMovementInSeconds;
        bool output;
        if(side)
        {
            variable1 += Time.unscaledDeltaTime;
            variable2 *= System.Math.Ceiling(UITimeDeltas[timeIndex] / buttonMovementInSeconds);
            if(variable1 >= (double)count) output = false;
            else output = variable1 > variable2;
        }
        else
        {
            variable1 -= Time.unscaledDeltaTime;
            variable2 *= System.Math.Floor(UITimeDeltas[timeIndex] / buttonMovementInSeconds);
            if(variable1 <= 0d) output = false;
            else output = variable1 < variable2;
            
        }

        // Debug.Log(variable1 + " / " + variable2 + ". " + output); // for debug purposes
        return output;
    }

    /// <summary>
    /// This deals with moving user interface elements. Everything in dynamic timing.
    /// </summary>
    /// <param name="UIElements"> Elements of user interface. It's in arrays, so keep that in mind. </param>
    /// <param name="UITimeDeltaIndex"> Index of user interface delta time. </param>
    /// <param name="side"> False -> Left side. True -> Right side. </param>

    private void MoveCoupleUIElements(RectTransform[] UIElements, int UITimeDeltaIndex, bool side, float multiplication = 1f)
    {
        float time = Time.deltaTime;
        if(multiplication == 1f) if(CheckUIScroll(UITimeDeltaIndex, side, UIElements.Length))
        audioSource.PlayOneShot(clip);
        // Debug.Log(time);
        float reversibleTime = time;
        if(!side) reversibleTime *= -1;
        UITimeDeltas[UITimeDeltaIndex] += reversibleTime;
        float timeToPosX = (float)(UITimeDeltas[UITimeDeltaIndex] / buttonMovementInSeconds) * 500 - 250;
        if(side) for (int i = 0; i < UIElements.Length; i++)
        {
            Vector3 tempPos = UIElements[i].position;
            tempPos.x = Mathf.Clamp((timeToPosX - 500 * i) * reswidth * multiplication, -250f * reswidth * multiplication, 250f * reswidth * multiplication);
            UIElements[i].position = tempPos;
        }
        else for (int i = UIElements.Length - 1; i >= 0 ; i--)
        {
            Vector3 tempPos = UIElements[i].position;
            tempPos.x = Mathf.Clamp((timeToPosX - 500 * (UIElements.Length-i-1)) * reswidth * multiplication, -250f * reswidth * multiplication, 250f * reswidth * multiplication);
            UIElements[i].position = tempPos;
        }
    }
    double CalculateButtonTime(RectTransform[] transforms)
    {
        return buttonMovementInSeconds * transforms.Length;
    }

    void MoveEntireArrayOfButtons(RectTransform[] transforms)
    {
        foreach (var transform in transforms)
        {
            transform.position -= new Vector3(500f*reswidth,0f,0f);
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
        Application.targetFrameRate = Screen.currentResolution.refreshRate * 4;
        alreadystarted = true;
        instance = this;
    }
    public bool drpcSwitch;

    public void SwitchDRPC()
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
        MoveEntireArrayOfButtons(mainMenuGUIMovement);
        MoveEntireArrayOfButtons(settingsGUIMovement);
        MoveEntireArrayOfButtons(inputsGUIMovement);
        MoveEntireArrayOfButtons(rotationSystemsGUIMovement);
        MoveEntireArrayOfButtons(customModeSettingsGUIMovement);
        MoveEntireArrayOfButtons(preferencesGUIMovement);
        MoveEntireArrayOfButtons(tuningGUIMovement);
    }
    public bool IfDoubleIsInValueRange(double test, double variable1, double variable2)
    {
        return (test >= variable1 || test <= variable2);
    }
    public void ExtractStatsToNotifications(NetworkBoard board)
    {
        if(board.singles > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 0] + board.singles, Color.white); // Singles
        if(board.doubles > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 1] + board.doubles, Color.white); // Doubles
        if(board.triples > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 2] + board.triples, Color.white); // Triples
        if(board.tetrises > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 3] + board.tetrises, Color.white); // Tetrises
        if(board.pentrises > 0) NotificationEngine.instance.InstantiateNotification("5 " + notifLangString[(int)language, 4] + board.pentrises, Color.white); // 5 lines
        if(board.sixtrises > 0) NotificationEngine.instance.InstantiateNotification("6 " + notifLangString[(int)language, 4] + board.sixtrises, Color.white); // 6 lines
        if(board.septrises > 0) NotificationEngine.instance.InstantiateNotification("7 " + notifLangString[(int)language, 4] + board.septrises, Color.white); // 7 lines
        if(board.octrises > 0) NotificationEngine.instance.InstantiateNotification("8+ " + notifLangString[(int)language, 4] + board.octrises, Color.white); // 8+ lines
        if(board.totalLines > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 5] + notifLangString[(int)language, 4] + board.totalLines, Color.white); // Total lines
        if(board.piecesController.lockedPieces > 0) NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 6] + (board.piecesController.lockedPieces), Color.white); // Pieces
        NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 7] + gradeStringConversion[board.grade], Color.white); // Grade
        if(board.statGradePoints > 0) {NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 8], Color.white); // Total grade score
        NotificationEngine.instance.InstantiateNotification(Math.Floor(board.statGradePoints).ToString(), Color.white);}
        NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 9] + board.level + "/" + (board.level < 2100 ? (board.curSect + 1) * 100 : 2100), Color.white); // Level
        NotificationEngine.instance.InstantiateNotification(notifLangString[(int)language, 10] + (board.gravity / 6 * Time.fixedDeltaTime * 1000), Color.white); // Gravity
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
    bool executedOnce = false;
    // rough framerate measurement
    void Update()
    {
        double rawframetime = Time.unscaledDeltaTime;
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
        if (previousLang != language)
        {
            previousLang = language;
            UpdateLang();
        }
        if(drpcSwitch) if (platformCompat())
        {
            var activityManager = discord.GetActivityManager();
            int rpclvl = 0;
            if(yourPlayer != null) rpclvl = yourPlayer.level < yourPlayer.endingLevel ? (yourPlayer.curSect + 1) * 100 : yourPlayer.endingLevel;
            Activity activity;
            if(yourPlayer != null)activity = new Activity
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
            else activity = new Activity
            {
                State = !Application.genuineCheckAvailable ? "The game is tampered" : framerate > 2600 ? "Suspiciously smooth" : framerate < 10 ? "Performance issues"
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
        // if (GameOver)
        // {
        //     frames++;
        //     if(frames > 300)
        //     {
        //         boardpos = this.transform.position;
        //         boardpos.y -= (float)(0.1f + (((frames-300) / 33) * ((frames-300) / 33)));
        //         this.transform.position = boardpos;
        //         // boardrot = curBoard.transform.Rotate;
        //         // boardrot.z -= 0.16f;
        //         curBoard.transform.Rotate(new Vector3(0f, 0f, -0.1f - (float)(((frames-300) / 66) * ((frames-400) / 66))));
        //         if (frames == 301)
        //         {
        //             audioSource2.PlayOneShot(topoutSE);
        //         }
        //         if (frames == 351)
        //         {
        //             audioSource2.Play();
        //         }
        //         if (frames == 400)
        //         {
        //             // SceneManager.LoadScene("MenuScene");
        //         }
        //         if (frames == 401)
        //         {
        //             frames = 0;
        //             GameOver = false;
        //             IntentionalGameOver = false;
        //             starting = true;
        //             this.transform.position = Vector3.zero;
        //             BackgroundController.bginstance.TriggerBackgroundChange(0);
        //             if (ReplayRecord.instance.mode == ReplayModeType.write)
        //             {
        //                 ReplayRecord.instance.SaveReplay(DateTime.Now.ToString("MM-dd-yyyy-HH-mm-ss"));
        //             }
        //         }
        //     }
        // }
        if (quitting || (pressedSubMenu && menu == 1) || startGame)
        {
            //Movement to the left
            if(UITimeDeltas[0] > 0)
            {
                MoveCoupleUIElements(mainMenuGUIMovement, 0, false);
            }
            else if(quitting)
            {
                if(drpcSwitch) discord.Dispose();
                Application.Quit();
            }
            // if (frames == 1)
            // {
            //     mainMenu.SetActive(true);
            //     audioSource.PlayOneShot(clip);
            // }
            // else if(frames % 8 == 7 && frames < MMf -1)
            // {
            //     if(mainMenuGUI[(frames-1)/8].activeSelf)audioSource.PlayOneShot(clip);
            // }
            // if(frames < MMf + 1)
            // {
            //     posMMGUI[(frames-1)/8] = mainMenuGUIMovement[(frames-1)/8].position;
            //     posMMGUI[(frames-1)/8].x -= (float)(62.5 * reswidth);
            //     mainMenuGUIMovement[(frames-1)/8].position = posMMGUI[(frames-1)/8];
            // }
            else if (startGame && UITimeDeltas[0] <= 0)
            {
                UITimeDeltas[0] -= Time.deltaTime;
                if (!executedOnce)
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
                    executedOnce = true;
                    // GameEngine.instance.gradePoints = 0;
                    // GameEngine.instance.gradePointRequirement = 100;
                    // GameEngine.instance.statGradePoints = 0;
                    // SceneManager.LoadScene("GameScene");

                    InstantiatePlayer();
                    // GameObject board = GameObject.Instantiate(inGameBoard, transform);
                    // curBoard = board;
                    // NetworkBoard netBoard = curBoard.GetComponent<NetworkBoard>();
                    // board.transform.position = new Vector3(0.0f, 20f, 0.0f);
                    mainMenuMusic.Stop();
                    // netBoard.piecesController.UpdateShownPieces();
                }
                if (UITimeDeltas[0] < -0.17)
                {
                    // PiecesController.instance.bagPieceResult=Random.Range(0,7);
                    startGame = false;
                    frames = 0;
                    UITimeDeltas[0] = 0d;
                    // imgbg.SetActive(false);
                    // imgprjchlg.SetActive(false);
                    NetworkBoard netBoard = curBoard.GetComponent<NetworkBoard>();
                    netBoard.paused = false;
                    executedOnce = false;
                    // netBoard.piecesController.UpdatePieceBag();
                }
            }
        }
        if (pressedBack)
        {
            
            switch (prevMenu)
            {
                //In Settings menu
                case 1:
                
                    //Movement to the right
                    if(UITimeDeltas[1] > 0d)
                    {
                        MoveCoupleUIElements(settingsGUIMovement, 1, false);
                        if(UITimeDeltas[2] > 0d)
                        MoveCoupleUIElements(settingsGUIPartMovement, 2, false, 2f);
                    }
                    //Movement to the left
                    else if (UITimeDeltas[0] < CalculateButtonTime(mainMenuGUIMovement))
                    {
                        MoveCoupleUIElements(mainMenuGUIMovement, 0, true);
                    }
                    else
                    {
                        menuSectors[1].SetActive(false);
                        UITimeDeltas[0] = CalculateButtonTime(mainMenuGUIMovement);
                        UITimeDeltas[1] = 0;
                        UITimeDeltas[2] = 0;
                        pressedBack = false;
                    }

                break;

                //In Inputs menu
                case 2:
                
                    //Movement to the right
                    if(UITimeDeltas[3] > 0d)
                    {
                        MoveCoupleUIElements(inputsGUIMovement, 3, false);
                    }
                    //Movement to the left
                    else if (UITimeDeltas[1] < CalculateButtonTime(settingsGUIMovement))
                    {
                        MoveCoupleUIElements(settingsGUIMovement, 1, true);
                    }
                    else
                    {
                        menuSectors[2].SetActive(false);
                        UITimeDeltas[1] = CalculateButtonTime(settingsGUIMovement);
                        UITimeDeltas[3] = 0;
                        pressedBack = false;
                    }
                    
                break;
                
                //In Rotation Systems menu
                case 3:
                
                    //Movement to the right
                    if(UITimeDeltas[4] > 0d)
                    {
                        MoveCoupleUIElements(rotationSystemsGUIMovement, 4, false);
                    }
                    //Movement to the left
                    else if (UITimeDeltas[0] < CalculateButtonTime(settingsGUIMovement))
                    {
                        MoveCoupleUIElements(settingsGUIMovement, 1, true);
                    }
                    else
                    {
                        menuSectors[3].SetActive(false);
                        UITimeDeltas[1] = CalculateButtonTime(settingsGUIMovement);
                        UITimeDeltas[4] = 0;
                        pressedBack = false;
                    }
                    
                break;
                
                default: break;
            }
        }
        if (pressedSubMenu)
        {
            switch (menu)
            {
                //Settings button press
                case 1:

                    //Movement to the left
                    if(UITimeDeltas[0] > 0)
                    {
                        MoveCoupleUIElements(mainMenuGUIMovement, 0, false);
                    }
                    //Movement to the right
                    else if (UITimeDeltas[1] < CalculateButtonTime(settingsGUIMovement))
                    {
                        MoveCoupleUIElements(settingsGUIMovement, 1, true);
                        if(UITimeDeltas[2] < CalculateButtonTime(settingsGUIPartMovement))
                        MoveCoupleUIElements(settingsGUIPartMovement, 2, true, 3f);
                    }
                    else
                    {
                        menuSectors[0].SetActive(false);
                        UITimeDeltas[0] = 0;
                        UITimeDeltas[1] = CalculateButtonTime(settingsGUIMovement);
                        UITimeDeltas[2] = CalculateButtonTime(settingsGUIPartMovement);
                        pressedSubMenu = false;
                    }

                break;

                //Inputs button press
                case 2:

                    //Movement to the left
                    if(UITimeDeltas[1] > 0)
                    {
                        MoveCoupleUIElements(settingsGUIMovement, 1, false);
                        if(UITimeDeltas[2] > 0d)
                        MoveCoupleUIElements(settingsGUIPartMovement, 2, false, 2f);
                    }
                    //Movement to the right
                    else if (UITimeDeltas[3] < CalculateButtonTime(inputsGUIMovement))
                    {
                        MoveCoupleUIElements(inputsGUIMovement, 3, true);
                    }
                    else
                    {
                        menuSectors[1].SetActive(false);
                        UITimeDeltas[0] = 0;
                        UITimeDeltas[3] = CalculateButtonTime(inputsGUIMovement);
                        pressedSubMenu = false;
                    }

                break;

                //Rotation Systems button press
                case 3:
                

                    //Movement to the left
                    if(UITimeDeltas[1] > 0)
                    {
                        MoveCoupleUIElements(settingsGUIMovement, 1, false);
                        if(UITimeDeltas[2] > 0d)
                        MoveCoupleUIElements(settingsGUIPartMovement, 2, false, 2f);
                    }
                    //Movement to the right
                    else if (UITimeDeltas[4] < CalculateButtonTime(rotationSystemsGUIMovement))
                    {
                        MoveCoupleUIElements(rotationSystemsGUIMovement, 4, true);
                    }
                    else
                    {
                        menuSectors[1].SetActive(false);
                        UITimeDeltas[0] = 0;
                        UITimeDeltas[4] = CalculateButtonTime(rotationSystemsGUIMovement);
                        pressedSubMenu = false;
                    }
                
                break;

                default: break;
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
           
            //Movement to the right
            if(UITimeDeltas[0] < CalculateButtonTime(mainMenuGUIMovement))
            {
                MoveCoupleUIElements(mainMenuGUIMovement, 0, true);
            }
            else
            {
                UITimeDeltas[0] = CalculateButtonTime(mainMenuGUIMovement);
                starting = false;
            }
        }
    }
    // [ServerRpc]
    // void StartServerUp()
    // {
        
    // }
}
