using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class SavableEntity : MonoBehaviour
{
    [SerializeField] string uniqueId = "";
    private static Dictionary<string, SavableEntity> _globalLookup = new Dictionary<string, SavableEntity>();

    public string UniqueId => uniqueId;

    // Used to capture state of the game object on which the savableEntity is attached
    public object CaptureState()
    {
        Dictionary<string, object> state = new Dictionary<string, object>();
        foreach (ISavable savable in GetComponents<ISavable>())
        {
            state[savable.GetType().ToString()] = savable.CaptureState();
        }
        return state;
    }

    // Used to restore state of the game object on which the savableEntity is attached
    public void RestoreState(object state)
    {
        Dictionary<string, object> stateDict = (Dictionary<string, object>)state;
        foreach (ISavable savable in GetComponents<ISavable>())
        {
            string id = savable.GetType().ToString();

            if (stateDict.ContainsKey(id))
                savable.RestoreState(stateDict[id]);
        }
    }

#if UNITY_EDITOR
    // Update method used for generating UUID of the SavableEntity
    private void Update()
    {
        // Don't execute in playmode
        if (Application.IsPlaying(gameObject)) return;

        // Don't generate Id for prefabs (prefab scene will have path as null)
        if (String.IsNullOrEmpty(gameObject.scene.path)) return;

        SerializedObject serializedObject = new SerializedObject(this);
        SerializedProperty property = serializedObject.FindProperty("uniqueId");

        if (String.IsNullOrEmpty(property.stringValue) || !IsUnique(property.stringValue))
        {
            property.stringValue = Guid.NewGuid().ToString();
            serializedObject.ApplyModifiedProperties();
        }

        _globalLookup[property.stringValue] = this;
    }
#endif

    private bool IsUnique(string candidate)
    {
        if (!_globalLookup.ContainsKey(candidate)) return true;

        if (_globalLookup[candidate] == this) return true;

        // Handle scene unloading cases
        if (_globalLookup[candidate] == null)
        {
            _globalLookup.Remove(candidate);
            return true;
        }

        // Handle edge cases like designer manually changing the UUID
        if (_globalLookup[candidate].UniqueId != candidate)
        {
            _globalLookup.Remove(candidate);
            return true;
        }

        return false;
    }
}