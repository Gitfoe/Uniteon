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
    [SerializeField] private GrowthRate growthRate;
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
    public GrowthRate GrowthRate => growthRate;
    public List<LearnableMove> LearnableMoves => learnableMoves;
    public AudioClip Cry => cry;
    
    // Static properties
    public static int MaxMoves { get; private set; } = 4;
    
    /// <summary>
    /// https://bulbapedia.bulbagarden.net/wiki/Experience
    /// </summary>
    /// <returns>The experience points for this Uniteon for a certain level taking into account growth rate.</returns>
    public int GetExperienceForLevel(int level) =>
        growthRate switch
        {
            GrowthRate.Fast => 4 * (level * level * level) / 5,
            GrowthRate.MediumFast => level * level * level,
            GrowthRate.MediumSlow => 6 * (level * level * level) / 5 - 15 * (level * level) + 100 * level - 140,
            GrowthRate.Slow => 5 * (level * level * level) / 4,
            GrowthRate.Fluctuating => GetFluctuating(level),
            GrowthRate.Erratic => GetErratic(level),
            _ => -1
        };
    
    private int GetFluctuating(int level) =>
        level switch
        {
            < 15 => Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor((level + 1) / 3) + 24) / 50)),
            >= 15 and < 36 => Mathf.FloorToInt(Mathf.Pow(level, 3) * ((level + 14) / 50)),
            _ => Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor(level / 2) + 32) / 50))
        };

    private int GetErratic(int level) =>
        level switch
        {
            < 50 => Mathf.FloorToInt(Mathf.Pow(level, 3) * ((level - 100) / 50)),
            >= 50 and < 68 => Mathf.FloorToInt(Mathf.Pow(level, 3) * ((level - 150) / 100)),
            >= 68 and < 98 => Mathf.FloorToInt(Mathf.Pow(level, 3) * ((Mathf.Floor(1911 - (level * 10)) / 3) / 500)),
            _ => Mathf.FloorToInt(Mathf.Pow(level, 3) * ((level - 160) / 100))
        };
}

/// <summary>
/// Struct that holds a learnable move and at which level it is earned by leveling up.
/// </summary>
[Serializable]
public class LearnableMove
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
/// The Uniteon growth rate. The faster the growth rate, the less experience the Uniteon needs to level up.
/// </summary>
public enum GrowthRate
{
    Fast,
    MediumFast,
    MediumSlow,
    Slow,
    Fluctuating,
    Erratic
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
