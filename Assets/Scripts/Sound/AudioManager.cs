using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicPlayer;
    [SerializeField] private AudioSource sfxPlayer;
    
    public static AudioManager i { get; private set; }

    private void Awake()
    {
        i = this;
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null)
            return;
        musicPlayer.clip = clip;
        musicPlayer.loop = loop;
        musicPlayer.Play();
    }

    public void PlayMusic(AudioClip beginClip, AudioClip loopClip)
    {
        if (beginClip == null)
            PlayMusic(loopClip);
        else
            StartCoroutine(PlayMusicRoutine(beginClip, loopClip));
    }

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
}
