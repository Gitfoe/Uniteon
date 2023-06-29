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
    [SerializeField] private Color lowPPColour;
    [SerializeField] private Color noPPColour;

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
        yield return new WaitForSeconds(0.72f);
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

    public void EnableActionSelector(bool enabled)
    {
        // Make dialog text box narrower and set selection active
        ThinDialogBox(enabled);
        actionSelector.SetActive(enabled);
    }

    /// <summary>
    /// Thins the dialog box to allow room for action selector..
    /// </summary>
    /// <param name="thin">To thin (true) or to widen (false).</param>
    private void ThinDialogBox(bool thin) =>
        dialogText.rectTransform.offsetMax = thin ? new Vector2(-300f, dialogText.rectTransform.offsetMax.y) : new Vector2(-25f, dialogText.rectTransform.offsetMax.y);

    /// <summary>
    /// Highlights the correct action selection.
    /// </summary>
    /// <param name="selectedAction">The currently selected action.</param>
    public void UpdateActionSelection(int selectedAction) => HighlightSelectionInList(selectedAction, actionTexts);

    /// <summary>
    /// Highlights the correct move selection.
    /// </summary>
    /// <param name="selectedMove">The currently selected move.</param>
    /// <param name="uniteonMove">The move that is selected.</param>
    public void UpdateMoveSelection(int selectedMove, Move uniteonMove)
    {
        powerPointsText.text = $"PP {uniteonMove.PowerPoints}/{uniteonMove.MoveBase.PowerPoints}";
        typeText.text = uniteonMove.MoveBase.MoveType.ToString();
        HighlightSelectionInList(selectedMove, moveTexts);
        // Change colour of PP text depending on PP amount
        if (uniteonMove.PowerPoints <= 0)
            powerPointsText.color = noPPColour;
        else if ((float)uniteonMove.PowerPoints / uniteonMove.MoveBase.PowerPoints <= 0.5)
            powerPointsText.color = lowPPColour;
        else
            powerPointsText.color = deselectedColour;
    }
    
    /// <summary>
    /// Highlights the correct selection in some texts list.
    /// </summary>
    /// <param name="selectedText">The selected text.</param>
    /// <param name="textsList">The list that the selection has to take place in.</param>
    private void HighlightSelectionInList(int selectedText, List<Text> textsList)
    {
        for (var i = 0; i < textsList.Count; i++)
        {
            textsList[i].color = i == selectedText ? selectedColour : deselectedColour;
        }
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
