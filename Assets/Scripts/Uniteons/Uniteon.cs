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
        HealthPoints = uniteonBase.MaxHealthPoints;
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

    // Formula's from Pokemon to calculate stats based on level
    // https://bulbapedia.bulbagarden.net/wiki/Stat#Generation_III_onward
    public int MaxHealthPoints => Mathf.FloorToInt((UniteonBase.MaxHealthPoints * Level) / 100f) + 10;
    public int Attack => Mathf.FloorToInt((UniteonBase.Attack * Level) / 100f) + 5;
    public int Defense => Mathf.FloorToInt((UniteonBase.Defense * Level) / 100f) + 5;
    public int SpecialAttack => Mathf.FloorToInt((UniteonBase.SpecialAttack * Level) / 100f) + 5;
    public int SpecialDefense => Mathf.FloorToInt((UniteonBase.SpecialDefense * Level) / 100f) + 5;
    public int Speed => Mathf.FloorToInt((UniteonBase.Speed * Level) / 100f) + 5;
}
