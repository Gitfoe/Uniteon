using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcController : MonoBehaviour, Interactable
{
    [SerializeField] private Dialog dialog;

    /// <summary>
    /// As the NPC, speak to the dialog box.
    /// </summary>
    public void Interact() => StartCoroutine(DialogManager.Instance.PrintDialog(dialog));
}
