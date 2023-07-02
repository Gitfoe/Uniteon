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
    [SerializeField] private Transition transition;
    [SerializeField] private AudioClip[] grassSteps;
    private Character _character;
    private Vector2 _gamerInput;
    private Vector2 _previousGamerInput;
    private bool _inTransition;
    private MentorController _battlingMentor;
    private OverworldUniteonController _battlingOverworldUniteon;

    // Properties
    public string GamerName => gamerName;
    public Sprite Sprite => sprite;
    public Character Character => _character;

    // Events
    public event Action<MentorController, OverworldUniteonController> OnEncountered;
    public event Action<Collider2D> OnInMentorsView;
    public event Action<Collider2D> OnInUniteonsView;
    public event Action OnTransitionDone;

    /// <summary>
    /// Initialise variables.
    /// </summary>
    private void Awake()
    {
        _character = GetComponent<Character>();
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
        CheckInUniteonsView();
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
                OnEncountered?.Invoke(null, null);
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
    }
    
    /// <summary>
    /// Checks if in Uniteon's view and initates the dialog of the Uniteon.
    /// </summary>
    private void CheckInUniteonsView()
    {
        Collider2D uniteonCollider = Physics2D.OverlapCircle(transform.position - new Vector3(0, _character.YOffset), 0.2f, UnityLayers.Instance.FovLayer);
        if (!ReferenceEquals(uniteonCollider, null))
            _battlingOverworldUniteon = uniteonCollider.GetComponentInParent<OverworldUniteonController>();
        else return;
        if (!ReferenceEquals(_battlingOverworldUniteon, null))
        {
            _character.Animator.IsMoving = false;
            _character.LookTowards(uniteonCollider.transform.position);
            OnInUniteonsView?.Invoke(uniteonCollider);
        }
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
    /// Shows a battle transition animation and invokes an event letting overworld Uniteons know when the transition is done.
    /// </summary>
    public void TransitionIntoOverworldUniteonBattle()
    {
        Debug.Log($"In battle transition: {_inTransition}");
        Sequence sequence = DOTween.Sequence();
        BattleTransition(_battlingOverworldUniteon.BattleIntro, _battlingOverworldUniteon.BattleLoop, sequence, 2.8f);
        sequence.OnComplete(() =>
        {
            OnTransitionDone?.Invoke();
            OnTransitionDone = null; // Clear from subscribed mentors after transition done
            _inTransition = false; // No longer in transition
            _battlingOverworldUniteon = null; // No longer needed
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
        Debug.Log($"In battle transition: {_inTransition}");
        float cameraSize = mainCamera.orthographicSize;
        _inTransition = true;
        if (ReferenceEquals(sequence, null))
            sequence = DOTween.Sequence();
        AudioManager.Instance.PlayMusic(introClip, loopClip);
        float halvedTransitionTime = transitionTime / 2;
        sequence.Append(mainCamera.DOOrthoSize(cameraSize + 2.5f, halvedTransitionTime));
        sequence.Append(mainCamera.DOOrthoSize(cameraSize - 3.5f, halvedTransitionTime).SetEase(Ease.InSine));
        StartCoroutine(transition.FadeIn(halvedTransitionTime, halvedTransitionTime, Color.white, () =>
        {
            mainCamera.orthographicSize = cameraSize;
        }));
    }

    /// <summary>
    /// Interacts with collider right in front of it.
    /// </summary>
    private void Interact()
    {
        
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
