using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UniteonParty : MonoBehaviour
{
    [SerializeField] private List<Uniteon> uniteons;
    
    /// <summary>
    /// Initialise gamer's party.
    /// </summary>
    void Start()
    {
        foreach (var uniteon in uniteons)
        {
            uniteon.InitialiseUniteon();
        }
    }

    /// <summary>
    /// Gets the first healthy Uniteon in the party list.
    /// </summary>
    /// <returns>First healthy Uniteon.</returns>
    public Uniteon GetHealthyUniteon() => uniteons.FirstOrDefault(u => u.HealthPoints > 0);

    // Update is called once per frame
    void Update()
    {
        
    }
}
