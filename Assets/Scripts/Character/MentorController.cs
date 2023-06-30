using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MentorController : MonoBehaviour, Interactable
{
    // Fields
    [SerializeField] private Dialog dialog;
    [SerializeField] private Dialog dialogAfterBattle;
    [SerializeField] private string mentorName;
    [SerializeField] private Sprite sprite;
    [SerializeField] private GameObject exclamationMark;
    [SerializeField] private GameObject fov;
    [SerializeField] private AudioClip eyesMeetIntro;
    [SerializeField] private AudioClip eyesMeetLoop;
    private Character _character;
    private Collider2D _collider;
    private bool _battleLost;
    
    // Properties
    public string MentorName => mentorName;
    public Sprite Sprite => sprite;
    public bool BattleLost => _battleLost;
    
    private void Awake()
    {
        _character = GetComponent<Character>();
        _collider = fov.GetComponent<Collider2D>();
    }

    private void Start()
    {
        SetFovRotation(_character.Animator.InitialCardinal);
    }
    
    /// <summary>
    /// Gets triggered once the gamer enters the mentor's FOV.
    /// </summary>
    /// <param name="gamer">The controller of the gamer that interacted with this mentor.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator TriggerMentorBattle(GamerController gamer = null)
    {
        AudioManager.Instance.PlayMusic(eyesMeetIntro, eyesMeetLoop);
        yield return AnimateExclamationMark(0.5f, 0.27f);
        if (!ReferenceEquals(gamer, null))
        {
            // Move towards the gamer
            Vector3 differenceVector = gamer.transform.position - transform.position;
            Vector3 moveVector = differenceVector - differenceVector.normalized; // Subtract by 1
            moveVector = new Vector3(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));
            yield return _character.Move(moveVector);
        }

        // Open dialog
        StartCoroutine(DialogManager.Instance.PrintDialog(dialog));
    }
    
    /// <summary>
    /// Faces the mentor to another direction and changes it's FOV rotation as well.
    /// </summary>
    /// <param name="initiator">The transform of the initiator.</param>
    public void Interact(Transform initiator)
    {
        var position = initiator.position;
        var facing = _character.GetFacingDirection(position);
        SetFovRotation(facing);
        _character.LookTowards(position);
        if (_battleLost)
            StartCoroutine(DialogManager.Instance.PrintDialog(dialogAfterBattle));
    }
    
    /// <summary>
    /// Gets fired once the transition animation is done by the gamer.
    /// </summary>
    public void InitiateMentorBattle() => GameController.Instance.InitiateBattle(this);

    public void HandleBattleLost()
    {
        fov.gameObject.SetActive(false);
        _battleLost = true;
    }

    /// <summary>
    /// Animates the exclamation mark that shows when interacted with a mentor.
    /// </summary>
    /// <param name="moveDuration">The duration of the animation.</param>
    /// <param name="fadeDuration">The duration of the fading.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator AnimateExclamationMark(float moveDuration, float fadeDuration)
    {
        exclamationMark.SetActive(true);
        // Store the original position of the exclamation mark
        var position = exclamationMark.transform.position;
        Vector3 originalPosition = position;
        // Move the exclamation mark slightly down from its original position
        position += new Vector3(0f, 0.5f, 0f);
        exclamationMark.transform.position = position;
        // Fade in the exclamation mark
        exclamationMark.GetComponent<SpriteRenderer>().DOFade(1f, fadeDuration);
        // Move the exclamation mark up to its original position
        exclamationMark.transform.DOMove(originalPosition, moveDuration);
        // Wait for the specified duration
        yield return new WaitForSeconds(moveDuration + fadeDuration);
        // Fade out the exclamation mark
        exclamationMark.GetComponent<SpriteRenderer>().DOFade(0f, fadeDuration);
        // Wait for the fade out to complete
        yield return new WaitForSeconds(fadeDuration);
        exclamationMark.SetActive(false);
    }

    /// <summary>
    /// Sets the FOV rotation to be the same angle as the facing direction.
    /// </summary>
    /// <param name="cardinal">The cardinal direction.</param>
    private void SetFovRotation(FacingCardinal cardinal)
    {
        // Default: West
        float angle = 0;
        switch (cardinal)
        {
            case FacingCardinal.North:
                angle = 180;
                break;
            case FacingCardinal.East:
                angle = 90;
                break;
            case FacingCardinal.West:
                angle = 270;
                break;
        }
        fov.transform.eulerAngles = new Vector3(0f, 0f, angle);
        // Disable and re-enable the collider to trigger collision detection
        _collider.enabled = false;
        _collider.enabled = true;
    }
}
