using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MentorController : MonoBehaviour
{
    [SerializeField] private Dialog dialog;
    [SerializeField] private GameObject exclamationMark;
    private Character _character;
    
    private void Awake()
    {
        _character = GetComponent<Character>();
    }
    
    /// <summary>
    /// Gets triggered once the gamer enters the mentor's FOV.
    /// </summary>
    /// <param name="gamer"></param>
    /// <returns></returns>
    public IEnumerator TriggerMentorBattle(GamerController gamer)
    {
        // Show exclamation mark
        exclamationMark.SetActive(true);
        yield return new WaitForSeconds(0.72f);
        exclamationMark.SetActive(true);
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
}
