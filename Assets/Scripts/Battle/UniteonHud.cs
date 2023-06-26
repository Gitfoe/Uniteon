using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UniteonHud : MonoBehaviour
{
    // Fields
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text healthText;
    [SerializeField] private HealthBar healthBar;
    private Uniteon _uniteon;

    /// <summary>
    /// Sets the data for the Uniteon's on the battle scene.
    /// </summary>
    /// <param name="uniteon">The Uniteon that needs to be drawn to the screen.</param>
    public void SetGamerData(Uniteon uniteon)
    {
        _uniteon = uniteon;
        nameText.text = uniteon.UniteonBase.UniteonName;
        levelText.text = $"Lv.{uniteon.Level}";
        if (healthText != null) // Only assign health text if there is place for it (only for the gamer, not for the foe)
            healthText.text = $"{uniteon.HealthPoints}/{uniteon.MaxHealthPoints}";
        healthBar.SetGamerHealthBar((float)uniteon.HealthPoints / uniteon.MaxHealthPoints); // Normalize health points
    }

    /// <summary>
    /// Updates new health points to the health bar.
    /// </summary>
    public void UpdateHealthPoints()
    {
        healthBar.SetGamerHealthBar((float)_uniteon.HealthPoints / _uniteon.MaxHealthPoints); // Normalize health points
    }
}
