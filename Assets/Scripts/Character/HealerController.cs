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
        yield return GameController.Instance.HealUniteonsTransition("centerHeal");
        gamerParty.HealAllUniteons();
        IdleTimer = 0f;
        NpcState = NpcState.Idling;
    }
}
