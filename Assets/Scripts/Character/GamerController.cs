using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class GamerController : MonoBehaviour
{
    // Fields
    [SerializeField] private Camera mainCamera;
    [SerializeField] private SpriteRenderer transitionBlock;
    [SerializeField] private AudioClip sceneMusic;
    [SerializeField] private AudioClip wildMusicIntro;
    [SerializeField] private AudioClip wildMusicLoop;
    private Character _character;
    private Vector2 _gamerInput;
    private Vector2 _previousGamerInput;
    private float _cameraSize;
    private bool _inTransition;
    
    // Events
    public event Action OnEncountered;
    public event Action<Collider2D> OnInMentorsView;

    /// <summary>
    /// Initialise variables.
    /// </summary>
    private void Awake()
    { 
        _character = GetComponent<Character>();
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
        if (!_character.IsMoving && !_inTransition) // Check if the gamer is not currently moving or in transition
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
                StartCoroutine(_character.Move(_gamerInput, HandleMoveOver));
            }
            _character.HandleUpdate();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                Interact();
        }
    }

    /// <summary>
    /// Encapsulates all the checking methods.
    /// </summary>
    private void HandleMoveOver()
    {
        CheckWildGrass();
        CheckInMentorsView();
    }

    /// <summary>
    /// Checks if the gamer is on a wild grass grid position and if so triggers the battle scene based on odds.
    /// </summary>
    private void CheckWildGrass()
    {
        Vector3 grassPosition = transform.position;
        grassPosition.y -= 0.5f; // Move gamer down to make the game not think that you're touching grass when you're 1 grid below it
        Collider2D getNextObject = Physics2D.OverlapCircle(grassPosition, 0.2f, UnityLayers.Instance.WildGrassLayer);
        if (ReferenceEquals(getNextObject, null)) return;
        if (Random.Range(1, 101) <= 10)
        {
            _inTransition = true;
            _character.Animator.IsMoving = false;
            // Start battle transition
            AudioManager.Instance.PlayMusic(wildMusicIntro, wildMusicLoop);
            var sequence = DOTween.Sequence();
            sequence.Append(mainCamera.DOOrthoSize(_cameraSize + 2.5f, 1.35f));
            sequence.Append(mainCamera.DOOrthoSize(_cameraSize - 3.5f, 1.35f).SetEase(Ease.InSine));
            sequence.Join(transitionBlock.DOFade(1f, 1.35f).SetEase(Ease.InSine));
            sequence.OnComplete(() => OnEncountered?.Invoke());
        }
    }

    /// <summary>
    /// Checks if a gamer is in the view of a mentor.
    /// </summary>
    private void CheckInMentorsView()
    {
        Collider2D mentorCollider = Physics2D.OverlapCircle(transform.position, 0.2f, UnityLayers.Instance.FovLayer);
        if (!ReferenceEquals(mentorCollider, null))
            OnInMentorsView?.Invoke(mentorCollider);
    }

    /// <summary>
    /// Interacts with collider right in front of it.
    /// </summary>
    private void Interact()
    {
        Vector3 faceDir = new Vector3(_character.Animator.MoveX, _character.Animator.MoveY);
        Vector3 interactPos = transform.position + faceDir;
        Collider2D collider = Physics2D.OverlapCircle(interactPos, 0.3f, UnityLayers.Instance.InteractableLayer);
        if (collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }
}
