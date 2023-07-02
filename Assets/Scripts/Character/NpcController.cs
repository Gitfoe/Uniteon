using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    // Fields
    [SerializeField] protected Dialog dialog;
    [SerializeField] private List<Vector2> movementPattern;
    [SerializeField] private float timeBetweenMove;
    protected float IdleTimer;
    private int _currentPattern;
    protected Character Character;
    protected NpcState NpcState;

    public virtual void Start()
    {
        Character = GetComponent<Character>();
    }

    /// <summary>
    /// Constantly walk the pattern set in movementPattern.
    /// </summary>
    private void Update()
    {
        if (NpcState == NpcState.Idling)
        {
            IdleTimer += Time.deltaTime;
            if (IdleTimer > timeBetweenMove)
            {
                IdleTimer = 0f;
                if (movementPattern.Count > 0)
                    StartCoroutine(Walk());
            }
        }
        Character.HandleUpdate();
    }

    /// <summary>
    /// Make the NPC walk 1 set of the movement pattern.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator Walk()
    {
        NpcState = NpcState.Walking;
        Vector3 oldPosition = transform.position;
        yield return Character.Move(movementPattern[_currentPattern]);
        if (transform.position != oldPosition) // Only increment pattern count if NPC walked
            _currentPattern = (_currentPattern + 1) % movementPattern.Count;
        NpcState = NpcState.Idling;
    }

    /// <summary>
    /// As the NPC, speak to the dialog box.
    /// </summary>
    /// <param name="initiator">The transform of the GameObject that initiated the interaction.</param>
    public virtual void Interact(Transform initiator)
    {
        if (NpcState == NpcState.Idling)
        {
            NpcState = NpcState.Dialog;
            Character.LookTowards(initiator.position);
            StartCoroutine(DialogManager.Instance.PrintDialog(dialog, () =>
            { // Go back to the idling state once the dialog is over
                IdleTimer = 0f;
                NpcState = NpcState.Idling;
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
