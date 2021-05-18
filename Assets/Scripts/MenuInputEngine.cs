using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Event/Event System")]
[DisallowMultipleComponent]
public class MenuInputEngine : MonoBehaviour
{
    public static MenuInputEngine MIE;
    
    public KeyCode Up {get; set;}
    public KeyCode Down {get; set;}
    public KeyCode Left {get; set;}
    public KeyCode Right {get; set;}
    public KeyCode CCW {get; set;}
    public KeyCode CW {get; set;}
    public KeyCode Rot180 {get; set;}
    public KeyCode Pause {get; set;}
    public KeyCode FrameStep {get; set;}
    public KeyCode ScrSh {get; set;}
    void Awake()
    {
        if(MIE == null)
        {
            DontDestroyOnLoad(gameObject);
            MIE = this;
        }
        else if (MIE != this)
        {
            Destroy(gameObject);
        }
        Up = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("upKey", "Space"));
        Down = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("downKey", "Down"));
        Left = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("leftKey", "Left"));
        Right = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("rightKey", "Right"));
        CCW = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("CCWKey", "Z"));
        CW = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("CWKey", "X"));
        Rot180 = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("rot180Key", "A"));
        Pause = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("pauseKey", "ESC"));
        FrameStep = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("frameStepKey", "N"));
        ScrSh = (KeyCode) System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("scrShKey", "F2"));
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
