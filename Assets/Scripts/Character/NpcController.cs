using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    // Fields
    [SerializeField] private Dialog dialog;
    private Character _character;

    private void Start()
    {
        _character = GetComponent<Character>();
    }

    private void Update()
    {
        _character.HandleUpdate();
    }

    /// <summary>
    /// As the NPC, speak to the dialog box.
    /// </summary>
    public void Interact()
    {
        // StartCoroutine(DialogManager.Instance.PrintDialog(dialog));
        StartCoroutine(_character.Move(new Vector2(0, 4)));
    }
}
