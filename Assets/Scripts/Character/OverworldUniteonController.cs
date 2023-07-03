using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class OverworldUniteonController : MonoBehaviour, Interactable
{
    // Fields
    [SerializeField] protected Dialog dialog;
    [SerializeField] private string uniteonName;
    [SerializeField] protected GameObject fov;
    [SerializeField] private AudioClip battleIntro;
    [SerializeField] private AudioClip battleLoop;
    [SerializeField] private Sprite[] overworldSprites;
    private SpriteRenderer _spriteRenderer;
    private SpriteAnimator _spriteAnimator;
    private bool _battleLost;
    
    // Properties
    public string UniteonName => uniteonName;
    public bool BattleLost => _battleLost;
    public AudioClip BattleIntro => battleIntro;
    public AudioClip BattleLoop => battleLoop;

    // Events
    public event Action OnInitiateOverworldUniteonBattle;
    
    /// <summary>
    /// Initialises vars in components and starts the overworld animation.
    /// </summary>
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteAnimator = new SpriteAnimator(overworldSprites, _spriteRenderer, 0.4f);
        _spriteAnimator.StartAnimation();
    }

    /// <summary>
    /// Updates the animation.
    /// </summary>
    private void Update()
    {
        _spriteAnimator.HandleUpdate();
    }

    /// <summary>
    /// Gets triggered once the gamer enters the Uniteon's FOV.
    /// </summary>
    /// <param name="gamer">The controller of the gamer that interacted with this Uniteon.</param>
    /// <returns>Coroutine.</returns>
    public virtual IEnumerator TriggerOverworldUniteonBattle(GamerController gamer = null)
    {
        Debug.Log($"Overworld Uniteon battle triggered: {UniteonName}");
        yield return DialogManager.Instance.PrintDialog(dialog, OnInitiateOverworldUniteonBattle);
    }
    
    /// <summary>
    /// Faces the Uniteon to another direction and changes it's FOV rotation as well.
    /// </summary>
    /// <param name="initiator">The transform of the initiator.</param>
    public void Interact(Transform initiator)
    {
        StartCoroutine(DialogManager.Instance.PrintDialog(dialog));
    }
    
    public void OnBattleOver()
    {
        fov.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Gets fired once the transition animation is done by the gamer.
    /// </summary>
    public void InitiateOverworldUniteonBattle() => GameController.Instance.InitiateBattle(null, this);
}
