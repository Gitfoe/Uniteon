using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealerController : NpcController
{
    private Transition _transition;
    
    /// <summary>
    /// Load the transition class.
    /// </summary>
    public override void Start()
    {
        base.Start();
        _transition = FindObjectOfType<Transition>();
    }
    
    public override void Interact(Transform initiator)
    {
        if (NpcState == NpcState.Idling)
        {
            NpcState = NpcState.Dialog;
            Character.LookTowards(initiator.position);
            StartCoroutine(DialogManager.Instance.PrintDialog(dialog, () =>
            { // Heal Uniteon and then go back to the idling state
                StartCoroutine(HealUniteonsSequence(initiator.GetComponentInParent<UniteonParty>()));
            }));
        }
    }

    /// <summary>
    /// Plays the fading, sfx, music and healing sequence for healing all party Uniteons.
    /// </summary>
    /// <param name="gamerParty">The party uniteon that needs to be healed.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator HealUniteonsSequence(UniteonParty gamerParty)
    {
        string sfxName = "centerHeal";
        float sfxLength = AudioManager.Sfx[sfxName].length;
        float fadeTime = 0.4f;
        gamerParty.HealAllUniteons();
        StartCoroutine(AudioManager.Instance.FadeMuteMusicVolume(fadeTime, sfxLength));
        yield return _transition.FadeIn(fadeTime, Color.black);
        AudioManager.Instance.PlaySfx(sfxName);
        yield return _transition.FadeOut(fadeTime, sfxLength, Color.black);
        IdleTimer = 0f;
        NpcState = NpcState.Idling;
    }
}
