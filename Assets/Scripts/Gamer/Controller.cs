using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask objectsLayer;
    public LayerMask wildGrassLayer;
    private Animator _animator;
    private bool _isMoving;
    private Vector2 _gamerInput;
    private Vector2 _previousGamerInput;
    private static readonly int MoveX = Animator.StringToHash("moveX");
    private static readonly int MoveY = Animator.StringToHash("moveY");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    // Awake is called when the script is loaded
    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isMoving) // Check if the gamer is not currently moving
        { 
            // Check every frame if the gamer is applying an input
            _gamerInput.x = Input.GetAxisRaw("Horizontal"); // -1, 0 or 1
            _gamerInput.y = Input.GetAxisRaw("Vertical");
            if (_gamerInput != Vector2.zero) // Only do something if position has to be changed
            {
                if (_gamerInput.x != 0 && _gamerInput.y != 0) // Only do something if gamer inputs diagonally
                {
                    _gamerInput.x *= _previousGamerInput.x == 0 ? 1 : 0;
                    _gamerInput.y *= _previousGamerInput.y == 0 ? 1 : 0;
                }
                else
                    _previousGamerInput = _gamerInput; // Save previous gamer input to know what to do with diagonal movements
                _animator.SetFloat(MoveX, _gamerInput.x); // Pass variables through to animator
                _animator.SetFloat(MoveY, _gamerInput.y);
                var nextPos = transform.position; // transport.position = current position 
                nextPos.x += _gamerInput.x; // Save new position in temporary variable
                nextPos.y += _gamerInput.y;
                if (IsWalkable(nextPos))
                {
                    StartCoroutine(Move(nextPos));
                    CheckWildGrass();
                }
            }
            _animator.SetBool(IsMoving, _isMoving);
        }
    }

    /// <summary>
    /// Coroutine to move a gamer on the grid.
    /// </summary>
    /// <param name="nextPos">The next position the player needs to be moved to.</param>
    /// <returns>Nothing of importance.</returns>
    private IEnumerator Move(Vector3 nextPos)
    {
        _isMoving = true;
        // Check if player's target position & current position is greater than a very small value (Epsilon)
        while ((nextPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // Move the player to a new location by a very small value
            transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
            // Time.deltaTime ensures the movement is independent of framerate (otherwise higher FPS will make it faster)
            yield return null; // Keep repeating while loop until current position and target position are really close
        }
        transform.position = nextPos; // If the current and target position are close, just set the position
        _isMoving = false;
    }

    /// <summary>
    /// Checks if a tile is walkable and not blocked by an object.
    /// </summary>
    /// <param name="nextPos">The next position the player wants to move to.</param>
    /// <returns>True if walkable and false if not.</returns>
    private bool IsWalkable(Vector3 nextPos)
    {
        nextPos.y -= 0.5f; // move from head to foot of player to look more natural in-game
        var getNextObject = Physics2D.OverlapCircle(nextPos, 0.2f, objectsLayer);
        return ReferenceEquals(getNextObject, null);
    }

    /// <summary>
    /// Checks if the gamer is on a wild grass grid position and if so triggers the battle scene based on odds.
    /// </summary>
    private void CheckWildGrass()
    {
        var getNextObject = Physics2D.OverlapCircle(transform.position, 0.2f, wildGrassLayer);
        if (ReferenceEquals(getNextObject, null)) return;
        if (Random.Range(0, 100) <= 10)
            Debug.Log("Triggered wild encounter (code has to be programmed still)");
    }
}
