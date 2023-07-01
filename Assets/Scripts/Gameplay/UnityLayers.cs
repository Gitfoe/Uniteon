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
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask fovLayer;
    [SerializeField] private LayerMask portalLayer;
    
    // Properties
    public LayerMask TriggerableLayers => wildGrassLayer | fovLayer | portalLayer;
    public LayerMask ObjectsLayer => objectsLayer;
    public LayerMask WildGrassLayer => wildGrassLayer;
    public LayerMask InteractableLayer => interactableLayer;
    public LayerMask PlayerLayer => playerLayer;
    public LayerMask FovLayer => fovLayer;
    public LayerMask PortalLayer => portalLayer;
    
    public static UnityLayers Instance { get; private set; }

    private void Awake() => Instance = this;
}
