using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    // Fields
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text healthText;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Color selectedColour;
    [SerializeField] private Color deselectedColour;
    private Uniteon _uniteon;

    /// <summary>
    /// Sets the data for the Uniteon's in the party slot.
    /// </summary>
    /// <param name="uniteon">The Uniteon that needs to be set.</param>
    public void SetGamerData(Uniteon uniteon)
    {
        _uniteon = uniteon;
        nameText.text = uniteon.UniteonBase.UniteonName;
        levelText.text = $"Lv.{uniteon.Level}";
        healthText.text = $"{uniteon.HealthPoints}/{uniteon.MaxHealthPoints}";
        healthBar.SetHealthBar((float)uniteon.HealthPoints / uniteon.MaxHealthPoints); // Normalize health points
    }

    public void HighlightSelected(bool selected)
    {
        nameText.color = selected ? selectedColour : deselectedColour;
    }
}