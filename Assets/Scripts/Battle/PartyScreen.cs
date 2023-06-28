using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyScreen : MonoBehaviour
{
    // Fields
    [SerializeField] private Text messageText;
    private PartyMemberUI[] _partyMemberSlots;

    public void InitialisePartyScreen() => _partyMemberSlots = GetComponentsInChildren<PartyMemberUI>();

    /// <summary>
    /// Adds all the Uniteon's in a gamer's party to the party slots.
    /// </summary>
    /// <param name="uniteons">List of gamer Uniteon's.</param>
    public void AddUniteonsToPartySlots(List<Uniteon> uniteons)
    {
        for (int i = 0; i < _partyMemberSlots.Length; i++)
        {
            if (i < uniteons.Count)
                _partyMemberSlots[i].SetGamerData(uniteons[i]);
            else
                _partyMemberSlots[i].gameObject.SetActive(false);
        }
        messageText.text = "Choose a Uniteon";
    }
}
