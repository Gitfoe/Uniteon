using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObjectsSpawner : MonoBehaviour
{
    [SerializeField] private GameObject baseObjectsPrefab;

    // Spawns the BaseObjects prefab if it's not already active
    private void Awake()
    {
        BaseObjects[] existingObjects = FindObjectsOfType<BaseObjects>();
        if (existingObjects.Length == 0)
            Instantiate(baseObjectsPrefab, new Vector3(0f, 0f, 0f), Quaternion.identity); // No rotation
    }
}
