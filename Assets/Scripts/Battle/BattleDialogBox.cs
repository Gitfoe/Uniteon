using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialogBox : MonoBehaviour
{
    [SerializeField] private int typeOutSpeed;
    [SerializeField] private Text dialogText;
    [SerializeField] private GameObject actionSelector;
    [SerializeField] private GameObject moveSelector;
    [SerializeField] private GameObject moveDetails;
    [SerializeField] private List<Text> actionTexts;
    [SerializeField] private List<Text> moveTexts;
    [SerializeField] private Text powerPointsText;
    [SerializeField] private Text typeText;
    [SerializeField] private Color selectedColour;
    [SerializeField] private Color deselectedColour;

    /// <summary>
    /// Sets dialog text immediately.
    /// </summary>
    /// <param name="text">Text that has to be set.</param>
    public void SetDialogText(string text) => dialogText.text = text;

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
    
    /// <summary>
    /// Enables or disables the move selector and the accompanying move details.
    /// </summary>
    public void EnableMoveSelector(bool enabled)
    {
        moveSelector.SetActive(enabled);
        moveDetails.SetActive(enabled);
    }

    public void EnableDialogText(bool enabled) => dialogText.enabled = enabled;

    public void EnableActionSelector(bool enabled) => actionSelector.SetActive(enabled);

    /// <summary>
    /// Highlights the correct selection in some texts list.
    /// </summary>
    /// <param name="selectedText">The selected text.</param>
    /// <param name="textsList">The list that the selection has to take place in.</param>
    private void UpdateSelection(int selectedText, List<Text> textsList)
    {
        for (var i = 0; i < textsList.Count; i++)
        {
            textsList[i].color = i == selectedText ? selectedColour : deselectedColour;
        }
    }
    
    /// <summary>
    /// Highlights the correct action selection.
    /// </summary>
    /// <param name="selectedAction">The currently selected action.</param>
    public void UpdateActionSelection(int selectedAction) => UpdateSelection(selectedAction, actionTexts);

    /// <summary>
    /// Highlights the correct move selection.
    /// </summary>
    /// <param name="selectedMove">The currently selected move.</param>
    /// <param name="uniteonMove">The move that is selected.</param>
    public void UpdateMoveSelection(int selectedMove, Move uniteonMove)
    {
        powerPointsText.text = $"PP {uniteonMove.PowerPoints}/{uniteonMove.MoveBase.PowerPoints}";
        typeText.text = uniteonMove.MoveBase.MoveType.ToString();
        UpdateSelection(selectedMove, moveTexts);
    }

    /// <summary>
    /// Sets the move names the Uniteon knows to the battle screen.
    /// </summary>
    /// <param name="moves">The list of moves a Uniteon knows.</param>
    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].MoveBase.MoveName;
            else
                moveTexts[i].text = "---";
        }
    }
}
