using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    
    public float moveSpeed;

    private bool _isMoving;

    private Vector2 _gamerInput;

    private Vector2 _previousGamerInput;
    // Start is called before the first frame update
    void Start()
    {
        
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
                var nextPos = transform.position; // transport.position = current position 
                nextPos.x += _gamerInput.x; // Save new position in temporary variable
                nextPos.y += _gamerInput.y;
                StartCoroutine(Move(nextPos));
            }
        }
    }

    /// <summary>
    /// Coroutine to move a gamer on the grid.
    /// </summary>
    /// <param name="nextPos">The next position the player needs to be moved to.</param>
    /// <returns>Nothing actually.</returns>
    IEnumerator Move(Vector3 nextPos)
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
}
