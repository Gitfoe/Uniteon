using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Animate any GameObject with an image/sprite renderer by looping through the sprites.
/// </summary>
public class SpriteAnimator
{
    // Fields
    private readonly Sprite[] _sprites;
    private readonly SpriteRenderer _spriteRenderer;
    private readonly Image _image;
    private readonly float _framerate;
    private int _currentFrame;
    private float _timer;
    
    // Properties
    public Sprite[] Sprites => _sprites;

    /// <summary>
    /// Animates multiple frames of a sprite.
    /// </summary>
    /// <param name="sprites">The list of sprites you want to animate.</param>
    /// <param name="spriteRenderer">The GameObject with a SpriteRenderer component where you want your sprite to be animated.</param>
    /// <param name="framerate">The framerate of your animation.</param>
    public SpriteAnimator(Sprite[] sprites, SpriteRenderer spriteRenderer, float framerate = 0.16f)
    {
        _sprites = sprites;
        _spriteRenderer = spriteRenderer;
        _framerate = framerate;
    }
    
    /// <summary>
    /// Animates multiple frames of a sprite.
    /// </summary>
    /// <param name="sprites">The list of sprites you want to animate.</param>
    /// <param name="image">The GameObject with a Image component where you want your sprite to be animated.</param>
    /// <param name="framerate">The framerate of your animation.</param>
    public SpriteAnimator(Sprite[] sprites, Image image, float framerate = 0.16f)
    {
        _sprites = sprites;
        _image = image;
        _framerate = framerate;
    }

    public void StartAnimation()
    {
        _currentFrame = 1;
        _timer = 0f;
        if (_image == null)
            _spriteRenderer.sprite = _sprites[1];
        else
            _image.sprite = _sprites[1];
    }

    public void HandleUpdate()
    {
        _timer += Time.deltaTime;
        if (_timer > _framerate)
        {
            _currentFrame = (_currentFrame + 1) % _sprites.Length;
            if (_image == null)
                _spriteRenderer.sprite = _sprites[_currentFrame];
            else
                _image.sprite = _sprites[_currentFrame];
            _timer -= _framerate;
        }
    }
}
