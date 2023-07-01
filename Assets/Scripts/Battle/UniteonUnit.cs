using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.Serialization;

public class UniteonUnit : MonoBehaviour
{
    // Fields
    [SerializeField] private bool isGamerUniteon; // To determine if the Uniteon is the gamer's or the foe's
    [SerializeField] private UniteonHud uniteonHud;
    private Image _sprite;
    private Vector3 _originalPosSprite;
    private Color _originalColorSprite;
    private Coroutine _spriteAnimation;

    // Properties
    public Uniteon Uniteon { get; set; }
    public bool IsGamerUniteon => isGamerUniteon;
    public UniteonHud UniteonHud => uniteonHud;
    
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
    public void InitialiseUniteonUnit(Uniteon uniteon)
    {
        Uniteon = uniteon;
        if (_spriteAnimation != null)
            StopCoroutine(_spriteAnimation);
        _spriteAnimation = StartCoroutine(isGamerUniteon
            ? PlaySpriteAnimation(Uniteon.UniteonBase.BackSprite)
            : PlaySpriteAnimation(Uniteon.UniteonBase.FrontSprite));
        uniteonHud.gameObject.SetActive(true);
        uniteonHud.SetGamerData(uniteon);
        _sprite.color = _originalColorSprite;
        PlayBattleEnterAnimation();
    }

    public void DisableHud() => uniteonHud.gameObject.SetActive(false);
    
    #region Animations
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
        _sprite.rectTransform.localPosition = isGamerUniteon ? new Vector3(-500f, _originalPosSprite.y) : new Vector3( 500f, _originalPosSprite.y);
        _sprite.rectTransform.DOLocalMoveX(_originalPosSprite.x, 1.27f);
    }
    
    /// <summary>
    /// Plays the battle leave animation.
    /// </summary>
    public void PlayBattleLeaveAnimation()
    {
        if (isGamerUniteon)
            _sprite.rectTransform.DOLocalMoveX(_originalPosSprite.x - 500f, 1.27f);
        else
            _sprite.rectTransform.DOLocalMoveX(_originalPosSprite.x + 500f, 1.27f);
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
    /// <param name="effectiveness">The higher the effectiveness, the more the hit animation flashes</param>
    public void PlayHitAnimation(float effectiveness)
    {
        int loops = effectiveness switch
        {
            > 2f => 4,
            > 1f => 3,
            < 1f => 1,
            _ => 2
        };
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_sprite.DOColor(Color.gray, 0.07f));
        sequence.Append(_sprite.DOColor(_originalColorSprite, 0.07f));
        sequence.SetLoops(loops);
    }

    /// <summary>
    /// Plays a fainting animation when an Uniteon faints.
    /// </summary>
    public IEnumerator PlayFaintAnimation()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(_sprite.transform.DOLocalMoveY(_originalPosSprite.y - 150f, 0.35f));
        sequence.Join(_sprite.DOFade(0f, 0.35f));
        yield return new WaitForSeconds(2f);
    }
    
    /// <summary>
    /// Plays the Uniteon stat raised animation.
    /// </summary>
    /// <param name="raised">If it needs to show the raise (true) or fall (false) animation.</param>
    public void PlayStatRaisedAnimation(bool raised)
    {
        Sequence sequence = DOTween.Sequence();
        if (raised)
        {
            // Raise animation
            sequence.Append(_sprite.DOColor(new Color(0, 1, 0), 1.25f));
            sequence.Join(_sprite.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 1.25f));
        }
        else
        {
            // Fall animation
            sequence.Append(_sprite.DOColor(new Color(0.5f, 0.5f, 0.5f), 1.25f));
            sequence.Join(_sprite.transform.DOScale(new Vector3(0.9f, 0.9f, 0.9f), 1.25f));
        }
        // Restore color and scale
        sequence.Append(_sprite.DOColor(_originalColorSprite, 0.25f));
        sequence.Join(_sprite.transform.DOScale(Vector3.one, 0.25f));
    }
    #endregion
}
