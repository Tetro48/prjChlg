using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using TMPro;

public class NetworkBoard : NetworkBehaviour
{
    public int time, rollTime, rollTimeLimit = 11000, notifDelay, sectionlasttime, coolprevtime;

    public int[] sectionTime = {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};

    public SectionState[] cools = new SectionState[21];

    /// <summary>
    /// Section COOL criteria Time 
    /// </summary>
	public static int[] tableTimeCool =
	{
		5200, 5200, 4900, 4500, 4500, 4200, 4200, 3800, 3800, 3800, 3300, 3300, 3300, 2800, 2800, 2200, 1800, 1400, 900, 600, -1
	};

    /// <summary>
    /// Section REGRET criteria Time 
    /// </summary>
    public static int[] tableTimeRegret = 
    {
        9000, 7500, 7500, 6800, 6000, 6000, 5000, 5000, 500, 5000, 4500, 4500, 4500, 4000, 4000, 3400, 3000, 2600, 1700, 800, -1
    };

    public int level;

    public int endingLevel = 2100;

    public int curSect, sectAfter20g;

    // Number of combos | Keeping combo
    public int comboCount, comboKeepCounter;

    public AudioClip[] comboSE;

    private double musicTime;

    public AudioSource gameAudio, gameMusic;

    public AudioClip[] bgm_1p_lv;

    public string audioPath;

    public AudioClip readySE, goSE, gradeUp, excellent, coolSE, regretSE;

    public int bgmlv = 1;

    public double gradePoints, statGradePoints, gradePointRequirement;

    private int virtualBasePoint;

    public TextMeshPro levelTextRender, nextSecLv, timeCounter, rollTimeCounter, ppsCounter;

    public SpriteRenderer readyGoIndicator, gradeIndicator;

    public Sprite[] gradeSprites;

    public int grade;

    public Sprite readySprite, goSprite;

    public int nextPieces, nextibmblocks;

    public RotationSystems RS;

    public bool TLS, tSpin, ending, coolchecked, previouscool;

    public bool lineFreezingMechanic;

    [Range(0, 1000)]
    public double LockDelay = 50;

    [Range(0, 1000)]
    public double DAS = 15;

    [Range(0, 1000)]
    public double SDF = 6;

    [Range(0, 1000)]
    public double ARE = 41.66666666666666;

    public int AREf = 42 - 300;

    [Range(0, 1000)]
    public double AREline = 16.66666666666666666;

    public int lineDelayf = 0;

    [Range(0, 1000)]
    public double lineDelay = 25;

    [Range(0, 60)]
    public float gravity = 3/64f;

    public int singles, doubles, triples, tetrises, pentrises, sixtrises, septrises, octrises;

    public int totalLines;

    public int[] lineClonePerPiece = {2147483647,2147483647,20,20,20,20,20,20,20,20,16,16,16,8,8,6,5,4,3,2,2,2};

    public int lineClonePiecesLeft = 20;

    public float percentage = 0.8f;

    public bool paused, FrameStep, framestepped;

    public bool[] Inputs;

    public int tileInvisTime = -1;

    public Vector2 movement;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
