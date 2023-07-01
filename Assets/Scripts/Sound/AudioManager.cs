using System.Collections;
using DG.Tweening;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Fields
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private AudioSource sfxPlayer1;
    [SerializeField] private AudioSource sfxPlayer2;
    private Coroutine _playMusicRoutine;
    
    // Properties
    public static AudioManager Instance { get; private set; }

    /// <summary>
    /// Sets this instance of the audio manager to a public static variable.
    /// </summary>
    private void Awake()
    {
        Instance = this;
    }

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
            _playMusicRoutine = StartCoroutine(PlayMusicRoutine(beginClip, loopClip));
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

    /// <summary>
    /// Stops any active music from playing.
    /// </summary>
    /// <param name="fade">Whether to fade out the music.</param>
    /// <param name="fadeDuration">The duration of the fade-out effect.</param>
    public void StopMusic(bool fade = false, float fadeDuration = 0f)
    {
        // Fade out the music if fade is enabled and fadeDuration is greater than 0
        if (musicPlayer.isPlaying && fade && fadeDuration > 0f)
        {
            float startVolume = musicPlayer.volume;
            musicPlayer.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                musicPlayer.Stop();
                musicPlayer.volume = startVolume;
            });
        }
        // Stop the music immediately
        else if (musicPlayer.isPlaying)
            musicPlayer.Stop();
        // Stop any active music coroutine
        if (_playMusicRoutine != null)
            StopCoroutine(_playMusicRoutine);
    }

    /// <summary>
    /// Stops the playing sound effect on a specific audio channel.
    /// </summary>
    /// <param name="channel">The SFX player channel the audio must be stopped on.</param>
    /// <param name="clip">Only stop if this audio clip is playing.</param>
    public void StopSfx(int channel = 1, AudioClip clip = null)
    {
        AudioSource sfxPlayerChannel = SelectSfxPlayerChannel(channel);
        if (clip != null && sfxPlayerChannel.isPlaying && sfxPlayerChannel.clip == clip)
            sfxPlayerChannel.Stop();
        else if (clip == null && sfxPlayerChannel.isPlaying)
            sfxPlayerChannel.Stop();
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
}
