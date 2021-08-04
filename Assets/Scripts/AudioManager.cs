using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioManager
{
    
    /// <param name="clip">An audio to play upon</param>
    public static void PlayClip(AudioClip clip)
    {
        GameEngine.instance.gameAudio.PlayOneShot(clip);
    }
}
