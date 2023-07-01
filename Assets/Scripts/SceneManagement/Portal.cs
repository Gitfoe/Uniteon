using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour, IPlayerTriggerable
{
    public void OnPlayerTriggered(GamerController gamer)
    {
        Debug.Log("Gamer entered portal");
    }
}
