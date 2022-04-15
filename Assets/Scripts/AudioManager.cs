using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioManager
{
    public static Dictionary<string,AudioClip> audioDictionary;

    /// <param name="clip">An audio to play upon</param>
    public static void PlayClip(AudioClip clip)
    {
        GameEngine.instance.gameAudio.PlayOneShot(clip);
    }
    /// <param name="clipName">An audio to play upon</param>
    public static void PlayClip(string clipName)
    {
        GameEngine.instance.gameAudio.PlayOneShot(audioDictionary[clipName]);
    }
}
