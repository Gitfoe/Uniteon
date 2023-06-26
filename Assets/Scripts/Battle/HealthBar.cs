using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private GameObject health;
    
    /// <summary>
    /// Sets the health bar of the gamer to a new value.
    /// </summary>
    /// <param name="normalizedHealthPoints"></param>
    public void SetGamerHealthBar(float normalizedHealthPoints)
    {
        health.transform.localScale = new Vector3(normalizedHealthPoints, 1f);
    }
}
