using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class GamerController : MonoBehaviour
{
    // Fields
    [SerializeField] private float moveSpeed;
    [SerializeField] private LayerMask objectsLayer;
    [SerializeField] private LayerMask wildGrassLayer;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private SpriteRenderer transitionBlock;
    [SerializeField] private AudioClip sceneMusic;
    [SerializeField] private AudioClip wildMusicIntro;
    [SerializeField] private AudioClip wildMusicLoop;
    private Animator _animator;
    private bool _isMoving;
    private Vector2 _gamerInput;
    private Vector2 _previousGamerInput;
    private static readonly int MoveX = Animator.StringToHash("moveX");
    private static readonly int MoveY = Animator.StringToHash("moveY");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private float _cameraSize;
    private bool _inTransition;
    
    // Events
    public event Action OnEncountered;

    /// <summary>
    /// Initialise variables.
    /// </summary>
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _cameraSize = mainCamera.orthographicSize;
        _inTransition = false;
        AudioManager.Instance.PlayMusic(sceneMusic);
    }

    /// <summary>
    /// Sets a few key variables for switching back to the world mode from battle modes.
    /// </summary>
    public void InitialiseWorld()
    {
        transitionBlock.color = new Color(1f, 1f, 1f, 0f);
        mainCamera.orthographicSize = _cameraSize;
        _inTransition = false;
        AudioManager.Instance.PlayMusic(sceneMusic);
    }

    /// <summary>
    /// Update is called once per frame if set active by the GameController.
    /// </summary>
    public void ControllerUpdate()
    {
        if (!_isMoving && !_inTransition) // Check if the gamer is not currently moving or in transition
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
                    StartCoroutine(Move(nextPos));
            }
            _animator.SetBool(IsMoving, _isMoving);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                Interact();
        }
    }

    /// <summary>
    /// Coroutine to move a gamer on the grid.
    /// </summary>
    /// <param name="nextPos">The next position the player needs to be moved to.</param>
    /// <returns>Coroutine.</returns>
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
        CheckWildGrass(); // Check if after moving a wild Uniteon has been encountered
    }

    /// <summary>
    /// Checks if a tile is walkable and not blocked by an object.
    /// </summary>
    /// <param name="nextPos">The next position the player wants to move to.</param>
    /// <returns>True if walkable and false if not.</returns>
    private bool IsWalkable(Vector3 nextPos)
    {
        nextPos.y -= 0.5f; // move from head to foot of player to look more natural in-game
        var getNextObject = Physics2D.OverlapCircle(nextPos, 0.2f, objectsLayer | interactableLayer);
        return ReferenceEquals(getNextObject, null);
    }

    /// <summary>
    /// Checks if the gamer is on a wild grass grid position and if so triggers the battle scene based on odds.
    /// </summary>
    private void CheckWildGrass()
    {
        Vector3 grassPosition = transform.position;
        grassPosition.y -= 0.5f; // Move gamer down to make the game not think that you're touching grass when you're 1 grid below it
        Collider2D getNextObject = Physics2D.OverlapCircle(grassPosition, 0.2f, wildGrassLayer);
        if (ReferenceEquals(getNextObject, null)) return;
        if (Random.Range(1, 101) <= 10)
        {
            _inTransition = true;
            _animator.SetBool(IsMoving, false);
            // Start battle transition
            AudioManager.Instance.PlayMusic(wildMusicIntro, wildMusicLoop);
            var sequence = DOTween.Sequence();
            sequence.Append(mainCamera.DOOrthoSize(_cameraSize + 2.5f, 1.35f));
            sequence.Append(mainCamera.DOOrthoSize(_cameraSize - 3.5f, 1.35f).SetEase(Ease.InSine));
            sequence.Join(transitionBlock.DOFade(1f, 1.35f).SetEase(Ease.InSine));
            sequence.OnComplete(() => OnEncountered?.Invoke());
        }
    }

    private void Interact()
    {
        Vector3 faceDir = new Vector3(_animator.GetFloat(MoveX), _animator.GetFloat(MoveY));
        Vector3 interactPos = transform.position + faceDir;
        Collider2D collider = Physics2D.OverlapCircle(interactPos, 0.3f, interactableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact();
        }
    }
}
