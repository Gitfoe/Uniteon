using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class UniteonBattle : MonoBehaviour
{
    // Fields
    [SerializeField] private UniteonUnit uniteonUnitGamer;
    [SerializeField] private UniteonUnit uniteonUnitFoe;
    [SerializeField] private UniteonHud uniteonHudGamer;
    [SerializeField] private UniteonHud uniteonHudFoe;
    
    // Start is called before the first frame update
    private void Start()
    {
        InitialiseBattle();
    }

    /// <summary>
    /// Initializes the battle scene.
    /// </summary>
    private void InitialiseBattle()
    {
        uniteonUnitGamer.InitialiseUniteon();
        uniteonHudGamer.SetGamerData(uniteonUnitGamer.Uniteon);
        uniteonUnitFoe.InitialiseUniteon();
        uniteonHudFoe.SetGamerData(uniteonUnitFoe.Uniteon);
    }
}
