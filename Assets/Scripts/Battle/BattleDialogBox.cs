using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] private int typeOutSpeed;
    [SerializeField] private Text dialogText;

    /// <summary>
    /// Sets dialog text immediately.
    /// </summary>
    /// <param name="text">Text that has to be set.</param>
    public void SetDialogText(string text)
    {
        dialogText.text = text;
    }

    /// <summary>
    /// Types out text based on a certain interval.
    /// </summary>
    /// <param name="text">Text that hast o be set.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator TypeOutDialog(string text)
    {
        dialogText.text = "";
        foreach (var l in text)
        {
            dialogText.text += l;
            yield return new WaitForSeconds(1f/typeOutSpeed);
        }
    }
    
}
