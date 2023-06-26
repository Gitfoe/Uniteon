using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Uniteon", menuName = "Uniteon/Create new Uniteon")]
public class UniteonBase : ScriptableObject
{
    // Fields
    [SerializeField] private int uniteonID;
    [SerializeField] private string uniteonName;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private UniteonType uniteonType1; // Temporarily one sprite
    [SerializeField] private UniteonType uniteonType2;
    [SerializeField] private int maxHealthPoints;
    [SerializeField] private int attack;
    [SerializeField] private int defense;
    [SerializeField] private int specialAttack;
    [SerializeField] private int specialDefense;
    [SerializeField] private int speed;
    [SerializeField] private List<LearnableMove> learnableMoves;
    
    // Properties
    public int UniteonID => uniteonID;
    public string UniteonName => uniteonName;
    public string Description => description;
    public Sprite FrontSprite => frontSprite;
    public Sprite BackSprite => backSprite;
    public UniteonType UniteonType1 => uniteonType1;
    public UniteonType UniteonType2 => uniteonType2;
    public int MaxHealthPoints => maxHealthPoints;
    public int Attack => attack;
    public int Defense => defense;
    public int SpecialAttack => specialAttack;
    public int SpecialDefense => specialDefense;
    public int Speed => speed;
    public List<LearnableMove> LearnableMoves => learnableMoves;
}

/// <summary>
/// Struct that holds a learnable move and at which level it is earned by leveling up.
/// </summary>
[System.Serializable]
public struct LearnableMove
{
    [SerializeField] private MoveBase moveBase;
    [SerializeField] private int level;
    public MoveBase MoveBase { get; set; }
    public int Level { get; set; }
}

public enum UniteonType
{
    None,
    Normeon,
    Flamiteon,
    Aquoreon,
    Herbeon,
    Voltineon,
    Battleon,
    Venomeon,
    Terreon,
    Aeroeon,
    Psycoeon,
    Buggeon,
    Stondeon,
    Spectreon,
    Shadoweon,
    Dracineon,
    Ironeon,
    Faerieon
}
