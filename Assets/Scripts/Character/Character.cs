using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    // Fields
    [SerializeField] private float moveSpeed;
    private CharacterAnimator _animator;
    
    // Properties
    public CharacterAnimator Animator => _animator;
    public bool IsMoving { get; private set; }

    private void Awake()
    {
        _animator = GetComponent<CharacterAnimator>();
    }

    /// <summary>
    /// Coroutine to move a character on the grid.
    /// </summary>
    /// <param name="moveVector">The next position the character needs to be moved to.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator Move(Vector2 moveVector, Action OnMoveOver = null)
    {
        _animator.MoveX = moveVector.x; // Pass variables through to animator
        _animator.MoveY = moveVector.y;
        var nextPos = transform.position; // transport.position = current position 
        nextPos.x += moveVector.x; // Save new position in temporary variable
        nextPos.y += moveVector.y;
        if (!IsWalkable(nextPos))
            yield break;
        IsMoving = true;
        // Check if characters's target position & current position is greater than a very small value (Epsilon)
        while ((nextPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // Move the character to a new location by a very small value
            transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
            // Time.deltaTime ensures the movement is independent of framerate (otherwise higher FPS will make it faster)
            yield return null; // Keep repeating while loop until current position and target position are really close
        }
        transform.position = nextPos; // If the current and target position are close, just set the position
        IsMoving = false;
        OnMoveOver?.Invoke(); // Invoke any method that needs to be executed after movement
    }

    public void HandleUpdate() => _animator.IsMoving = IsMoving;

    /// <summary>
    /// Checks if a tile is walkable and not blocked by an object.
    /// </summary>
    /// <param name="nextPos">The next position the player wants to move to.</param>
    /// <returns>True if walkable and false if not.</returns>
    private bool IsWalkable(Vector3 nextPos)
    {
        nextPos.y -= 0.5f; // move from head to foot of player to look more natural in-game
        var getNextObject = Physics2D.OverlapCircle(nextPos, 0.2f, UnityLayers.Instance.ObjectsLayer | UnityLayers.Instance.InteractableLayer);
        return ReferenceEquals(getNextObject, null);
    }
}
