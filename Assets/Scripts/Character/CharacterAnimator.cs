using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is responsible for animating any character in the Uniteon game.
/// </summary>
public class CharacterAnimator : MonoBehaviour
{
    // Fields
    [SerializeField] private Sprite[] walkUpSprites;
    [SerializeField] private Sprite[] walkDownSprites;
    [SerializeField] private Sprite[] walkLeftSprites;
    [SerializeField] private Sprite[] walkRightSprites;
    private SpriteRenderer _spriteRenderer;
    private SpriteAnimator _currentAnimation;
    private bool _wasPreviouslyMoving;
    
    // Properties
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    
    // States
    private SpriteAnimator _walkUpAnimation;
    private SpriteAnimator _walkDownAnimation;
    private SpriteAnimator _walkLeftAnimation;
    private SpriteAnimator _walkRightAnimation;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _walkUpAnimation = new SpriteAnimator(walkUpSprites, _spriteRenderer);
        _walkDownAnimation = new SpriteAnimator(walkDownSprites, _spriteRenderer);
        _walkLeftAnimation = new SpriteAnimator(walkLeftSprites, _spriteRenderer);
        _walkRightAnimation = new SpriteAnimator(walkRightSprites, _spriteRenderer);
        _currentAnimation = _walkDownAnimation;
    }

    /// <summary>
    /// Detect movements and then set the correct sprites.
    /// </summary>
    private void Update()
    {
        var previousAnimation = _currentAnimation;
        if (MoveX == 1)
            _currentAnimation = _walkRightAnimation;
        else if (MoveX == -1)
            _currentAnimation = _walkLeftAnimation;
        else if (MoveY == 1)
            _currentAnimation = _walkUpAnimation;
        else if (MoveY == -1)
            _currentAnimation = _walkDownAnimation;
        if (_currentAnimation != previousAnimation || IsMoving != _wasPreviouslyMoving)
            _currentAnimation.StartAnimation();
        if (IsMoving)
            _currentAnimation.HandleUpdate();
        else
            _spriteRenderer.sprite = _currentAnimation.Sprites[0];
        _wasPreviouslyMoving = IsMoving; // Fix for sliding NPCs with short input
    }
}
