using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniteonParty : MonoBehaviour
{
    // Fields
    [SerializeField] private List<Uniteon> uniteons;
    
    // Properties
    public List<Uniteon> Uniteons => uniteons;

    /// <summary>
    /// Initialise gamer's party.
    /// </summary>
    private void Start()
    {
        foreach (var uniteon in uniteons)
            uniteon.InitialiseUniteon();
    }

    /// <summary>
    /// Gets the first healthy Uniteon in the party list.
    /// </summary>
    /// <returns>First healthy Uniteon.</returns>
    public Uniteon GetHealthyUniteon() => uniteons.FirstOrDefault(u => u.HealthPoints > 0);

    public void HealAllUniteons() =>
        uniteons.ForEach(x => x.HealthPoints = Mathf.FloorToInt((x.UniteonBase.MaxHealthPoints * x.Level) / 100f) + 10 + x.Level);
}
