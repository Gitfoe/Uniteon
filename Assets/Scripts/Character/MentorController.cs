using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MentorController : MonoBehaviour
{
    [SerializeField] private Dialog dialog;
    [SerializeField] private GameObject exclamationMark;
    [SerializeField] private GameObject fov;
    private Character _character;
    
    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(_character.Animator.InitialCardinal);
    }
    
    /// <summary>
    /// Gets triggered once the gamer enters the mentor's FOV.
    /// </summary>
    /// <param name="gamer"></param>
    /// <returns></returns>
    public IEnumerator TriggerMentorBattle(GamerController gamer)
    {
        yield return AnimateExclamationMark(0.5f, 0.27f);
        // Move towards the gamer
        Vector3 differenceVector = gamer.transform.position - transform.position;
        Vector3 moveVector = differenceVector - differenceVector.normalized; // Subtract by 1
        moveVector = new Vector3(Mathf.Round(moveVector.x), Mathf.Round(moveVector.y));
        yield return _character.Move(moveVector);
        // Open dialog
        StartCoroutine(DialogManager.Instance.PrintDialog(dialog, () =>
        {
            // Start battle
            Debug.Log("start battle test");
        }));
    }
    
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
    public void SetFovRotation(FacingCardinal cardinal)
    {
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
    }
}
