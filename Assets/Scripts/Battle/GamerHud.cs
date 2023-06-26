using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamerHud : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private HealthBar healthBar;

    /// <summary>
    /// Sets the data for the gamer's Uniteon.
    /// </summary>
    /// <param name="uniteon">The Uniteon that needs to be drawn to the screen.</param>
    public void SetGamerData(Uniteon uniteon)
    {
        nameText.text = uniteon.UniteonBase.UniteonName;
        levelText.text = $"Lv.{uniteon.Level}";
        healthBar.SetGamerHealthBar((float)uniteon.HealthPoints / uniteon.MaxHealthPoints); // Normalize health points
    }
}
