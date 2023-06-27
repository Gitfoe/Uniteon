using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    // Fields
    [SerializeField] private bool isGamer;
    [SerializeField] private Image health;
    [SerializeField] private Image healthBorder;
    [SerializeField] private Color healthColourHalf;
    [SerializeField] private Color healthColourLow;
    [SerializeField] private Color endFlashColour;
    [SerializeField] private float flashDuration;
    private Coroutine _flashCoroutine;
    private Color _originalHealthColor;
    private Color _startFlashColour;

    /// <summary>
    /// Initialises variables.
    /// </summary>
    private void Awake()
    {
        _originalHealthColor = health.color;
        _startFlashColour = healthBorder.color;
    }
    
    /// <summary>
    /// Sets the health bar to a new value and (re)sets the colors.
    /// </summary>
    /// <param name="normalizedHealthPoints">The normalized value of HP.</param>
    public void SetHealthBar(float normalizedHealthPoints)
    {
        health.transform.localScale = new Vector3(normalizedHealthPoints, 1f);
        health.color = _originalHealthColor;
        healthBorder.color = _startFlashColour;
    }

    /// <summary>
    /// Sets the health bar to a new value in a smooth manner.
    /// </summary>
    /// <param name="newHealthPoints">The normalized new HP value after damage.</param>
    /// <param name="maxHealthPoints">The maximum health points of the Uniteon.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator SetHealthBarSmoothly(float newHealthPoints)
    {
        float currentHealthPoints = health.transform.localScale.x; // Get the current health bar HP
        float changeAmount = currentHealthPoints - newHealthPoints; // Find the amount that has to be changed
        while (currentHealthPoints - newHealthPoints > Mathf.Epsilon) // Change the HP by a very small amount
        {
            currentHealthPoints -= changeAmount * Time.deltaTime;
            health.transform.localScale = new Vector3(currentHealthPoints, 1f);
            // Change colour of health bar depending on HP
            if (currentHealthPoints <= 0.2)
                health.color = healthColourLow;
            else if (currentHealthPoints <= 0.5)
                health.color = healthColourHalf;
            yield return null;
        }
        health.transform.localScale = new Vector3(newHealthPoints, 1f); // After the coroutine has been completed, set to new HP
    }
    
    /// <summary>
    /// Flashes the health border to indicate low health.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator FlashHealthBorder()
    {
        Color startColor = _startFlashColour;
        Color endColor = endFlashColour;
        while (true)
        {
            float t = 0f;
            while (t < flashDuration)
            {
                t += Time.deltaTime;
                float normalizedTime = t / flashDuration;
                // Calculate in between colors with Lerp
                Color currentColor = Color.Lerp(startColor, endColor, normalizedTime);
                healthBorder.color = currentColor;
                yield return null;
            }
            // Swap start and end colors for the next iteration
            (startColor, endColor) = (endColor, startColor);
        }
    }
    
    /// <summary>
    /// Enables or disables the flashing of the health border.
    /// </summary>
    /// <param name="start">True for on and false for off.</param>
    public void SetFlashingHealthBorder(bool start)
    {
        if (!isGamer) return;
        switch (start)
        {
            case true when _flashCoroutine == null:
                _flashCoroutine = StartCoroutine(FlashHealthBorder());
                break;
            case false when _flashCoroutine != null:
                StopCoroutine(_flashCoroutine);
                healthBorder.color = _startFlashColour;
                _flashCoroutine = null;
                break;
        }
    }
}
