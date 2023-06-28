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
    [SerializeField] private bool isGamer;
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
        healthBar.SetHealthBar((float)uniteon.HealthPoints / uniteon.MaxHealthPoints, isGamer); // Normalize health points
    }

    /// <summary>
    /// Updates new health points to the health bar, and if available, health text.
    /// </summary>
    /// <param name="currentHealthPoints">The health points the Uniteon had before taking damage.</param>
    public IEnumerator UpdateHealthPoints(float currentHealthPoints)
    {
        var updateCoroutines = new Coroutine[]
        {
            StartCoroutine(UpdateHealthText(currentHealthPoints)),
            StartCoroutine(UpdateHealthBar())
        };
        foreach (var coroutine in updateCoroutines)
        {
            yield return coroutine;
        }
        if (isGamer)
            healthBar.CalculateFlashHealthBorder((float)_uniteon.HealthPoints / _uniteon.MaxHealthPoints);
    }

    /// <summary>
    /// Updates the health text in a smooth manner.
    /// </summary>
    /// <param name="currentHealthPoints">The health points the Uniteon had before taking damage.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator UpdateHealthText(float currentHealthPoints)
    {
        if (healthText != null) // Only assign health text if there is place for it (only for the gamer, not for the foe)
        {
            float changeAmount = currentHealthPoints - _uniteon.HealthPoints; // Find the amount that has to be changed
            while (currentHealthPoints - _uniteon.HealthPoints > Mathf.Epsilon) // Change the HP by a very small amount
            {
                currentHealthPoints -= changeAmount * Time.deltaTime;
                healthText.text = $"{Mathf.Round(currentHealthPoints)}/{_uniteon.MaxHealthPoints}";
                yield return null;
            }
            healthText.text = $"{_uniteon.HealthPoints}/{_uniteon.MaxHealthPoints}";
        }
    }
    
    /// <summary>
    /// Updates the health bar in a smooth manner.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator UpdateHealthBar()
    {
        yield return healthBar.SetHealthBarSmoothly((float)_uniteon.HealthPoints / _uniteon.MaxHealthPoints); // Normalize health points
    }
}
