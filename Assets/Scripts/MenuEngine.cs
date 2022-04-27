using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Discord;
using TMPro;
using Unity.Netcode;

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

public enum Language {English, Русский, 日本語};

// Please note: Cosmic rays can affect your computer's bits. If you experience bugs that is NOT caused by a code, it's extremely likely that bit flip IS a cause of it.
// These bit flips are soft errors, and it's quite hard to notice.

[RequireComponent(typeof(AudioSource))]
public class MenuEngine : MonoBehaviour
{
    public InputActionAsset modifiableInputAsset;
    public Language language;
    public static List<GameObject> players;
    public NetworkBoard mainPlayer;
    public Discord.Discord discord;
    public static MenuEngine instance;
    public int menu = 0, prevMenu;
    public bool quitting = false;
    public bool startGame = false;
    public bool GameOver = false;
    public bool IntentionalGameOver = false;
    public bool starting;
    public bool pressedSubMenu;
    public bool pressedBack;

    public string[] gradeStringConversion = {"9","8","7","6","5","4","3","2","1","S1","S2","S3","S4","S5","S6","S7","S8","S9","GM","FPGM"};

    public TextMeshProUGUI framerateCounter;

    public TMP_Dropdown resDropdown;

    [SerializeField] double[] UITimeDeltas;

    [SerializeField]
    GameObject[] menuSectors;
    public AudioSource audioSource, audioSource2, mainMenuMusic, audioSourceConfirmation;
    public AudioClip clip, topoutSE;
    public GameObject inGameBoard, curBoard;
    public AudioClip ModeOK, messageboxPopup;
    public GameObject imgprjchlg, mobileInput;
    public RectTransform[] mainMenuGUIMovement, settingsGUIMovement, settingsGUIPartMovement;
    public MenuSegment[] segments;

    public TextMeshProUGUI[] switchesGUIText, mainMenuGUIText, settingsGUIText, inputsGUIText;
    Resolution[] resolutions;
    public float reswidth;
    /// <summary>
    /// How long does each button takes to move
    /// </summary>
    public double buttonMovementInSeconds;

    #region Player Configuration
    public double[] timings = {50, 41.6666666, 16.6666666, 25, 3/64};
    public RotationSystems rotationSystems;
    public int nextPieces = 7, endingLevel = 2100;
    public bool[] switches;

    [BurstCompatible]
    public struct int3Array : IJobParallelFor
    {
        public NativeArray<int3> array;
        public int3 setTo;
        public void Execute(int i)
        {
            array[i] = setTo;
        }
    }

    public void TestAmountOfInt3s(int size)
    {
        float time = Time.realtimeSinceStartup;
        NativeArray<int3> int3s = new NativeArray<int3>(size, Allocator.TempJob);
        int3 setTo = new int3(1023,255,511);
        var job = new int3Array()
        {
            array = int3s,
            setTo = setTo
        };
        JobHandle jobHandle = job.Schedule(int3s.Length, 1);
        jobHandle.Complete();
        float afterTime = Time.realtimeSinceStartup;
        Debug.Log(afterTime - time);
        Notify("Time: " + (afterTime - time) + ". Jobified Ints: " + size * 3);
        int3s.Dispose();
    }

    public void ChangeTiming(int index, double timing)
    {
        timings[index] = timing;
    }
    public void ChangeNextPieces(int amount)
    {
        nextPieces = amount;
    }
    public void ChangeSwitch(int index)
    {
        switches[index] = !switches[index];
        string switchName;
        switch (index)
        {
            case 0:
            switchName = "Frozen lines: {0}";
            break;
            default:
            switchName = "No-name switch: {0}";
            break;
        }
        switchesGUIText[index].text = String.Format(switchName, switches[index] ? "ON" : "OFF");
    }
    public void RebindKey(int index)
    {
        modifiableInputAsset.Disable();
        modifiableInputAsset.actionMaps[0].actions[index].PerformInteractiveRebinding()
        .WithControlsExcluding("<Mouse>/*")
        .WithControlsExcluding("*/escape")
        .OnMatchWaitForAnother(1f)
        .OnComplete(callback => {
            Notify("Rebound! Currently it'll reset when the game is closed.", Color.green);
            modifiableInputAsset.Enable();
            callback.Dispose();})
        .Start();
    }
    #endregion

    Language previousLang;
    public GameObject InstantiatePlayer(double LockDelay = 50, double spawnDelay = 41.6666666, double lineSpawnDelay = 16.6666666, double lineDropDelay = 25, float gravity = 3 / 64f, RotationSystems rotationSystem = RotationSystems.SRS, int nextPieces = 7, int endingLevel = 2100)
    {
        GameObject newBoard = Instantiate(inGameBoard, transform);
        newBoard.transform.localPosition += new Vector3(25f, 0f, 0f) * (NetworkBoard.player.Count -1);
        // newBoard.GetComponent<NetworkObject>().Spawn();
        NetworkBoard component = newBoard.GetComponent<NetworkBoard>();
        component.mode = new MarathonMode();
        component.mode.OnObjectSpawn(newBoard.transform);
        component.ruleset = new DefaultSRS();
        component.LockDelay = LockDelay;
        component.spawnDelay = spawnDelay;
        component.spawnTicks = (int)spawnDelay - 300;
        component.lineSpawnDelay = lineSpawnDelay;
        component.lineDropDelay = lineDropDelay;
        component.gravity = gravity;
        component.nextPieces = nextPieces;
        component.RS = rotationSystem;
        component.lineFreezingMechanic = switches[0];
        component.bigMode = switches[1];
        component.oneshot = switches[2];
        component.piecesController.InitiatePieces();
        component.endingLevel = endingLevel;
        return newBoard;
    }
    public void QuitGame()
    {
        if (platformCompat() || Application.isMobilePlatform) 
        {
            quitting = true;
            if(drpcSwitch) SwitchDRPC();
        }
    }
    public void TriggerGameOver()
    {
        GameOver = true;
        starting = false;
    }
    public void PlayGame()
    {
        if (switches[2] && PlayerPrefs.GetInt("oneshot", 0) != 3)
        {
            if (PlayerPrefs.GetInt("oneshot", 0) != 3) 
            {
                audioSource.PlayOneShot(messageboxPopup);
                mainMenuMusic.Pause();
                MessageBoxHandler.MessageBox(IntPtr.Zero, "You've used your only shot.", "Project Challenger", 0x00000010u);
                mainMenuMusic.UnPause();
                return;
            }
            mainMenuMusic.Pause();
            int messageboxOutput = MessageBoxHandler.MessageBox(IntPtr.Zero, "You'll have only one shot, but, you can beat it.", "Project Challenger", 0x00000024u);
            mainMenuMusic.UnPause();
            if (messageboxOutput == 7)
            {
                return;
            }
            switches[2] = true;
            endingLevel = 1000;
        }
        if(!starting && !startGame) { startGame = true; if (PlayerPrefs.GetInt("Oneshot", 0) != 3 || !switches[2]) Notify(LanguageList.LangString[(int)LangArray.notifications][(int)language, 15], Color.white); }
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
        pressedSubMenu equals true
        prevMenu equals menu
        menu equals menuType
        menuSector array MenuType SetActive true
        */

        if (menuType == menu || pressedBack || pressedSubMenu || starting) return;
        pressedSubMenu = true;
        prevMenu = menu;
        menu = menuType;
        menuSectors[menuType].SetActive(true);
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
    public AudioClip[] audioClips;
    void Awake()
    {
        players = new List<GameObject>();
        Application.targetFrameRate = Screen.currentResolution.refreshRate * 4;
        alreadystarted = true;
        instance = this;
        AudioManager.audioDictionary = new Dictionary<string, AudioClip>();
        for (int i = 0; i < audioClips.Length; i++)
        {
            Debug.Log("clipname: " + audioClips[i].name);
            AudioManager.audioDictionary.Add(audioClips[i].name, audioClips[i]);
        }
        TextureUVs.GenerateTextureUVs();
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
        if(!platformCompat()) {reswidth = 1f; menuSectors[1].transform.position += new Vector3(0f,60.00f * (Screen.width / 1920.0f),0f);}
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
            settingsGUIMovement[0].gameObject.SetActive(false);
            settingsGUIPartMovement[0].gameObject.SetActive(false);
            if(!Application.isMobilePlatform)mainMenuGUIMovement[2].gameObject.SetActive(false);
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
    }
    public void ExtractStatsToNotifications(NetworkBoard board)
    {
        if(board.singles > 0) Notify(LanguageList.Extract(LangArray.notifications, language, 0) + board.singles, Color.white); // Singles
        if(board.doubles > 0) Notify(LanguageList.Extract(LangArray.notifications, language, 1) + board.doubles, Color.white); // Doubles
        if(board.triples > 0) Notify(LanguageList.Extract(LangArray.notifications, language, 2) + board.triples, Color.white); // Triples
        if(board.tetrises > 0) Notify(LanguageList.Extract(LangArray.notifications, language, 3) + board.tetrises, Color.white); // Tetrises
        if(board.pentrises > 0) Notify("5 " + LanguageList.Extract(LangArray.notifications, language, 4) + board.pentrises, Color.white); // 5 lines
        if(board.sixtrises > 0) Notify("6 " + LanguageList.Extract(LangArray.notifications, language, 4) + board.sixtrises, Color.white); // 6 lines
        if(board.septrises > 0) Notify("7 " + LanguageList.Extract(LangArray.notifications, language, 4) + board.septrises, Color.white); // 7 lines
        if(board.octrises > 0) Notify("8+ " + LanguageList.Extract(LangArray.notifications, language, 4) + board.octrises, Color.white); // 8+ lines
        if(board.allClears > 0) Notify(LanguageList.Extract(LangArray.notifications, language, 16) + board.allClears, Color.white); // All Clears
        if(board.totalLines > 0) Notify(LanguageList.Extract(LangArray.notifications, language, 5) + LanguageList.Extract(LangArray.notifications, language, 4) + board.totalLines, Color.white); // Total lines
        if(board.piecesController.lockedPieces > 0) Notify(LanguageList.LangString[(int)LangArray.notifications][(int)language, 6] + (board.piecesController.lockedPieces), Color.white); // Pieces
        Notify(LanguageList.LangString[(int)LangArray.notifications][(int)language, 7] + gradeStringConversion[board.grade], Color.white); // Grade
        if(board.statGradePoints > 0) {Notify(LanguageList.LangString[(int)LangArray.notifications][(int)language, 8], Color.white); // Total grade score
        Notify(Math.Floor(board.statGradePoints).ToString(), Color.white);}
        Notify(LanguageList.LangString[(int)LangArray.notifications][(int)language, 9] + board.level + "/" + (board.level < 2100 ? (board.curSect + 1) * 100 : 2100), Color.white); // Level
        Notify(LanguageList.LangString[(int)LangArray.notifications][(int)language, 10] + (board.gravity / 6 * Time.fixedDeltaTime * 1000), Color.white); // Gravity
        Notify(LanguageList.Extract(LangArray.notifications, language, 11) + board.timeCounter.text, Color.white);
    }
    public void Notify(string text, Color color = default)
    {
        NotificationEngine.Notify(text, color);
    }
    public void UpdateLang()
    {
        if(language == Language.日本語)
        {
            Notify("Notice! Japanese translation is not perfect.");
            Notify("通知！ 日本語の翻訳は完璧ではありません。");
        }
        for (int mmguiIndex = 0; mmguiIndex < mainMenuGUIMovement.Length; mmguiIndex++)
        {
            mainMenuGUIText[mmguiIndex].text = LanguageList.Extract(LangArray.mainMenu, language, mmguiIndex);
        }
        for (int sguiIndex = 0; sguiIndex < settingsGUIMovement.Length; sguiIndex++)
        {
            settingsGUIText[sguiIndex].text = LanguageList.Extract(LangArray.settingsMenu, language, sguiIndex);
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
    public string GetGameObjectName(GameObject @object, string nullName = "Null")
    {
        if (@object == null) return nullName;
        else return @object.name;
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
    [SerializeField] double[] frameratebuffer = new double[10];
    bool executedOnce = false;
    bool isMessageBoxActive = false;
    // Updates once every frame.
    void Update()
    {
        if (isMessageBoxActive)
        {
            isMessageBoxActive = !isMessageBoxActive;
            return;
        }
        framerate = 1 / Time.smoothDeltaTime;
        if (previousLang != language)
        {
            previousLang = language;
            UpdateLang();
        }
        if(drpcSwitch) if (platformCompat())
        {
            var activityManager = discord.GetActivityManager();
            int rpclvl = 0;
            if(mainPlayer) rpclvl = mainPlayer.level < mainPlayer.endingLevel ? (mainPlayer.curSect + 1) * 100 : mainPlayer.endingLevel;
            Activity activity;
            if(mainPlayer) activity = mainPlayer.mode.GetDiscordActivity();
            else activity = new Activity
            {
                State = !Application.genuineCheckAvailable ? "The game's integrity couldn't confirmed." : framerate < 20 ? "Performance issues"
                : curBoard != null ? "Currently playing" : quitting ? "Quitting" : menu == 1 ? "Currently in settings menu" : "Currently in main menu",
                Details = "Highlighted: " + GetGameObjectName(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject),
                Assets = {
                    LargeImage = "icon"
                }
            };

            activityManager.UpdateActivity(activity, (res) => {
            });
            if(drpcSwitch)discord.RunCallbacks();
        }

        if(curBoard == null && Input.GetKeyDown(KeyCode.Escape))
        {
            if (menu > 0)
            {
                // Back();
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
        if (quitting || startGame)
        {
            if(quitting && segments[0].MoveCoupleUIElements(false))
            {
                Application.Quit();
            }
            else if(!segments[0].MoveCoupleUIElements(false)) return;
            else if(PlayerPrefs.GetInt("Oneshot", 0) == 3 && switches[2])
            {
                // audioSource.PlayOneShot(messageboxPopup);
                mainMenuMusic.Pause();
                MessageBoxHandler.MessageBox(new IntPtr(0), "You've used your only shot.", "Project Challenger", 0x00000010u);
                mainMenuMusic.UnPause();
                isMessageBoxActive = true;
                startGame = false;
                starting = true;
            }
            else if (startGame && UITimeDeltas[0] <= 0)
            {
                UITimeDeltas[0] -= Time.deltaTime;
                if (!executedOnce)
                {
                    Unity.Netcode.NetworkManager.Singleton.StartHost();
                    UnityEngine.Random.InitState(SeedManager.NewSeed());
                    ReplayRecord.instance.Reset();
                    ReplayRecord.seed = SeedManager.seed;
                    
                    if (ReplayRecord.instance.mode == ReplayModeType.read)
                    {
                        ReplayRecord.instance.LoadReplay("1");
                        UnityEngine.Random.InitState(ReplayRecord.seed);
                    }
                    menuSectors[0].SetActive(false);
                    executedOnce = true;

                    curBoard = InstantiatePlayer(timings[0], timings[1], timings[2], timings[3], (float)timings[4], rotationSystems, nextPieces, endingLevel);
                    mainMenuMusic.Stop();
                }
                if (UITimeDeltas[0] < -0.17)
                {
                    startGame = false;
                    UITimeDeltas[0] = 0d;
                    NetworkBoard netBoard = curBoard.GetComponent<NetworkBoard>();
                    netBoard.paused = false;
                    executedOnce = false;
                }
            }
        }
        if(pressedSubMenu)
        {
            if(segments[prevMenu].MoveCoupleUIElements(false))
            if(segments[menu].MoveCoupleUIElements(true))
            pressedSubMenu = false;
        }
        if (starting)
        {
            if (!mainMenuMusic.isPlaying)mainMenuMusic.Play();
            menuSectors[0].SetActive(true);
           
            //Movement to the right
            if(segments[0].MoveCoupleUIElements(true, 1f, 0.5)) starting = false;
        }
    }
}
