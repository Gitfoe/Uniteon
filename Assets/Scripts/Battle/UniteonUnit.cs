using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class UniteonUnit : MonoBehaviour
{
    // Fields
    [SerializeField] private UniteonBase uniteonBase;
    [SerializeField] private int level;
    [SerializeField] private bool isGamerUniteon; // To determine if the Uniteon is the gamer's or the foe's
    private Animator _animator;
    private AnimatorOverrideController _animatorOverrideController;

    // Properties
    public Uniteon Uniteon { get; set; }
    
    // Awake is called when the script is loaded
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animatorOverrideController = new AnimatorOverrideController
        {
            runtimeAnimatorController = _animator.runtimeAnimatorController
        };
    }

    /// <summary>
    /// Creates a new Uniteon object and sets the correct image.
    /// </summary>
    public void InitialiseUniteon()
    {
        Uniteon = new Uniteon(uniteonBase, level);
        if (isGamerUniteon)
            _animatorOverrideController["missingno"] = Uniteon.UniteonBase.BackSprite;
        else
            _animatorOverrideController["missingno"] = Uniteon.UniteonBase.FrontSprite;
        _animator.runtimeAnimatorController = _animatorOverrideController;
    }
}
