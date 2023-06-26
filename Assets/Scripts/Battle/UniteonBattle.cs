using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniteonBattle : MonoBehaviour
{
    // Fields
    [SerializeField] private UniteonGamer uniteonGamer;
    [SerializeField] private GamerHud gamerHud;
    
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
        uniteonGamer.InitialiseUniteon();
        gamerHud.SetGamerData(uniteonGamer.Uniteon);
    }
}
