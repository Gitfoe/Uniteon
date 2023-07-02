using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Fields
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private AudioSource sfxPlayer1;
    [SerializeField] private AudioSource sfxPlayer2;
    [SerializeField] private List<UniteonAudioClip> sfxClips;
    [SerializeField] private List<UniteonAudioClip> musicClips;
    private Coroutine _playMusicRoutine;

    // Properties
    public static AudioManager Instance { get; private set; }
    public static Dictionary<string, AudioClip> Sfx { get; private set; }
    public static Dictionary<string, AudioClip> Music { get; private set; }
    public static List<AudioClip> PlayingMusic { get; private set; }
    public static List<AudioClip> PlayingWorldMusic { get; set; }

    /// <summary>
    /// Sets this instance of the audio manager to a public static variable, as well as the audio clips.
    /// </summary>
    private void Awake()
    {
        Instance = this;
        Sfx = UniteonAudioClip.ConvertListToDictionary(sfxClips);
        Music = UniteonAudioClip.ConvertListToDictionary(musicClips);
    }

    #region Music
    /// <summary>
    /// Plays music on the music channel.
    /// </summary>
    /// <param name="clip">The music clip that must be played.</param>
    /// <param name="loop">If the clip must be looped or not.</param>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null)
            return;
        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();
        PlayingMusic = new List<AudioClip>() { clip };
    }
    
    public void PlayMusic(string clip, bool loop = true)
    {
        PlayMusic(Music[clip], loop);
    }

    /// <summary>
    /// Plays music that has a beginning section and a looping section on the music channel.
    /// </summary>
    /// <param name="beginClip">The beginning section of a looping music clip, played once.</param>
    /// <param name="loopClip">The looping section of a music clip.</param>
    public void PlayMusic(AudioClip beginClip, AudioClip loopClip)
    {
        if (beginClip == null)
            PlayMusic(loopClip);
        else
        {
            _playMusicRoutine = StartCoroutine(PlayMusicRoutine(beginClip, loopClip));
            PlayingMusic = new List<AudioClip>() { beginClip, loopClip };
        }
    }

    public void PlayMusic(string beginClip, string endClip)
    {
        PlayMusic(Music[beginClip], Music[endClip]);
    }

    public void PlayMusic(List<AudioClip> clips)
    {
        switch (clips.Count)
        {
            case 1:
                PlayMusic(clips[0]);
                break;
            case 2:
                PlayMusic(clips[0], clips[1]);
                break;
        }
    }

    /// <summary>
    /// Plays the beginning clip once and then indefinitely loops the looping clip.
    /// </summary>
    /// <param name="beginClip">The beginning section of a looping music clip, played once.</param>
    /// <param name="loopClip">The looping section of a music clip.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator PlayMusicRoutine(AudioClip beginClip, AudioClip loopClip)
    {
        musicPlayer.clip = beginClip;
        musicPlayer.loop = false;
        musicPlayer.Play();
        yield return new WaitForSeconds(beginClip.length);
        musicPlayer.clip = loopClip;
        musicPlayer.loop = true;
        musicPlayer.Play();
    }
    
    /// <summary>
    /// Stops any active music from playing.
    /// </summary>
    /// <param name="fade">Whether to fade out the music.</param>
    /// <param name="fadeDuration">The duration of the fade-out effect.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator StopMusic(bool fade = false, float fadeDuration = 0f)
    {
        // Fade out the music if fade is enabled and fadeDuration is greater than 0
        if (musicPlayer.isPlaying && fade && fadeDuration > 0f)
        {
            float startVolume = musicPlayer.volume;
            yield return musicPlayer.DOFade(0f, fadeDuration).WaitForCompletion();
            musicPlayer.Stop();
            musicPlayer.volume = startVolume;
            PlayingMusic = null;
        }
        // Stop the music immediately if any other than this music is playing
        else if (musicPlayer.isPlaying)
        {
            musicPlayer.Stop();
            PlayingMusic = null;
        }
        // Stop any active music coroutine
        if (_playMusicRoutine != null)
        {
            StopCoroutine(_playMusicRoutine); 
        }
    }

    /// <summary>
    /// Fades out playing music and then plays new music.
    /// </summary>
    /// <param name="clip">The music clip that must be played.</param>
    /// <param name="fadeDuration">The duration of the fade-out effect.</param>
    /// <param name="loop">If the clip must be looped or not.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator FadeOutMusicAndPlayNewMusic(AudioClip clip, float fadeDuration, bool loop = true)
    {
        yield return StopMusic(true, fadeDuration);
        PlayMusic(clip, loop);
    }

    public IEnumerator FadeOutMusicAndPlayNewMusic(string clip, float fadeDuration, bool loop = true)
    {
        yield return FadeOutMusicAndPlayNewMusic(Music[clip], fadeDuration, loop);
    }
    
    /// <summary>
    /// Fades out playing music and then plays new music.
    /// </summary>
    /// <param name="beginClip">The beginning section of a looping music clip, played once.</param>
    /// <param name="loopClip">The looping section of a music clip.</param>
    /// <param name="fadeDuration">The duration of the fade-out effect.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator FadeOutMusicAndPlayNewMusic(AudioClip beginClip, AudioClip loopClip, float fadeDuration)
    {
        yield return StopMusic(true, fadeDuration);
        PlayMusic(beginClip, loopClip);
    }

    public IEnumerator FadeOutMusicAndPlayNewMusic(string beginClip, string loopClip, float fadeDuration)
    {
        yield return FadeOutMusicAndPlayNewMusic(Music[beginClip], Music[loopClip], fadeDuration);
    }
    #endregion

    #region Sfx
    /// <summary>
    /// Plays a sound effect.
    /// </summary>
    /// <param name="clip">The sound effect clip.</param>
    /// <param name="loop">If the sound effect needs to be looped.</param>
    /// <param name="channel">The SFX player channel the clip must be played on.</param>
    /// <param name="panning">The panning value for audio balance (-1 for left to 1 for right).</param>
    /// <param name="pitch">The pitch value for playback speed (1 = normal speed).</param>
    public void PlaySfx(AudioClip clip, bool loop = false, int channel = 1, float panning = 0f, float pitch = 1f)
    {
        if (clip == null)
            return;
        AudioSource sfxPlayerChannel = SelectSfxPlayerChannel(channel);
        sfxPlayerChannel.clip = clip;
        sfxPlayerChannel.loop = loop;
        sfxPlayerChannel.panStereo = panning;
        sfxPlayerChannel.pitch = pitch;
        sfxPlayerChannel.Play();
    }
    
    public void PlaySfx(string clip, bool loop = false, int channel = 1, float panning = 0f, float pitch = 1f)
    {
        PlaySfx(Sfx[clip], loop, channel, panning, pitch);
    }

    /// <summary>
    /// Stops the playing sound effect on a specific audio channel.
    /// </summary>
    /// <param name="channel">The SFX player channel the audio must be stopped on.</param>
    /// <param name="clip">Only stop if this audio clip is playing.</param>
    public void StopSfx(int channel, AudioClip clip = null)
    {
        AudioSource sfxPlayerChannel = SelectSfxPlayerChannel(channel);
        if (clip != null && sfxPlayerChannel.isPlaying && sfxPlayerChannel.clip == clip)
            sfxPlayerChannel.Stop();
        else if (clip == null && sfxPlayerChannel.isPlaying)
            sfxPlayerChannel.Stop();
    }

    public void StopSfx(int channel, string clip)
    {
        StopSfx(channel, Sfx[clip]);
    }

    /// <summary>
    /// Checks if an sfx player is currently playing an audio clip.
    /// </summary>
    /// <param name="clip">The clip you want to check.</param>
    /// <param name="channel">The audio channel you want to check the clip on.</param>
    /// <returns>True if playing and false if not.</returns>
    public bool IsPlayingSfx(AudioClip clip, int channel = 1)
    {
        AudioSource sfxPlayerChannel = SelectSfxPlayerChannel(channel);
        return sfxPlayerChannel.clip == clip;
    }

    public bool IsPlayingSfx(string clip, int channel = 1)
    {
        return IsPlayingSfx(Sfx[clip], channel);
    }

    /// <summary>
    /// Selects the correct AudioSource for the sfx player.
    /// </summary>
    /// <param name="channel">The sfx channel you want.</param>
    /// <returns>Your selected sfx channel.</returns>
    private AudioSource SelectSfxPlayerChannel(int channel)
    {
        AudioSource sfxPlayerChannel = channel switch
        {
            1 => sfxPlayer1,
            2 => sfxPlayer2,
            _ => default
        };
        return sfxPlayerChannel;
    }
    #endregion
    
    #region Volume
    /// <summary>
    /// Mutes the music in a fading motion, waits for a bit, and then restores the volume.
    /// </summary>
    /// <param name="fadeTime">The fading duration.</param>
    /// <param name="silenceTime">The time you want the audio to be silent.</param>
    /// <returns></returns>
    public IEnumerator FadeMuteMusicVolume(float fadeTime, float silenceTime)
    {
        if (musicPlayer.isPlaying)
        {
            float startVolume = musicPlayer.volume;
            yield return musicPlayer.DOFade(0f, fadeTime).WaitForCompletion();
            yield return new WaitForSeconds(silenceTime);
            yield return musicPlayer.DOFade(startVolume, fadeTime).WaitForCompletion();
        }
    }
    #endregion
    
    #region Set Properties
    /// <summary>
    /// Sets the PlayingWorldMusic property. No values entered means that the property will be set to null.
    /// </summary>
    /// <param name="clip1">The intro clip, or if there is no intro clip, the looping clip..</param>
    /// <param name="clip2">The looping clip.</param>
    public void SetPlayingWorldMusic(AudioClip clip1 = null, AudioClip clip2 = null)
    {
        if (!ReferenceEquals(clip1, null) && ReferenceEquals(clip2, null))
            PlayingWorldMusic = new List<AudioClip>() { clip1 };
        else if (!ReferenceEquals(clip1, null))
            PlayingWorldMusic = new List<AudioClip>() { clip1, clip2 };
        else if (ReferenceEquals(clip2, null))
            PlayingWorldMusic = null;
    }

    public void SetPlayingWorldMusic(string clip1, string clip2 = null)
    {
        if (ReferenceEquals(clip2, null))
            SetPlayingWorldMusic(Music[clip1]);
        else
            SetPlayingWorldMusic(Music[clip1], Music[clip2]);
    }
    #endregion
}

/// <summary>
/// Holds an audio clip and their accompanying name.
/// </summary>
[Serializable]
public struct UniteonAudioClip
{
    [SerializeField] private string clipName;
    [SerializeField] private AudioClip clip;

    public string ClipName => clipName;
    public AudioClip Clip => clip;

    /// <summary>
    /// Converts a list of UniteonSfx structs to a dictionary, since they're functionally the same and easier to manage.
    /// </summary>
    /// <param name="uniteonSfxList">The sfx list full of UniteonSfx structs.</param>
    /// <returns>The generated dictionary.</returns>
    public static Dictionary<string, AudioClip> ConvertListToDictionary(List<UniteonAudioClip> uniteonSfxList) => 
        uniteonSfxList.ToDictionary(sfx => sfx.ClipName, sfx => sfx.Clip);
}
