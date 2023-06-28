using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Uniteon
{
    // Properties
    public UniteonBase UniteonBase { get; set; }
    public int Level { get; set; }
    public int HealthPoints { get; set; }
    public List<Move> Moves { get; set; }

    // Constructor
    public Uniteon(UniteonBase uniteonBase, int level)
    {
        UniteonBase = uniteonBase;
        Level = level;
        HealthPoints = MaxHealthPoints;
        // Add moves to Uniteon
        Moves = new List<Move>();
        foreach (var move in UniteonBase.LearnableMoves)
        {
            if (move.Level <= level)
                Moves.Add(new Move(move.MoveBase)); // Add new moves to the moves list if leveling up
            if (Moves.Count >= 4)
                break; // After the Uniteon already knows 4 moves, don't learn more moves
        }
    }

    // Formula's from Pokemon to calculate stats based on the current Uniteon's level
    // https://bulbapedia.bulbagarden.net/wiki/Stat#Generation_III_onward
    public int MaxHealthPoints => Mathf.FloorToInt((UniteonBase.MaxHealthPoints * Level) / 100f) + 10;
    public int Attack => Mathf.FloorToInt((UniteonBase.Attack * Level) / 100f) + 5;
    public int Defense => Mathf.FloorToInt((UniteonBase.Defense * Level) / 100f) + 5;
    public int SpecialAttack => Mathf.FloorToInt((UniteonBase.SpecialAttack * Level) / 100f) + 5;
    public int SpecialDefense => Mathf.FloorToInt((UniteonBase.SpecialDefense * Level) / 100f) + 5;
    public int Speed => Mathf.FloorToInt((UniteonBase.Speed * Level) / 100f) + 5;

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
            _ => 0 // Status moves
        };
        float defense = move.MoveBase.MoveCategory switch
        {
            MoveCategory.Physical => Defense,
            MoveCategory.Special => SpecialDefense,
            _ => 0
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
        yield return new WaitForSeconds(UniteonBase.Cry.length);
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
