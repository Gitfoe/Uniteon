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
        _animator.MoveX = Mathf.Clamp(moveVector.x, -1f, 1f); // Pass variables through to animator
        _animator.MoveY = Mathf.Clamp(moveVector.y, -1f, 1f);
        var nextPos = transform.position; // transport.position = current position 
        nextPos.x += moveVector.x; // Save new position in temporary variable
        nextPos.y += moveVector.y;
        if (!IsPathWalkable(nextPos))
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
    private bool IsTileWalkable(Vector3 nextPos)
    {
        nextPos.y -= 0.5f; // move from head to foot of player to look more natural in-game
        var getNextObject = Physics2D.OverlapCircle(nextPos, 0.2f, UnityLayers.Instance.ObjectsLayer | UnityLayers.Instance.InteractableLayer);
        return ReferenceEquals(getNextObject, null);
    }

    /// <summary>
    /// Box cast to determine if entire walking path is clear.
    /// </summary>
    /// <param name="nextPos"></param>
    /// <returns></returns>
    private bool IsPathWalkable(Vector3 nextPos)
    {
        Vector3 currentPosition = transform.position;
        Vector3 difference = nextPos - currentPosition; // Difference between target and current pos
        Vector3 direction = difference.normalized; // Length & direction, get direction by getting normalized (same direction with length 1)
        Vector3 position = currentPosition + direction; // Add 1 unit
        return !Physics2D.BoxCast(position, new Vector2(0.2f, 0.2f), 0f, direction, difference.magnitude - 1,
            UnityLayers.Instance.ObjectsLayer | UnityLayers.Instance.InteractableLayer | UnityLayers.Instance.PlayerLayer); // True if collider found in area
    }

    /// <summary>
    /// Faces an NPC towards a target direction.
    /// </summary>
    /// <param name="targetDirection">The position the character needs to point towards.</param>
    /// <returns></returns>
    public void LookTowards(Vector3 targetDirection)
    {
        var position = transform.position;
        var xDifference = Mathf.Round(targetDirection.x) - Mathf.Round(position.x);
        var yDifference = Mathf.Round(targetDirection.y) - Mathf.Round(position.y);
        if (xDifference == 0 || yDifference == 0) // Only check cardinal directions, not diagonal
        {
            _animator.MoveX = Mathf.Clamp(xDifference, -1f, 1f); // Pass variables through to animator
            _animator.MoveY = Mathf.Clamp(yDifference, -1f, 1f);
        }
    }

    public FacingCardinal GetFacingDirection(Vector3 targetDirection)
    {
        var position = transform.position;
        var xDifference = Mathf.Round(targetDirection.x) - Mathf.Round(position.x);
        var yDifference = Mathf.Round(targetDirection.y) - Mathf.Round(position.y);
        if (xDifference == 0 || yDifference == 0) // Only check cardinal directions, not diagonal
        {
            float x = Mathf.Clamp(xDifference, -1f, 1f);
            float y = Mathf.Clamp(yDifference, -1f, 1f);
            if (x == 1)
                return FacingCardinal.East;
            if (x == -1)
                return FacingCardinal.West;
            if (y == 1)
                return FacingCardinal.North;
        }
        return FacingCardinal.South; // y == -1
    }
}
