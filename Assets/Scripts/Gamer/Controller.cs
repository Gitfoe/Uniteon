using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    
    public float moveSpeed;

    private bool _isMoving;

    private Vector2 _gamerInput;
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
                var nextPosition = transform.position; // transport.position = current position 
                nextPosition.x += _gamerInput.x; // Save new position in temporary variable
                nextPosition.y += _gamerInput.y;
                StartCoroutine(Move(nextPosition));
            }
        }
    }

    IEnumerator Move(Vector3 nextPos)
    {
        _isMoving = true;
        // Check if player's target position & current position is greater than a very small value (Epsilon)
        while ((nextPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            // Move the player to a new location by a very small value
            transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
            yield return null; // Keep repeating until current position and target position are really close
        }
        transform.position = nextPos; // If the current and target position are close, just set the position
        _isMoving = false;
    }
}
