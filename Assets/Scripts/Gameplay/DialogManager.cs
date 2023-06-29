using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    // Fields
    [SerializeField] private GameObject dialogBox;
    [SerializeField] private Text dialogText;
    [SerializeField] private int typeOutSpeed;
    [SerializeField] private AudioClip aButton;
    private Dialog _dialog;
    private int _currentDialogLine;
    private bool _isTyping; // To ensure gamer can't go to the next line while line is still printing
    
    // Events
    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    // Properties
    public static DialogManager Instance { get; private set; }
    public bool IsOpen { get; private set; }

    /// <summary>
    /// Makes this instance publicly available.
    /// </summary>
    private void Awake() => Instance = this;

    /// <summary>
    /// Prints NPC dialog to the dialog box in the world UI.
    /// </summary>
    /// <param name="dialog">The dialog that needs to be printed.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator PrintDialog(Dialog dialog)
    {
        yield return new WaitForEndOfFrame(); // Wait 1 frame because GetKeyDown is still active in the same frame
        OnShowDialog?.Invoke();
        IsOpen = true;
        AudioManager.Instance.PlaySfx(aButton);
        _dialog = dialog;
        dialogBox.SetActive(true);
        StartCoroutine(TypeOutDialog(dialog.Lines[0]));
    }

    /// <summary>
    /// Detects next dialog input.
    /// </summary>
    public void ControllerUpdate()
    {
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && !_isTyping)
        {
            AudioManager.Instance.PlaySfx(aButton);
            _currentDialogLine++;
            if (_currentDialogLine < _dialog.Lines.Count)
            {
                StartCoroutine(TypeOutDialog(_dialog.Lines[_currentDialogLine]));
            }
            else
            {
                IsOpen = false;
                _currentDialogLine = 0;
                dialogBox.SetActive(false);
                OnCloseDialog?.Invoke();
            }
        }
    }
    
    /// <summary>
    /// Types out text based on a certain interval.
    /// </summary>
    /// <param name="line">Text that has to be set.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator TypeOutDialog(string line)
    {
        _isTyping = true;
        dialogText.text = "";
        foreach (var l in line)
        {
            dialogText.text += l;
            yield return new WaitForSeconds(1f/typeOutSpeed);
        }
        _isTyping = false;
    } 
}
