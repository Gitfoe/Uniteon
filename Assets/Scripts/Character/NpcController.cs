using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    // Fields
    [SerializeField] private Dialog dialog;
    [SerializeField] private Sprite[] sprites;
    private SpriteAnimator _spriteAnimator;

    private void Start()
    {
        _spriteAnimator = new SpriteAnimator(sprites, GetComponent<SpriteRenderer>());
        _spriteAnimator.StartAnimation();
    }

    private void Update()
    {
        _spriteAnimator.HandleUpdate();
    }

    /// <summary>
    /// As the NPC, speak to the dialog box.
    /// </summary>
    public void Interact() => StartCoroutine(DialogManager.Instance.PrintDialog(dialog));
}
