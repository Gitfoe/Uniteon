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
    /// <returns></returns>
    public IEnumerator FadeIn(float fadeTime, Color fadeColor)
    {
        Color colour = fadeColor;
        _transition.color = new Color(colour.r, colour.g, colour.b, 0f);
        yield return _transition.DOFade(1f, fadeTime).WaitForCompletion();
    }

    /// <summary>
    /// Fades in the overworld transition.
    /// </summary>
    /// <param name="fadeTime">The time it takes to fade in.</param>
    /// <param name="waitTime">The time the function waits to start fading in.</param>
    /// <param name="fadeColor">The color of the fade.</param>
    /// <returns></returns>
    public IEnumerator FadeIn(float fadeTime, float waitTime, Color fadeColor)
    {
        yield return new WaitForSeconds(waitTime);
        yield return FadeIn(fadeTime, fadeColor);
    }

    /// <summary>
    /// Fades out the overworld transition.
    /// </summary>
    /// <param name="fadeTime">The time it takes to fade out.</param>
    /// <param name="fadeColor">The color of the fade.</param>
    /// <returns></returns>
    public IEnumerator FadeOut(float fadeTime, Color fadeColor)
    {
        Color colour = fadeColor;
        _transition.color = new Color(colour.r, colour.g, colour.b, 1f);
        yield return _transition.DOFade(0f, fadeTime).WaitForCompletion();
    }
}
