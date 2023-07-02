using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    // Fields
    [SerializeField] private int loadScene = -1;
    [SerializeField] private Transform spawn;
    [SerializeField] private PortalDestination destination;
    [SerializeField] private PortalSfx portalSfx;
    [SerializeField] private string sceneMusicIntro;
    [SerializeField] private string sceneMusicLoop;
    private Transition _transition;
    private GamerController _gamer;

    /// <summary>
    /// When a gamer enters a portal.
    /// </summary>
    /// <param name="gamer">The gamer itself.</param>
    public void OnPlayerTriggered(GamerController gamer)
    {
        _gamer = gamer;
        StartCoroutine(SwitchScene());
    }

    /// <summary>
    /// Load transition fader.
    /// </summary>
    private void Start()
    {
        _transition = FindObjectOfType<Transition>();
    }

    /// <summary>
    /// Switches the scene when a portal is entered.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator SwitchScene()
    {
        // Transition in - don't destroy portal
        DontDestroyOnLoad(gameObject);
        GameController.Instance.PauseGame(true);
        PlayPortalSfx(portalSfx);
        // Play new scene music if set
        if (sceneMusicIntro == "none" || sceneMusicLoop == "none")
            StartCoroutine(AudioManager.Instance.StopMusic(true, 0.72f));
        else if (sceneMusicIntro != "" && sceneMusicLoop != "")
        {
            StartCoroutine(AudioManager.Instance.FadeOutMusicAndPlayNewMusic(sceneMusicIntro, sceneMusicLoop, 0.72f));
            AudioManager.Instance.SetPlayingWorldMusic(sceneMusicIntro, sceneMusicLoop);
        }
        else if (sceneMusicIntro == "" && sceneMusicLoop != "")
        {
            StartCoroutine(AudioManager.Instance.FadeOutMusicAndPlayNewMusic(sceneMusicLoop, 0.72f));
            AudioManager.Instance.SetPlayingWorldMusic(sceneMusicLoop);
        }
        // Fade in
        yield return _transition.FadeIn(0.72f, Color.black);
        // Load scene
        yield return SceneManager.LoadSceneAsync(loadScene);
        // Find same destination portal
        Portal destination = FindObjectsOfType<Portal>().First(x => x != this && x.destination == this.destination);
        // Set character position to portal
        _gamer.Character.SetPositionAndSnapToTile(destination.spawn.position);
        // Transition out
        yield return _transition.FadeOut(0.72f, Color.black);
        GameController.Instance.PauseGame(false);
        Destroy(gameObject); // After executing code, portal can be destroyed
    }

    /// <summary>
    /// Plays a portal sfx.
    /// </summary>
    /// <param name="type">The portal sfx you want to have played.</param>
    private void PlayPortalSfx(PortalSfx type)
    {
        string sfxName;
        switch (type)
        {
            case PortalSfx.GoInside:
                sfxName = "goInside";
                break;
            case PortalSfx.GoOutside:
                sfxName = "goOutside";
                break;
            default:
                return;
        }
        AudioManager.Instance.PlaySfx(sfxName);
    }
}

/// <summary>
/// In case there are multiple portals pointing to the same scene.
/// </summary>
public enum PortalDestination
{
    A, B, C, D, E, F, G, H, I, J, K, L, M, N, O, P, Q, R, S, T, U, V, W, X, Y, Z
}

/// <summary>
/// Which sfx the portal should play.
/// </summary>
public enum PortalSfx
{
    None,
    GoInside,
    GoOutside,
}