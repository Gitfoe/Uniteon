using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObjects : MonoBehaviour
{
    /// <summary>
    /// Ensures that this GameObject doesn't get destroyed when switching scenes.
    /// </summary>
    private void Awake() => DontDestroyOnLoad(gameObject);
}
