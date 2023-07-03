using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    // Fields
    [SerializeField] private Text messageText;
    private PartyMemberUI[] _partyMemberSlots;
    private List<Uniteon> _uniteons;

    public void InitialisePartyScreen() => _partyMemberSlots = GetComponentsInChildren<PartyMemberUI>();

    /// <summary>
    /// Adds all the Uniteon's in a gamer's party to the party slots.
    /// </summary>
    /// <param name="uniteons">List of gamer Uniteon's.</param>
    public void AddUniteonsToPartySlots(List<Uniteon> uniteons)
    {
        _uniteons = uniteons;
        for (int i = 0; i < _partyMemberSlots.Length; i++)
        {
            if (i < uniteons.Count)
                _partyMemberSlots[i].SetGamerData(uniteons[i]);
            else
                _partyMemberSlots[i].gameObject.SetActive(false);
        }
        messageText.text = "Choose a Uniteon";
    }

    public void UpdateMemberSelection(int selectedMember)
    {
        for (int i = 0; i < _uniteons.Count; i++)
            _partyMemberSlots[i].HighlightSelected(i == selectedMember);
    }

    public void SetMessageText(string text) => messageText.text = text;

    public void ResetAllPartyMemberSlots()
    {
        foreach (var slot in _partyMemberSlots)
        {
            slot.gameObject.SetActive(true);
        }
    }
}
