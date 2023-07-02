using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GamerController : MonoBehaviour
{
    // Fields
    [SerializeField] private string gamerName;
    [SerializeField] private Sprite sprite;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Image transition;
    [SerializeField] private AudioClip[] grassSteps;
    private Character _character;
    private Vector2 _gamerInput;
    private Vector2 _previousGamerInput;
    private float _cameraSize;
    private bool _inTransition;
    private MentorController _battlingMentor;

    // Properties
    public string GamerName => gamerName;
    public Sprite Sprite => sprite;
    public Character Character => _character;

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
    }

    /// <summary>
    /// Play music.
    /// </summary>
    private void Start() => AudioManager.Instance.PlayMusic("eternaCity");

    /// <summary>
    /// Sets a few key variables for switching back to the world mode from battle modes.
    /// </summary>
    public void InitialiseWorld()
    {
        // Set camera back to original size
        mainCamera.orthographicSize = _cameraSize;
        // Start playing music
        AudioManager.Instance.PlayMusic("eternaCity");
        // Fade in
        transition.color = new Color(0f, 0f, 0f, 1f);
        transition.DOFade(0f, 0.72f).OnComplete(() => transition.gameObject.SetActive(false));
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
    /// Encapsulates all the other checking methods.
    /// </summary>
    private void HandleMoveOver()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, _character.YOffset), 0.2f, UnityLayers.Instance.TriggerableLayers);
        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (!ReferenceEquals(triggerable, null))
            {
                _character.Animator.IsMoving = false;
                triggerable.OnPlayerTriggered(this);
                break;
            }
        }
        CheckWildGrass();
        CheckInMentorsView();
    }

    /// <summary>
    /// Checks if the gamer is on a wild grass grid position and if so triggers the battle scene based on odds.
    /// </summary>
    private void CheckWildGrass()
    {
        Collider2D getNextObject = Physics2D.OverlapCircle(transform.position - new Vector3(0, _character.YOffset), 0.2f, UnityLayers.Instance.WildGrassLayer);
        if (ReferenceEquals(getNextObject, null)) return;
        if (Random.Range(1, 101) <= 10)
        {
            _character.Animator.IsMoving = false;
            // Start battle transition
            Sequence sequence = DOTween.Sequence();
            BattleTransition( AudioManager.Music["wildBattleIntro"], AudioManager.Music["wildBattleLoop"], sequence);
            sequence.OnComplete(() =>
            {
                OnEncountered?.Invoke(null);
                _inTransition = false;
                Debug.Log($"In battle transition: {_inTransition}");
            });
        }
        else
            PlayRandomGrassClip();
    }

    /// <summary>
    /// Checks if a gamer is in the view of a mentor and initiates the mentor's eyes meet sequence via an event.
    /// </summary>
    private void CheckInMentorsView()
    {
        Collider2D mentorCollider = Physics2D.OverlapCircle(transform.position - new Vector3(0, _character.YOffset), 0.2f, UnityLayers.Instance.FovLayer);
        if (!ReferenceEquals(mentorCollider, null))
            _battlingMentor = mentorCollider.GetComponentInParent<MentorController>();
        else return;
        if (_battlingMentor is { BattleLost: false })
        {
            _character.Animator.IsMoving = false;
            _character.LookTowards(mentorCollider.transform.position);
            OnInMentorsView?.Invoke(mentorCollider);
        }
        // Start mentor eyes meet sequence, Only execute battle transition if mentor & battle available
    }
    
    /// <summary>
    /// Shows a battle transition animation and invokes an event letting mentors know when the transition is done.
    /// </summary>
    public void TransitionIntoMentorBattle()
    {
        Debug.Log($"In battle transition: {_inTransition}");
        Sequence sequence = DOTween.Sequence();
        BattleTransition(_battlingMentor.BattleIntro, _battlingMentor.BattleLoop, sequence, 2.8f);
        sequence.OnComplete(() =>
        {
            OnTransitionDone?.Invoke();
            OnTransitionDone = null; // Clear from subscribed mentors after transition done
            _inTransition = false; // No longer in transition
            _battlingMentor = null; // No longer needed
        });
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
        transition.color = new Color(1f, 1f, 1f, 0f);
        transition.gameObject.SetActive(true);
        Debug.Log($"In battle transition: {_inTransition}");
        if (ReferenceEquals(sequence, null))
            sequence = DOTween.Sequence();
        AudioManager.Instance.PlayMusic(introClip, loopClip);
        float halvedTransitionTime = transitionTime / 2;
        sequence.Append(mainCamera.DOOrthoSize(_cameraSize + 2.5f, halvedTransitionTime));
        sequence.Append(mainCamera.DOOrthoSize(_cameraSize - 3.5f, halvedTransitionTime).SetEase(Ease.InSine));
        sequence.Join(transition.DOFade(1f, halvedTransitionTime).SetEase(Ease.InSine));
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
    
    /// <summary>
    /// Plays a random grass step audio sfx.
    /// </summary>
    private void PlayRandomGrassClip()
    {
        AudioClip grassClip = grassSteps[Random.Range(0, grassSteps.Length)];
        AudioManager.Instance.PlaySfx(grassClip);
    }
}
