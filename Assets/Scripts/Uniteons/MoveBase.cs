using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Move", menuName = "Move/Create new move")]
public class MoveBase : ScriptableObject
{
    // Fields
    [SerializeField] private int moveID;
    [SerializeField] private string moveName;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private UniteonType moveType;
    [SerializeField] private MoveCategory moveCategory;
    [SerializeField] public int powerPoints;
    [SerializeField] private int power;
    [SerializeField] private int accuracy;
    
    // Properties
    public int MoveID => moveID;
    public string MoveName => moveName;
    public string Description => description;
    public UniteonType MoveType => moveType;
    public MoveCategory MoveCategory => moveCategory;
    public int PowerPoints => powerPoints;
    public int Power => power;
    public int Accuracy => accuracy;
}

public enum MoveCategory
{
    Physical,
    Special,
    Status
}
