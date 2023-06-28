using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldArea : MonoBehaviour
{
    [SerializeField] private List<Uniteon> wildUniteons;

    /// <summary>
    /// Generates a wild Uniteon.
    /// </summary>
    /// <returns>Wild Uniteon.</returns>
    public Uniteon GetWildUniteon()
    {
        // Simple algorithm, just random Uniteon, in the future maybe will be based on rarity
        var wildUniteon = wildUniteons[Random.Range(0, wildUniteons.Count)];
        wildUniteon.InitialiseUniteon();
        return wildUniteon;
    }
}
