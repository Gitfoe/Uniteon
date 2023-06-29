using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds all Unity layers, for the Singleton Design Pattern
/// </summary>
public class UnityLayers : MonoBehaviour
{
    // Layers
    [SerializeField] private LayerMask objectsLayer;
    [SerializeField] private LayerMask wildGrassLayer;
    [SerializeField] private LayerMask interactableLayer;
    
    // Properties
    public LayerMask ObjectsLayer => objectsLayer;
    public LayerMask WildGrassLayer => wildGrassLayer;
    public LayerMask InteractableLayer => interactableLayer;
    
    public static UnityLayers Instance { get; set; }

    private void Awake() => Instance = this;
}
