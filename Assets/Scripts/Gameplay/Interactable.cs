using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for all interactable objects.
/// </summary>
public interface Interactable
{
    public void Interact(Transform initiator);
}
