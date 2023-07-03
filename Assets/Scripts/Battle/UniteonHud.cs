using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UniteonHud : MonoBehaviour
{
    // Fields
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private Text healthText;
    [SerializeField] private GameObject expBar;
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
        levelText.text = $"Lv.{_uniteon.Level}";
        if (healthText != null) // Only assign health text if there is place for it (only for the gamer, not for the foe)
            healthText.text = $"{uniteon.HealthPoints}/{uniteon.MaxHealthPoints}";
        healthBar.SetHealthBar((float)uniteon.HealthPoints / uniteon.MaxHealthPoints, isGamer); // Normalize health points
        SetExperienceBar();
    }

    /// <summary>
    /// Updates new health points to the health bar, and if available, health text.
    /// </summary>
    /// <param name="currentHealthPoints">The health points the Uniteon had before taking damage,
    /// or if you want to simply update to new values, the Uniteons current health.</param>
    /// <param name="sfx">If you want sfx to play for new health points updating. You don't want this for leveling up.</param>
    public IEnumerator UpdateHealthPoints(float currentHealthPoints, bool sfx = true)
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
            healthBar.CalculateFlashHealthBorder((float)_uniteon.HealthPoints / _uniteon.MaxHealthPoints, sfx);
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

    /// <summary>
    /// Sets the experience bar to the correct experience value.
    /// </summary>
    private void SetExperienceBar()
    {
        if (expBar == null) return;
        float normalizedExp = GetNormalizedExperienceGain();
        expBar.transform.localScale = new Vector3(normalizedExp, 1f, 1f);
    }
    
    /// <summary>
    /// Sets the experience bar in a smooth manner.
    /// </summary>
    /// <param name="reset">Resets the bar to 0 first in case of level up.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator UpdateExperienceBar(bool reset = false)
    {
        if (ReferenceEquals(expBar, null)) yield break;
        if (reset)
            expBar.transform.localScale = new Vector3(0f, 1f, 1f);
        float normalizedExp = GetNormalizedExperienceGain();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.27f).SetEase(Ease.Linear).WaitForCompletion();
        yield return new WaitForSeconds(0.72f);
    }
    
    /// <summary>
    /// Normalizes the experience gain.
    /// </summary>
    /// <returns>Experience value between 0 and 1 float.</returns>
    private float GetNormalizedExperienceGain()
    {
        int currentExp = _uniteon.UniteonBase.GetExperienceForLevel(_uniteon.Level);
        int nextExp = _uniteon.UniteonBase.GetExperienceForLevel(_uniteon.Level + 1);
        return Mathf.Clamp01((float)(_uniteon.Experience - currentExp) / (nextExp - currentExp)); // Normalize
    }

    /// <summary>
    /// Update level and health for leveling up.
    /// </summary>
    public void UpdateLevelAndHealth()
    {
        levelText.text = $"Lv.{_uniteon.Level}";
        StartCoroutine(UpdateHealthPoints(_uniteon.HealthPoints, false));
    }
}
