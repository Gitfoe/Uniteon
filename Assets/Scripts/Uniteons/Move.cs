using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// C# class that holds a move and it's PP.
/// </summary>
public class Move
{
    // Fields
    public MoveBase MoveBase { get; set; }
    public int PowerPoints { get; set; }

    // Constructor
    public Move(MoveBase moveBase)
    {
        MoveBase = moveBase;
        PowerPoints = moveBase.PowerPoints;
    }
}
