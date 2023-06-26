using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript
{
    private Base _uniteonBase;
    private int _level;

    public NewBehaviourScript(Base uniteonBase, int level)
    {
        _uniteonBase = uniteonBase;
        _level = level;
    }

    // Formula's from Pokemon to calculate stats based on level
    // https://bulbapedia.bulbagarden.net/wiki/Stat#Generation_III_onward
    public int HealthPoints => Mathf.FloorToInt((_uniteonBase.HealthPoints * _level) / 100f) + 10;
    public int Attack => Mathf.FloorToInt((_uniteonBase.Attack * _level) / 100f) + 5;
    public int Defense => Mathf.FloorToInt((_uniteonBase.Defense * _level) / 100f) + 5;
    public int SpecialAttack => Mathf.FloorToInt((_uniteonBase.SpecialAttack * _level) / 100f) + 5;
    public int SpecialDefense => Mathf.FloorToInt((_uniteonBase.SpecialDefense * _level) / 100f) + 5;
    public int Speed => Mathf.FloorToInt((_uniteonBase.Speed * _level) / 100f) + 5;
}
