using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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
    [SerializeField] private FacingCardinal initialCardinal = FacingCardinal.South;
    private SpriteRenderer _spriteRenderer;
    private SpriteAnimator _currentAnimation;
    private bool _wasPreviouslyMoving;
    
    // Properties
    public float MoveX { get; set; }
    public float MoveY { get; set; }
    public bool IsMoving { get; set; }
    public FacingCardinal InitialCardinal => initialCardinal;
    
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
        SetFacingCardinal(initialCardinal);
    }

    /// <summary>
    /// Detect movements and then set the correct sprites.
    /// </summary>
    private void Update()
    {
        var previousAnimation = _currentAnimation;
        if ((int)MoveX == 1)
            _currentAnimation = _walkRightAnimation;
        else if ((int)MoveX == -1)
            _currentAnimation = _walkLeftAnimation;
        else if ((int)MoveY == 1)
            _currentAnimation = _walkUpAnimation;
        else if ((int)MoveY == -1)
            _currentAnimation = _walkDownAnimation;
        if (_currentAnimation != previousAnimation || IsMoving != _wasPreviouslyMoving)
            _currentAnimation.StartAnimation();
        if (IsMoving)
            _currentAnimation.HandleUpdate();
        else
            _spriteRenderer.sprite = _currentAnimation.Sprites[0];
        _wasPreviouslyMoving = IsMoving; // Fix for sliding NPCs with short input
    }

    /// <summary>
    /// Sets the MoveX and MoveY parameters depending on the facing cardinal.
    /// </summary>
    /// <param name="cardinal">Facing cardinal.</param>
    private void SetFacingCardinal(FacingCardinal cardinal)
    {
        switch (cardinal)
        {
            case FacingCardinal.North:
                MoveY = 1;
                break;
            case FacingCardinal.East:
                MoveX = 1;
                break;
            case FacingCardinal.South:
                MoveY = -1;
                break;
            case FacingCardinal.West:
                MoveX = -1;
                break;
        }
    }
}

public enum FacingCardinal
{
    North,
    East,
    South,
    West
}
