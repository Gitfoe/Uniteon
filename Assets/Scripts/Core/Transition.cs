using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Transition : MonoBehaviour
{
    private Image _transition;

    private void Awake() => _transition = GetComponent<Image>();

    /// <summary>
    /// Fades in the overworld transition.
    /// </summary>
    /// <param name="fadeTime">The time it takes to fade in.</param>
    /// <param name="fadeColor">The color of the fade.</param>
    /// <param name="onComplete">Event that gets invoked once completed.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator FadeIn(float fadeTime, Color fadeColor, Action onComplete = null)
    {
        Color colour = fadeColor;
        _transition.color = new Color(colour.r, colour.g, colour.b, 0f);
        yield return _transition.DOFade(1f, fadeTime).SetEase(Ease.Linear).WaitForCompletion();
        onComplete?.Invoke();
    }

    /// <summary>
    /// Fades in the overworld transition.
    /// </summary>
    /// <param name="fadeTime">The time it takes to fade in.</param>
    /// <param name="waitTime">The time the function waits to start fading in.</param>
    /// <param name="fadeColor">The color of the fade.</param>
    /// <param name="onComplete">Event that gets invoked once completed.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator FadeIn(float fadeTime, float waitTime, Color fadeColor, Action onComplete = null)
    {
        yield return new WaitForSeconds(waitTime);
        yield return FadeIn(fadeTime, fadeColor, onComplete);
    }

    /// <summary>
    /// Fades out the overworld transition.
    /// </summary>
    /// <param name="fadeTime">The time it takes to fade out.</param>
    /// <param name="fadeColor">The color of the fade.</param>
    /// <param name="onComplete">Event that gets invoked once completed.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator FadeOut(float fadeTime, Color fadeColor, Action onComplete = null)
    {
        Color colour = fadeColor;
        _transition.color = new Color(colour.r, colour.g, colour.b, 1f);
        yield return _transition.DOFade(0f, fadeTime).SetEase(Ease.Linear).WaitForCompletion();
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Fades out the overworld transition.
    /// </summary>
    /// <param name="fadeTime">The time it takes to fade out.</param>
    /// <param name="waitTime">The time the function waits to start fading out.</param>
    /// <param name="fadeColor">The color of the fade.</param>
    /// <param name="onComplete">Event that gets invoked once completed.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator FadeOut(float fadeTime, float waitTime, Color fadeColor, Action onComplete = null)
    {
        yield return new WaitForSeconds(waitTime);
        yield return FadeOut(fadeTime, fadeColor);
        onComplete?.Invoke();
    }
    
    public void SetTransitionColor(Color color) => _transition.color = color;
}
