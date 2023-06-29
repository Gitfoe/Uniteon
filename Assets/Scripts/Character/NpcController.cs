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
        Vector3 oldPosition = transform.position;
        yield return _character.Move(movementPattern[_currentPattern]);
        if (transform.position != oldPosition) // Only increment pattern count if NPC walked
            _currentPattern = (_currentPattern + 1) % movementPattern.Count;
        _npcState = NpcState.Idling;
    }

    /// <summary>
    /// As the NPC, speak to the dialog box.
    /// </summary>
    /// <param name="initiatior">The transformr of the GameObject that initiated the interaction.</param>
    public void Interact(Transform initiatior)
    {
        if (_npcState == NpcState.Idling)
        {
            _npcState = NpcState.Dialog;
            _character.LookTowards(initiatior.position);
            StartCoroutine(DialogManager.Instance.PrintDialog(dialog, () =>
            { // Go back to the idling state once the dialog is over
                _idleTimer = 0f;
                _npcState = NpcState.Idling;
            }));
        }
    }
    
    /// <summary>
    /// Shows the path of the configured movement pattern in the Unity Editor.
    /// </summary>
    [ContextMenu("Show Path")]
    public void ShowPath()
    {
        var position = transform.position;
        var pos = new Vector2(position.x, position.y);
        var index = 0;
        var colours = new List<Color>()
        {
            Color.red,
            Color.green,
            Color.blue
        };
        foreach (Vector2 path in movementPattern)
        {
            Vector2 newPosRef = movementPattern[index];
            if(newPosRef.x == 0)
                newPosRef.y *= 1f;
            else if (newPosRef.y == 0)
                newPosRef.x *= 1f;
            Debug.DrawLine(pos, pos + newPosRef, colours[index % 3], 2f);
            index += 1;
            pos += newPosRef;
        }
    }
}

public enum NpcState
{
    Idling,
    Walking,
    Dialog
}
