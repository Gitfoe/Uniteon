using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveSelectionScreen : MonoBehaviour
{
    [SerializeField] private List<Text> moveNames;
    [SerializeField] private Color selectedColour;
    [SerializeField] private Color deselectedColour;
    [SerializeField] private Color deselectedColourNewMove;

    /// <summary>
    /// Set move names to the UI.
    /// </summary>
    /// <param name="currentMoves">The moves the Uniteon knows.</param>
    /// <param name="newMove">The new move it can learn.</param>
    public void SetMoveNames(List<MoveBase> currentMoves, MoveBase newMove)
    {
        for (int i = 0; i < currentMoves.Count; i++)
        {
            moveNames[i].text = currentMoves[i].MoveName;
        }
        moveNames[currentMoves.Count].text = newMove.MoveName;
    }
    
    /// <summary>
    /// Highlights the correct selection in the list.
    /// </summary>
    /// <param name="selectedText">The selected text.</param>
    /// <param name="textsList">The list that the selection has to take place in.</param>
    public void HighlightSelectionInList(int selectedText)
    {
        for (int i = 0; i < UniteonBase.MaxMoves + 1; i++)
        {
            moveNames[i].color = i == selectedText ? selectedColour : deselectedColour;
            if (i == UniteonBase.MaxMoves)
                moveNames[i].color = i == selectedText ? selectedColour : deselectedColourNewMove;
        }
    }
}
