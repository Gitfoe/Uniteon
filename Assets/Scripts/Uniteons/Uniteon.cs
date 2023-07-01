using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class Uniteon
{
    // Fields
    [SerializeField] private UniteonBase uniteonBase;
    [SerializeField] private int level;
    [SerializeField] private int healthPoints;

    // Properties
    public UniteonBase UniteonBase => uniteonBase;
    public int Level => level;

    public int HealthPoints
    {
        get => healthPoints;
        set => healthPoints = value;
    }
    public int Experience { get; set; }
    public List<Move> Moves { get; set; }
    public Move ExecutingMove { get; set; }
    public Dictionary<Statistic, int> Stats { get; private set; }
    public Dictionary<Statistic, int> StatBoosts { get; private set; } // Boost up to 6x
    public Queue<string> StatusMessages { get; private set; } // Status messages
    
    // Statistic properties
    public int MaxHealthPoints { get; private set; }
    public int Attack => GetBattleStat(Statistic.Attack);
    public int Defense => GetBattleStat(Statistic.Defense);
    public int SpecialAttack => GetBattleStat(Statistic.SpecialAttack);
    public int SpecialDefense => GetBattleStat(Statistic.SpecialDefense);
    public int Speed => GetBattleStat(Statistic.Speed);

    // Constructor
    public void InitialiseUniteon()
    {
        Moves = new List<Move>(); // Add moves to Uniteon
        foreach (var move in UniteonBase.LearnableMoves)
        {
            if (move.Level <= level)
                Moves.Add(new Move(move.MoveBase)); // Add new moves to the moves list if leveling up
            if (Moves.Count >= 4)
                break; // After the Uniteon already knows 4 moves, don't learn more moves
        }
        Experience = UniteonBase.GetExperienceForLevel(Level);
        CalculateStats();
        HealthPoints = MaxHealthPoints; // Set Uniteon HP to max when launching the game
        ResetBoosts();
        StatusMessages = new Queue<string>(); // Init queue
    }

    /// <summary>
    /// Calculate stats based on formula from Pokemon to calculate stats based on the current Uniteon's level
    /// https://bulbapedia.bulbagarden.net/wiki/Stat#Generation_III_onward
    /// </summary>
    private void CalculateStats()
    {
        Stats = new Dictionary<Statistic, int>
        {
            { Statistic.Attack, Mathf.FloorToInt((UniteonBase.Attack * Level) / 100f) + 5 },
            { Statistic.Defense, Mathf.FloorToInt((UniteonBase.Defense * Level) / 100f) + 5 },
            { Statistic.SpecialAttack, Mathf.FloorToInt((UniteonBase.SpecialAttack * Level) / 100f) + 5 },
            { Statistic.SpecialDefense, Mathf.FloorToInt((UniteonBase.SpecialDefense * Level) / 100f) + 5 },
            { Statistic.Speed, Mathf.FloorToInt((UniteonBase.Speed * Level) / 100f) + 5 } 
        };
        MaxHealthPoints = Mathf.FloorToInt((UniteonBase.MaxHealthPoints * Level) / 100f) + 10 + Level;
    }

    /// <summary>
    /// Retrieves the battle statistic of this Uniteon with all stat changing metrics applied.
    /// </summary>
    /// <param name="stat"></param>
    /// <returns></returns>
    private int GetBattleStat(Statistic stat)
    {
        int originalStat = Stats[stat];
        // Calculate any in-battle stat boosts
        int boost = StatBoosts[stat];
        float[] boostValues = new float[] { 1f, 1.5f, 2f, 2.5f, 3f, 3.5f, 4f }; // Multiplication amounts for each boost
        originalStat = boost >= 0 ? Mathf.FloorToInt(originalStat * boostValues[boost]) : Mathf.FloorToInt(originalStat / boostValues[-boost]);
        return originalStat;
    }

    /// <summary>
    /// Gets a random move with available PP.
    /// </summary>
    /// <returns>Random move, or if none available, null.</returns>
    public Move GetRandomMove()
    {
        List<Move> movesWithPP = new List<Move>();
        movesWithPP = Moves.Where(x => x.PowerPoints > 0).ToList();
        int randomMove = Random.Range(0, movesWithPP.Count);
        return movesWithPP.Count > 0 ? movesWithPP[randomMove] : null;
    }

    /// <summary>
    /// Applies a statistic boost or reduction by, for instance, an item or a move.
    /// </summary>
    /// <param name="statBoosts"></param>
    /// <returns>True if stats were raised and false if lowered.</returns>
    public bool ApplyBoosts(List<StatBoost> statBoosts)
    {
        foreach (var statBoost in statBoosts)
        {
            Statistic stat = statBoost.Stat;
            int boost = statBoost.Boost;
            StatBoosts[stat] = Mathf.Clamp(StatBoosts[stat] + boost, -6, 6);
            switch (boost)
            {
                case > 1:
                    StatusMessages.Enqueue($"{uniteonBase.UniteonName}'s {stat} rose sharply!");
                    break;
                case > 0:
                    StatusMessages.Enqueue($"{uniteonBase.UniteonName}'s {stat} rose!");
                    break;
                case < -1:
                    StatusMessages.Enqueue($"{uniteonBase.UniteonName}'s {stat} fell sharply!");
                    break;
                case < 0:
                    StatusMessages.Enqueue($"{uniteonBase.UniteonName}'s {stat} fell!");
                    break;
            }
            return boost > 0;
        }
        return false;
    }

    /// <summary>
    /// Resets all the stat boosts to 0.
    /// </summary>
    private void ResetBoosts()
    {
        StatBoosts = new Dictionary<Statistic, int>()
        {
            { Statistic.Attack, 0 },
            { Statistic.Defense, 0 },
            { Statistic.SpecialAttack, 0 },
            { Statistic.SpecialDefense, 0 },
            { Statistic.Speed, 0 },
            { Statistic.Accuracy, 0 },
            { Statistic.Evasion, 0 }
        };
    }

    /// <summary>
    /// Resets stat boosts.
    /// </summary>
    public void OnBattleOver() => ResetBoosts();
    
    /// <summary>
    /// Take damage according to the official Pokemon algorithm at https://bulbapedia.bulbagarden.net/wiki/Damage
    /// </summary>
    /// <param name="move">The move that the Uniteon is getting attacked by.</param>
    /// <param name="attacker">The attacking Uniteon.</param>
    /// <returns>True if the Uniteon fainted and false if not.</returns>
    public DamageData TakeDamage(Move move, Uniteon attacker)
    {
        // 6.25% chance of a critical hit (double the damage)
        float criticalHitModifier = 1f;
        if (Random.Range(1, 101) <= 6.25f)
            criticalHitModifier = 2f;
        // Get effectiveness of move
        float effectivenessType1 = EffectivenessChart.GetEffectiveness(move.MoveBase.MoveType, this.UniteonBase.UniteonType1);
        float effectivenessType2 = EffectivenessChart.GetEffectiveness(move.MoveBase.MoveType, this.UniteonBase.UniteonType2);
        float totalEffectivenessModifier = effectivenessType1 * effectivenessType2;
        // Initialise damage data
        DamageData damageData = new DamageData()
        {
            EffectivenessModifier = totalEffectivenessModifier,
            CriticalHitModifier = criticalHitModifier
        };
        // Find move category and assign attacking power and defense points
        float attack = move.MoveBase.MoveCategory switch
        {
            MoveCategory.Physical => attacker.Attack,
            MoveCategory.Special => attacker.SpecialAttack,
            MoveCategory.Status => 0, // Status moves, temporarily set to 0 until we implement status moves
            _ => throw new ArgumentOutOfRangeException()
        };
        float defense = move.MoveBase.MoveCategory switch
        {
            MoveCategory.Physical => Defense,
            MoveCategory.Special => SpecialDefense,
            MoveCategory.Status => Defense, // Temporarily default to defense for status moves
            _ => throw new ArgumentOutOfRangeException()
        };
        // Calculate damage
        float randomizeDamageModifier = Random.Range(0.85f, 1f) * criticalHitModifier * totalEffectivenessModifier; // Randomize move's damage between 85% and 100%
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.MoveBase.Power * ((float)attack / defense) + 2;
        int damage = Mathf.FloorToInt(d * randomizeDamageModifier);
        HealthPoints -= damage;
        if (HealthPoints <= 0)
        {
            HealthPoints = 0; // Avoid negative HP in the UI
            damageData.Fainted = true;
        }
        return damageData;
    }

    /// <summary>
    /// Plays the cry of this Uniteon and waits until the cry has been completed to continue.
    /// </summary>
    /// <param name="panning">Float for panning left to right (-1 is left, 1 is right, 0 is center).</param>
    /// <param name="fainted">If the Uniteon has fainted to slow down the pitch of the audio clip.</param>
    /// <returns>Coroutine.</returns>
    public IEnumerator PlayCry(float panning, bool fainted = false)
    {
        if (fainted)
            AudioManager.Instance.PlaySfx(UniteonBase.Cry, panning: panning, pitch: 0.72f);
        else
            AudioManager.Instance.PlaySfx(UniteonBase.Cry, panning: panning);
        yield return new WaitForSeconds(UniteonBase.Cry.length + 0.27f);
    }
}

/// <summary>
/// Holds data related to taking damage.
/// </summary>
public class DamageData
{
    public bool Fainted { get; set; }
    public float EffectivenessModifier { get; set; }
    public float CriticalHitModifier { get; set; }
    public DamageData() => Fainted = false;
}
