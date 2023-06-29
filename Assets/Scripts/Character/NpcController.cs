using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    // Fields
    [SerializeField] private Dialog dialog;
    [SerializeField] private List<Vector2> movementPattern;
    [SerializeField] private float timeBetweenMove;
    private Character _character;
    private NpcState _npcState;
    private float _idleTimer;
    private int _currentPattern;

    private void Start()
    {
        _character = GetComponent<Character>();
    }

    /// <summary>
    /// Constantly walk the pattren set in movementPattern.
    /// </summary>
    private void Update()
    {
        if (DialogManager.Instance.IsOpen) return; // Don't walk when dialog is open
        if (_npcState == NpcState.Idling)
        {
            _idleTimer += Time.deltaTime;
            if (_idleTimer > timeBetweenMove)
            {
                _idleTimer = 0f;
                if (movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }
        _character.HandleUpdate();
    }

    /// <summary>
    /// Make the NPC walk 1 set of the movement pattern.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator Walk()
    {
        _npcState = NpcState.Walking;
        yield return _character.Move(movementPattern[_currentPattern]);
        _currentPattern = (_currentPattern + 1) % movementPattern.Count;
        _npcState = NpcState.Idling;
    }

    /// <summary>
    /// As the NPC, speak to the dialog box.
    /// </summary>
    public void Interact()
    {
        if (_npcState == NpcState.Idling)
            StartCoroutine(DialogManager.Instance.PrintDialog(dialog));
    }
}

public enum NpcState
{
    Idling,
    Walking 
}
