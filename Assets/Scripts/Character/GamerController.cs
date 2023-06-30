using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class GamerController : MonoBehaviour
{
    // Fields
    [SerializeField] private string gamerName;
    [SerializeField] private Sprite sprite;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private SpriteRenderer transitionBlock;
    [SerializeField] private AudioClip sceneMusic;
    [SerializeField] private AudioClip wildMusicIntro;
    [SerializeField] private AudioClip wildMusicLoop;
    [SerializeField] private AudioClip mentorBattleIntro;
    [SerializeField] private AudioClip mentorBattleLoop;
    private Character _character;
    private Vector2 _gamerInput;
    private Vector2 _previousGamerInput;
    private float _cameraSize;
    private bool _inTransition;

    // Properties
    public string GamerName => gamerName;
    public Sprite Sprite => sprite;
    
    // Events
    public event Action<MentorController> OnEncountered;
    public event Action<Collider2D> OnInMentorsView;
    public event Action OnTransitionDone;

    /// <summary>
    /// Initialise variables.
    /// </summary>
    private void Awake()
    {
        _character = GetComponent<Character>();
        _cameraSize = mainCamera.orthographicSize;
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
            {
                Interact();
                CheckInMentorsView(); // Also check if in mentors view after they turned towards you
            }
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
            _character.Animator.IsMoving = false;
            // Start battle transition
            Sequence sequence = DOTween.Sequence();
            BattleTransition(wildMusicIntro, wildMusicLoop, sequence);
            sequence.OnComplete(() => OnEncountered?.Invoke(null));
        }
    }

    /// <summary>
    /// Checks if a gamer is in the view of a mentor and starts a battle.
    /// </summary>
    private void CheckInMentorsView()
    {
        Collider2D mentorCollider = Physics2D.OverlapCircle(transform.position, 0.2f, UnityLayers.Instance.FovLayer);
        if (!ReferenceEquals(mentorCollider, null))
        {
            _character.Animator.IsMoving = false;
            // Start mentor eyes meet sequence
            OnInMentorsView?.Invoke(mentorCollider);
            // Start battle transition
            DialogManager.Instance.OnCloseDialog += () =>
            {
                Sequence sequence = DOTween.Sequence();
                BattleTransition(mentorBattleIntro, mentorBattleLoop, sequence, 2.8f);
                sequence.OnComplete(() => OnTransitionDone?.Invoke());
            };
        }
    }
    
    /// <summary>
    /// Animates a transition into a battle and starts playing music,
    /// </summary>
    /// <param name="introClip">The intro part of an audio clip.</param>
    /// <param name="loopClip">The looping part of an audio clip.</param>
    /// <param name="sequence">An empty DOTween Sequence variable.</param>
    /// <param name="transitionTime">The total time it takes to complete th transition.</param>
    private void BattleTransition(AudioClip introClip, AudioClip loopClip, Sequence sequence = null, float transitionTime = 2.6f)
    {
        _inTransition = true;
        if (ReferenceEquals(sequence, null))
            sequence = DOTween.Sequence();
        AudioManager.Instance.PlayMusic(introClip, loopClip);
        float halvedTransitionTime = transitionTime / 2;
        sequence.Append(mainCamera.DOOrthoSize(_cameraSize + 2.5f, halvedTransitionTime));
        sequence.Append(mainCamera.DOOrthoSize(_cameraSize - 3.5f, halvedTransitionTime).SetEase(Ease.InSine));
        sequence.Join(transitionBlock.DOFade(1f, halvedTransitionTime).SetEase(Ease.InSine));
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
