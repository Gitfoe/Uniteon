using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UniteonUnit : MonoBehaviour
{
    // Fields
    [SerializeField] private UniteonBase uniteonBase;
    [SerializeField] private int level;
    [SerializeField] private bool isGamerUniteon; // To determine if the Uniteon is the gamer's or the foe's
    private Image _sprite;
    private Vector3 _originalPosSprite;
    private Color _originalColorSprite;

    // Properties
    public Uniteon Uniteon { get; set; }
    
    /// <summary>
    /// Initialise variables.
    /// </summary>
    private void Awake()
    {
        _sprite = GetComponent<Image>();
        _originalPosSprite = _sprite.rectTransform.localPosition;
        _originalColorSprite = _sprite.color;
    }

    /// <summary>
    /// Creates a new Uniteon object and sets the correct image.
    /// </summary>
    public void InitialiseUniteon()
    {
        Uniteon = new Uniteon(uniteonBase, level);
        StartCoroutine(isGamerUniteon
            ? PlaySpriteAnimation(Uniteon.UniteonBase.BackSprite)
            : PlaySpriteAnimation(Uniteon.UniteonBase.FrontSprite));
        PlayBattleEnterAnimation();
        _sprite.color = _originalColorSprite;
    }
    
    /// <summary>
    /// Starts playing the sprite of the Uniteon.
    /// </summary>
    /// <param name="sprites">The list of individual sprite frames.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator PlaySpriteAnimation(Sprite[] sprites)
    {
        int currentSpriteIndex = 0;
        float spriteFramerate = 0.1f;
        while (true)
        {
            _sprite.sprite = sprites[currentSpriteIndex];
            currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Length;
            // Slow down the framerate if HP is below 20%
            if (spriteFramerate >= 0.1f && (float)Uniteon.HealthPoints / Uniteon.MaxHealthPoints <= 0.2f)
                spriteFramerate = 0.2f;
            yield return new WaitForSeconds(spriteFramerate);
        }
    }

    /// <summary>
    /// Plays the battle enter animation.
    /// </summary>
    private void PlayBattleEnterAnimation()
    {
        if (isGamerUniteon)
            _sprite.rectTransform.localPosition = new Vector3(-500f, _originalPosSprite.y);
        else
            _sprite.rectTransform.localPosition = new Vector3( 500f, _originalPosSprite.y);
        _sprite.rectTransform.DOLocalMoveX(_originalPosSprite.x, 1.27f);
    }

    /// <summary>
    /// Plays the attack animations.
    /// </summary>
    /// <returns></returns>
    public IEnumerator PlayAttackAnimations()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(isGamerUniteon
            ? _sprite.transform.DOLocalMove(new Vector3(_originalPosSprite.x + 50f, _originalPosSprite.y + 25f), 0.27f)
            : _sprite.transform.DOLocalMove(new Vector3(_originalPosSprite.x - 50f, _originalPosSprite.y - 25f), 0.27f));
        sequence.Append(_sprite.transform.DOLocalMove(_originalPosSprite, 0.27f));
        yield return new WaitForSeconds(0.72f);
    }

    /// <summary>
    /// Plays the Uniteon hit animation.
    /// </summary>
    public void PlayHitAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_sprite.DOColor(Color.gray, 0.07f));
        sequence.Append(_sprite.DOColor(_originalColorSprite, 0.07f));
        sequence.SetLoops(2);
    }

    /// <summary>
    /// Plays a fainting animation when an Uniteon faints.
    /// </summary>
    public void PlayFaintAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_sprite.transform.DOLocalMoveY(_originalPosSprite.y - 150f, 0.35f));
        sequence.Join(_sprite.DOFade(0f, 0.35f));
    }
}
