using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Move", menuName = "Create new move")]
public class MoveBase : ScriptableObject
{
    // Fields
    [SerializeField] private int moveID;
    [SerializeField] private string moveName;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private UniteonType moveType;
    [SerializeField] private MoveCategory moveCategory;
    [SerializeField] private int powerPoints;
    [SerializeField] private int power;
    [SerializeField] private int accuracy;
    [SerializeField] private MoveEffects moveEffects;
    [SerializeField] private MoveTarget moveTarget;
    
    // Properties
    public int MoveID => moveID;
    public string MoveName => moveName;
    public string Description => description;
    public UniteonType MoveType => moveType;
    public MoveCategory MoveCategory => moveCategory;
    public int PowerPoints => powerPoints;
    public int Power => power;
    public int Accuracy => accuracy;
    public MoveEffects MoveEffects => moveEffects;
    public MoveTarget MoveTarget => moveTarget;
}

/// <summary>
/// The stats that can be boosted by this moves, and also status conditions and weather conditions etc.
/// </summary>
[Serializable]
public class MoveEffects
{   
    [SerializeField] private List<StatBoost> boosts;
    
    public List<StatBoost> Boosts => boosts;
}

/// <summary>
/// Struct to function as a 'dictionary' because Unity doesn't serialize dictionaries.
/// </summary>
[Serializable]
public struct StatBoost
{
    [SerializeField] private Statistic stat;
    [SerializeField] private int boost;

    public Statistic Stat => stat;
    public int Boost => boost;
}

public enum MoveCategory
{
    Physical,
    Special,
    Status
}

/// <summary>
/// What unit on the field is the target for a move.
/// </summary>
public enum MoveTarget
{
    Foe, Gamer
}
