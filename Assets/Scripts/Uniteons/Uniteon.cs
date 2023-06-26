using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    /// <returns>True if the Pokemon fainted and false if not.</returns>
    public bool TakeDamage(Move move, Uniteon attacker)
    {
        float randomizeDamageModifier = Random.Range(0.85f, 1f);
        float a = (2 * attacker.Level + 10) / 250f;
        float d = a * move.MoveBase.Power * ((float)attacker.Attack / Defense) + 2;
        int damage = Mathf.FloorToInt(d * randomizeDamageModifier);
        HealthPoints -= damage;
        if (HealthPoints <= 0)
        {
            HealthPoints = 0; // Avoid negative HP in the UI
            return true;
        }
        return false;
    }
}
