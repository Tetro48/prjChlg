using Discord;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

/*
    Project Challenger, a challenging block stacking game.
    Copyright (C) 2022-2023, Aymir

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

public enum Language { English, Русский, 日本語 };

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

    public string[] gradeStringConversion = { "9", "8", "7", "6", "5", "4", "3", "2", "1", "S1", "S2", "S3", "S4", "S5", "S6", "S7", "S8", "S9", "GM", "FPGM" };

    public TextMeshProUGUI framerateCounter;

    public TMP_Dropdown resDropdown;

    [SerializeField] private double[] UITimeDeltas;

    [SerializeField]
    private GameObject[] menuSectors;
    public AudioSource audioSource, audioSource2, mainMenuMusic, audioSourceConfirmation;
    public AudioClip clip, topoutSE;
    public GameObject inGameBoard, curBoard;
    public AudioClip ModeOK, messageboxPopup;
    public GameObject imgprjchlg, mobileInput;
    public RectTransform[] mainMenuGUIMovement, settingsGUIMovement, settingsGUIPartMovement;
    public MenuSegment[] segments;
    public bool isGamePaused;
    private bool pauseMenuConditional;
    private int pauseMenuTimeBlock;
    private bool gameRestart;
    public MenuSegment pauseMenuSegment;

    public TextMeshProUGUI[] switchesGUIText, mainMenuGUIText, settingsGUIText, inputsGUIText, inputMovementGUIText;
    public TextMeshProUGUI[] timingsGUIText, miscGUIText;
    public TMP_InputField[] timingsInputFields, miscInputFields;
    private Resolution[] resolutions;
    public float reswidth;
    /// <summary>
    /// How long does each button takes to move
    /// </summary>
    public double buttonMovementInSeconds;
    public double menuSegmentDelay = 0.5d;
    public double menuSegmentTime = 0d;

    #region Player Configuration
    public double[] timings = { 50, 41.6666666, 16.6666666, 25, 3 / 64 };
    public double percentage;
    public RotationSystems rotationSystem;
    public int nextPieces = 7, sectionSize = 100, endingLevel = 2100;
    public bool[] switches;
    private int configIndex;
    
    public void ResetCMConfigs()
    {
        double divisionFactor = 1d / Time.fixedDeltaTime;
        timings = new[] { 50, 41.6666666, 16.6666666, 25, 3d / 64d };
        for (int i = 0; i < timings.Length - 1; i++)
        {
            timingsGUIText[i].text = SIUnitsConversion.doubleToSITime(timings[i] / divisionFactor);
            timingsInputFields[i].text = string.Empty;
        }
        timingsGUIText[4].text = string.Format("{0:0.####}/sec", timings[4] / divisionFactor * 10000);
        percentage = 0.8;
        miscGUIText[0].text = "80%";
    }
    public void SetConfigIndex(int newIndex) => configIndex = newIndex;
    public void ChangeTiming(string strValue)
    {
        if (strValue == string.Empty)
        {
            return;
        }
        double divisionFactor = 1d / Time.fixedDeltaTime;
        if (strValue.Length >= 3 && strValue[..3].ToLowerInvariant() == "inf")
        timings[configIndex] = double.PositiveInfinity;
        else
        timings[configIndex] = double.Parse(strValue);
        if (configIndex == 4)
        {
            timings[4] = timings[4] / divisionFactor;
        }
        timingsGUIText[configIndex].text = SIUnitsConversion.doubleToSITime(timings[configIndex] / divisionFactor);
        timingsInputFields[configIndex].text = string.Empty;
        if (configIndex == 4)
        {
            timingsGUIText[4].text = string.Format("{0:0.####}/sec", timings[4] / divisionFactor * 10000);
        }
    }
    public void ChangeMiscValue(string strValue)
    {
        if (strValue == string.Empty)
        {
            return;
        }
        double value;
        if (!double.TryParse(strValue, out value))
        {
            miscGUIText[configIndex].text = "INVALID VALUE";
        }
        value = math.max(value, 0);
        miscInputFields[configIndex].text = string.Empty;
        switch (configIndex)
        {
            case 0:
                percentage = math.min(value / 100, 0.99);
                miscGUIText[0].text = percentage * 100 + "%";
                break;
            case 1:
                nextPieces = math.min((int)value, 100);
                miscGUIText[1].text = nextPieces.ToString();
                break;
            case 2:
                endingLevel = (int)value;
                miscGUIText[2].text = endingLevel.ToString();
                break;
            case 3:
                endingLevel /= sectionSize;
                sectionSize = (int)value;
                endingLevel *= sectionSize;
                miscGUIText[2].text = endingLevel.ToString();
                miscGUIText[3].text = sectionSize.ToString();
                break;
            default:
                break;
        }
    }
    public void ChangeRotationSystem(int id)
    {
        rotationSystem = (RotationSystems) id;
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
            case 1:
                switchName = "Big Mode: {0}";
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
        // .OnMatchWaitForAnother(1f)
        .OnComplete(callback =>
        {
            Notify("Rebound!", Color.green);
            inputsGUIText[index-1].text = callback.action.name + ": " + InputControlPath.ToHumanReadableString(
            callback.action.bindings[0].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
            modifiableInputAsset.Enable();
            callback.Dispose();
        })
        .Start();
    }
    public void RebindMovement(int compositeIndex)
    {
        modifiableInputAsset.Disable();
        // just a note: "actions[0]" assumes action "Movement".
        modifiableInputAsset.actionMaps[0].actions[0].PerformInteractiveRebinding()
        .WithTargetBinding(compositeIndex+1)
        .WithControlsExcluding("<Mouse>/*")
        .WithControlsExcluding("*/escape")
        // .OnMatchWaitForAnother(1f)
        .OnComplete(callback =>
        {
            Notify("Rebound!", Color.green);
            inputMovementGUIText[compositeIndex].text = callback.action.bindings[compositeIndex+1].name + ": " + InputControlPath.ToHumanReadableString(
            callback.action.bindings[compositeIndex+1].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
            modifiableInputAsset.Enable();
            callback.Dispose();
        })
        .Start();
    }
    public void LoadBindings()
    {
        string bindingJSON = PlayerPrefs.GetString("InputBindings", string.Empty);
        if (bindingJSON == string.Empty)
        {
            return;
        }
        modifiableInputAsset.LoadBindingOverridesFromJson(bindingJSON);
        for (int i = 0; i < inputsGUIText.Length; i++)
        {
            inputsGUIText[i].text = modifiableInputAsset.actionMaps[0].actions[i+1].name + ": " + InputControlPath.ToHumanReadableString(
            modifiableInputAsset.actionMaps[0].actions[i+1].bindings[0].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
        for (int i = 0; i < inputMovementGUIText.Length; i++)
        {
            inputMovementGUIText[i].text = modifiableInputAsset.actionMaps[0].actions[0].bindings[i+1].name + ": " + InputControlPath.ToHumanReadableString(
            modifiableInputAsset.actionMaps[0].actions[0].bindings[i+1].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);
        }
    }
    public void SaveBindings()
    {
        string bindingJSON = modifiableInputAsset.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("InputBindings", bindingJSON);
    }
    #endregion

    private Language previousLang;
    public GameObject InstantiatePlayer(
        int nextPieces = 7)
    {
        GameObject newBoard = Instantiate(inGameBoard, transform);
        newBoard.transform.localPosition += new Vector3(25f, 0f, 0f) * (NetworkBoard.player.Count - 1);
        NetworkBoard component = newBoard.GetComponent<NetworkBoard>();
        component.LockDelay = timings[0];
        component.spawnDelay = timings[1];
        component.lineDropDelay = timings[2];
        component.lineSpawnDelay = timings[3];
        component.gravity = timings[4];
        component.RS = rotationSystem;
        component.lineFreezingMechanic = switches[0];
        component.bigMode = switches[1];
        component.percentage = percentage;
        component.nextPieces = nextPieces;
        component.sectionSize = sectionSize;
        component.endingLevel = endingLevel;
        component.piecesController.InitiatePieces();
        return newBoard;
    }
    public void QuitGame()
    {
        if (platformCompat() || Application.isMobilePlatform)
        {
            quitting = true;
            if (drpcSwitch)
            {
                SwitchDRPC();
            }
        }
    }
    public void TriggerGameOver()
    {
        GameOver = true;
        starting = false;
    }
    public void PlayGame()
    {
        if (!starting && !startGame)
        {
            startGame = true;
            Notify(LanguageList.LangString[(int)LangArray.notifications][(int)language, 15], Color.white);
        }
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

        if (menuType == menu || pressedBack || pressedSubMenu || starting)
        {
            return;
        }

        pressedSubMenu = true;
        prevMenu = menu;
        menu = menuType;
        menuSectors[menuType].SetActive(true);
    }

    private bool platformLogged = false;
    public bool platformCompat()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.LinuxEditor:
                platformLogged = true;
                return true;
        }
        if (!platformLogged)
        {
            Debug.Log("Other platform");
            Debug.Log("Some functions are disabled for compatibility.");
        }
        platformLogged = true;
        return false;
    }
    public bool alreadystarted;
    public AudioClip[] audioClips;

    private void Awake()
    {
        for (int i = 0; i < modifiableInputAsset.actionMaps[0].actions.Count; i++)
        {
            Debug.Log(modifiableInputAsset.actionMaps[0].actions[i].name);
        }
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
        {
            discord.Dispose();
        }
        else
        {
            discord = new Discord.Discord(836600860976349192, (UInt64)Discord.CreateFlags.NoRequireDiscord);
        }
    }
    private int resRefreshrates = 0;

    private void Start()
    {
        if (!platformCompat())
        {
            reswidth = 1f;
            menuSectors[1].transform.position += new Vector3(0f, 60.00f * (Screen.width / 1920.0f), 0f);
        }
        LoadBindings();
        imgprjchlg.SetActive(true);
        if (platformCompat())
        {
            resolutions = Screen.resolutions;
            resDropdown.ClearOptions();
            List<string> options = new List<string>();

            int currentResolutionIndex = 1;
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = $"{resolutions[i].width}x{resolutions[i].height}@{resolutions[i].refreshRate}";
                options.Add(option);
                if (resolutions[i].width == Screen.currentResolution.width &&
                    resolutions[i].height == Screen.currentResolution.height &&
                    resolutions[i].refreshRate == Screen.currentResolution.refreshRate)
                {
                    currentResolutionIndex = i;
                    reswidth = resolutions[i].width / 1920;
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
            if (!Application.isMobilePlatform)
            {
                mainMenuGUIMovement[2].gameObject.SetActive(false);
            }
        }
        double divisionFactor = 1d / Time.fixedDeltaTime;
        for (int i = 0; i < timingsInputFields.Length; i++)
        {
            timingsGUIText[i].text = SIUnitsConversion.doubleToSITime(timings[i] / divisionFactor);
            timingsInputFields[i].text = string.Empty;
        }
        for (int i = 0; i < miscInputFields.Length; i++)
        {
            miscInputFields[i].text = string.Empty;
        }
        miscGUIText[0].text = percentage * 100 + "%";
        // miscGUIText[1].text = nextPieces.ToString();
        // miscGUIText[2].text = endingLevel.ToString();
        // miscGUIText[3].text = sectionSize.ToString();
        timingsGUIText[4].text = string.Format("{0:0.####}/sec", timings[4] / divisionFactor * 10000);
        starting = true;
    }
    public void PlayerResume()
    {
        isGamePaused = false;
    }
    public void PlayerExitInvoke(bool restart = false)
    {
        isGamePaused = false;
        mainPlayer.OnGameQuit();
        gameRestart = restart;
    }
    public void ExtractStatsToNotifications(NetworkBoard board)
    {
        for (int i = 0; i < 4; i++)
        {
            if (board.clearedLinesArray[i] > 0)
            Notify(LanguageList.Extract(LangArray.notifications, language, i) + board.clearedLinesArray[i], Color.white);
        }

        for (int i = 4; i < board.clearedLinesArray.Length; i++)
        {
            if (board.clearedLinesArray[i] > 0)
            Notify(i + " " + LanguageList.Extract(LangArray.notifications, language, 4) + board.clearedLinesArray[i], Color.white);
        }

        if (board.clearedLinesArray[^1] > 0)
        {
            Notify("8+ " + LanguageList.Extract(LangArray.notifications, language, 4) + board.clearedLinesArray[7], Color.white); // 8+ lines
        }

        if (board.allClears > 0)
        {
            Notify(LanguageList.Extract(LangArray.notifications, language, 16) + board.allClears, Color.white); // All Clears
        }

        if (board.totalLines > 0)
        {
            Notify(LanguageList.Extract(LangArray.notifications, language, 5) + LanguageList.Extract(LangArray.notifications, language, 4) + board.totalLines, Color.white); // Total lines
        }

        if (board.piecesController.lockedPieces > 0)
        {
            Notify(LanguageList.LangString[(int)LangArray.notifications][(int)language, 6] + (board.piecesController.lockedPieces), Color.white); // Pieces
        }

        Notify(LanguageList.LangString[(int)LangArray.notifications][(int)language, 7] + gradeStringConversion[board.grade], Color.white); // Grade
        if (board.statGradePoints > 0)
        {
            Notify(LanguageList.LangString[(int)LangArray.notifications][(int)language, 8], Color.white); // Total grade score
            Notify(Math.Floor(board.statGradePoints).ToString(), Color.white);
        }
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
        if (language == Language.日本語)
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
    public void SetResolution(int index)
    {
        if (platformCompat())
        {
            Resolution resolution = resolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, true);
            reswidth = (float)(resolution.width / 1920.0);
        }
    }
    public string GetGameObjectName(GameObject @object, string nullName = "Null")
    {
        if (@object == null)
        {
            return nullName;
        }
        else
        {
            return @object.name;
        }
    }

    // void OnDestroy()
    // {
    //     discord.Dispose();
    // }
    private void OnApplicationQuit()
    {
        SaveBindings();
        if (platformCompat() && drpcSwitch)
        {
            discord.Dispose();
        }
    }

    private double framerate;
    private bool executedOnce = false;
    private bool isMessageBoxActive = false;

    // Updates once every frame.
    private void Update()
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
        if (drpcSwitch)
        {
            if (platformCompat())
            {
                var activityManager = discord.GetActivityManager();
                int rpclvl = 0;
                if (mainPlayer)
                {
                    rpclvl = mainPlayer.level < mainPlayer.endingLevel ? (mainPlayer.curSect + 1) * 100 : mainPlayer.endingLevel;
                }

                Activity activity;
                if(mainPlayer != null)
                activity = new Activity
                {
                    Details = mainPlayer.ending ? "Roll time left: " + mainPlayer.rollTimeCounter.text 
                    : curBoard != null ? "Level " + mainPlayer.level + " | " + rpclvl + (mainPlayer.level > 800 ? ". Struggling." : string.Empty)
                    : null,

                    State = !Application.genuineCheckAvailable ? "The game's integrity couldn't be confirmed."
                    : mainPlayer.lives > 1 && mainPlayer.GameOver ? "Lost a life."
                    : mainPlayer.rollTime >= mainPlayer.rollTimeLimit ? String.Format("Successful at level {0}", mainPlayer.endingLevel)
                    : mainPlayer.IntentionalGameOver ? "Exiting..."
                    : mainPlayer.GameOver ? "Topped out"
                    : curBoard != null && mainPlayer.paused && !mainPlayer.FrameStep ? "Paused" 
                    : curBoard != null && ReplayRecord.instance.mode == ReplayModeType.read ? "Currently replaying" 
                    : curBoard != null && mainPlayer.paused && mainPlayer.FrameStep ? "Currently playing (Framestepping)" 
                    : curBoard != null ? "Currently playing"
                    : null,
                    Assets = {
                        LargeImage = "icon"
                    }
                };
                else
                {
                    activity = new Activity
                    {
                        State = !Application.genuineCheckAvailable ? "The game's integrity couldn't be confirmed."
                        : framerate < 20 ? "Performance issues"
                        : curBoard != null ? "Currently playing"
                        : quitting ? "Quitting"
                        : menu == 1 ? "Currently in settings menu"
                        : "Currently in main menu",
                        Details = "Highlighted: " + GetGameObjectName(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject),
                        Assets = {
                            LargeImage = "icon"
                        }
                    };
                }

                activityManager.UpdateActivity(activity, (res) =>
                {
                });
                if (drpcSwitch)
                {
                    discord.RunCallbacks();
                }
            }
        }

        if (curBoard == null && Input.GetKeyDown(KeyCode.Escape))
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
        framerateCounter.text = string.Format("{0:0.00} FPS", framerate);
        if (isGamePaused || pauseMenuConditional)
        {
            pauseMenuSegment.gameObject.SetActive(true);
            if (pauseMenuTimeBlock < 3)
            {
                bool pauseMenuSegmentOut = pauseMenuSegment.MoveCoupleUIElements(true, 0.5);
                pauseMenuConditional = !pauseMenuSegmentOut;
                if (pauseMenuSegmentOut)
                {
                    pauseMenuTimeBlock++;
                }
            }
            GameEngine.instance.gameMusic.Pause();
            mainPlayer.paused = true;
        }
        else if (mainPlayer != null && pauseMenuSegment.gameObject.activeSelf)
        {
            if (pauseMenuSegment.MoveCoupleUIElements(false))
            {
                GameEngine.instance.gameMusic.UnPause();
                mainPlayer.paused = false;
                pauseMenuTimeBlock = 0;
            }
        }
        if (quitting || startGame)
        {
            if (quitting && segments[0].MoveCoupleUIElements(false))
            {
                Application.Quit();
            }
            else if (!segments[0].MoveCoupleUIElements(false))
            {
                return;
            }
            else if (PlayerPrefs.GetInt("Oneshot", 0) == 3 && switches[2])
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

                    curBoard = InstantiatePlayer();
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
        if (pressedSubMenu)
        {
            if (segments[prevMenu].MoveCoupleUIElements(false))
            {
                if (menuSegmentTime < menuSegmentDelay)
                {
                    menuSegmentTime += Time.deltaTime;
                }
                else if (segments[menu].MoveCoupleUIElements(true))
                {
                    menuSegmentTime = 0;
                    pressedSubMenu = false;
                }
            }
        }
        if (starting)
        {
            if (gameRestart)
            {
                gameRestart = false;
                startGame = true;
                starting = false;
                return;
            }
            if (!mainMenuMusic.isPlaying)
            {
                mainMenuMusic.Play();
            }

            menuSectors[0].SetActive(true);

            //Movement to the right
            if (segments[0].MoveCoupleUIElements(true, 0.5))
            {
                starting = false;
            }
        }
    }
}
