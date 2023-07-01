using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Uniteon", menuName = "Create new Uniteon")]
public class UniteonBase : ScriptableObject
{
    // Fields
    [SerializeField] private int uniteonID;
    [SerializeField] private string uniteonName;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private Sprite[] frontSprite;
    [SerializeField] private Sprite[] backSprite;
    [SerializeField] private UniteonType uniteonType1;
    [SerializeField] private UniteonType uniteonType2;
    [SerializeField] private int maxHealthPoints;
    [SerializeField] private int attack;
    [SerializeField] private int defense;
    [SerializeField] private int specialAttack;
    [SerializeField] private int specialDefense;
    [SerializeField] private int speed;
    [SerializeField] private int baseExperience;
    [SerializeField] private List<LearnableMove> learnableMoves;
    [SerializeField] private AudioClip cry;
    
    // Properties
    public int UniteonID => uniteonID;
    public string UniteonName => uniteonName;
    public string Description => description;
    public Sprite[] FrontSprite => frontSprite;
    public Sprite[] BackSprite => backSprite;
    public UniteonType UniteonType1 => uniteonType1;
    public UniteonType UniteonType2 => uniteonType2;
    public int MaxHealthPoints => maxHealthPoints;
    public int Attack => attack;
    public int Defense => defense;
    public int SpecialAttack => specialAttack;
    public int SpecialDefense => specialDefense;
    public int Speed => speed;
    public int BaseExperience => baseExperience;
    public List<LearnableMove> LearnableMoves => learnableMoves;
    public AudioClip Cry => cry;
}

/// <summary>
/// Struct that holds a learnable move and at which level it is earned by leveling up.
/// </summary>
[Serializable]
public struct LearnableMove
{
    [SerializeField] private MoveBase moveBase;
    [SerializeField] private int level;
    public MoveBase MoveBase => moveBase;
    public int Level => level;
}

public enum UniteonType
{
    None,
    Normeon,
    Flamiteon,
    Aquoreon,
    Voltineon,
    Herbeon,
    Frozeon,
    Battleon,
    Venomeon,
    Terreon,
    Aeroeon,
    Psycoeon,
    Buggeon,
    Stondeon,
    Spectreon,
    Dracineon,
    Shadoweon,
    Ironeon,
    Faerieon
}

public enum Statistic
{
    Attack,
    Defense,
    SpecialAttack,
    SpecialDefense,
    Speed,
    // Not actual stats, but used for checking move accuracy
    Accuracy,
    Evasion
}

/// <summary>
/// The Uniteon type effectiveness chart, heavily 'inspired' by https://pokemondb.net/type
/// </summary>
public struct EffectivenessChart
{
    private static readonly float[][] Chart =
    {
        // @formatter:off
        //              NOR  FIR  WAT  ELC  GRS  ICE  FGT  PSN  GRN  FLY  PSY  BGG  RCK  GST  DRA  DRK  STL  FRY
        /*NOR*/ new[] { 1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f, 0.5f, 0f,  1f,  1f, 0.5f, 1f },
        /*FIR*/ new[] { 1f, 0.5f,0.5f, 1f,  2f,  2f,  1f,  1f,  1f,  1f,  1f,  2f, 0.5f, 1f, 0.5f, 1f,  2f,  1f },
        /*WAT*/ new[] { 1f,  2f, 0.5f, 1f, 0.5f, 1f,  1f,  1f,  2f,  1f,  1f,  1f,  2f,  1f, 0.5f, 1f,  1f,  1f },
        /*ELC*/ new[] { 1f,  1f,  2f, 0.5f,0.5f, 1f,  1f,  1f,  0f,  2f,  1f,  1f,  1f,  1f, 0.5f, 1f,  1f,  1f },
        /*GRS*/ new[] { 1f, 0.5f, 2f,  1f, 0.5f, 1f,  1f, 0.5f, 2f, 0.5f, 1f, 0.5f, 2f,  1f, 0.5f, 1f, 0.5f, 1f },
        /*ICE*/ new[] { 1f, 0.5f,0.5f, 1f,  2f, 0.5f, 1f,  1f,  2f,  2f,  1f,  1f,  1f,  1f,  2f,  1f, 0.5f, 1f },
        /*FGT*/ new[] { 2f,  1f,  1f,  1f,  1f,  2f,  1f, 0.5f, 1f, 0.5f,0.5f,0.5f, 2f,  0f,  1f,  1f,  2f, 0.5f},
        /*PSN*/ new[] { 1f,  1f,  1f,  1f,  2f,  1f,  1f, 0.5f,0.5f, 1f,  1f,  1f, 0.5f,0.5f, 1f,  1f,  0f,  2f },
        /*GRN*/ new[] { 1f, 0.5f, 1f,  2f, 0.5f, 1f,  1f,  2f,  1f,  0f,  1f, 0.5f, 2f,  1f,  1f,  1f,  2f,  1f },
        /*FLY*/ new[] { 1f,  1f,  1f, 0.5f, 2f,  1f,  2f,  1f,  1f,  1f,  1f,  2f, 0.5f, 1f,  1f,  1f, 0.5f, 1f },
        /*PSY*/ new[] { 1f,  1f,  1f,  1f,  1f,  1f,  2f,  2f,  1f,  1f, 0.5f, 1f,  1f,  1f,  1f,  1f, 0.5f, 1f },
        /*BGG*/ new[] { 1f, 0.5f, 1f,  1f,  2f,  1f, 0.5f,0.5f, 1f, 0.5f, 2f,  1f,  1f, 0.5f, 1f, 0.5f,0.5f,0.5f},
        /*RCK*/ new[] { 1f,  2f,  1f,  1f,  1f,  2f, 0.5f, 1f, 0.5f, 2f,  1f,  2f,  1f,  1f,  1f,  1f, 0.5f, 1f },
        /*GST*/ new[] {0.5f, 1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f,  2f,  1f, 0.5f, 1f,  1f },
        /*DRA*/ new[] { 1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f, 0.5f, 0f },
        /*DRK*/ new[] { 1f,  1f,  1f,  1f,  1f,  1f, 0.5f, 1f,  1f,  1f,  2f,  1f,  1f,  2f,  1f, 0.5f, 1f, 0.5f},
        /*STL*/ new[] { 1f, 0.5f,0.5f,0.5f, 1f,  2f,  1f,  1f,  1f,  1f,  1f,  1f,  2f,  1f,  1f,  1f, 0.5f, 2f },
        /*FRY*/ new[] { 1f, 0.5f, 1f,  1f,  1f,  1f,  2f, 0.5f, 1f,  1f,  1f,  1f,  1f,  1f,  2f,  2f, 0.5f, 1f }
        // @formatter:on
    };

    /// <summary>
    /// Get the effectiveness multiplier.
    /// </summary>
    /// <param name="attackType">The type of the attack.</param>
    /// <param name="defenseType">The type of the defending Uniteon.</param>
    /// <returns>The effectiveness multiplier for an attacker and a defender.</returns>
    public static float GetEffectiveness(UniteonType attackType, UniteonType defenseType)
    {
        if (attackType == UniteonType.None || defenseType == UniteonType.None)
            return 1; // If the Uniteon's type isn't set, give the default effectiveness value of 1
        int row = (int)attackType - 1;
        int column = (int)defenseType - 1;
        return Chart[row][column];
    }
}
