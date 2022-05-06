using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioManager
{
    public static Dictionary<string,AudioClip> audioDictionary;

    /// <param name="clip">An audio clip to play upon</param>
    public static void PlayClip(AudioClip clip, float volume = 1f)
    {
        GameEngine.instance.gameAudio.PlayOneShot(clip, volume);
    }
    /// <param name="clipName">An audio to play upon</param>
    public static void PlayClip(string clipName, float volume = 1f)
    {
        GameEngine.instance.gameAudio.PlayOneShot(audioDictionary[clipName], volume);
    }
    public static void ChangeBGMVolume(float volume)
    {
        GameEngine.instance.gameMusic.volume = volume;
    }
}
