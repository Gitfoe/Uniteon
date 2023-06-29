using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Dialog
{
    [SerializeField] private List<string> dialogLines;

    public List<string> Lines => dialogLines;
}
