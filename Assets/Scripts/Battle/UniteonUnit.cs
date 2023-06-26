using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UniteonUnit : MonoBehaviour
{
    // Fields
    [SerializeField] private UniteonBase uniteonBase;
    [SerializeField] private int level;
    [SerializeField] private bool isGamerUniteon; // To determine if the Uniteon is the gamer's or the foe's

    // Properties
    public Uniteon Uniteon { get; set; }
    
    /// <summary>
    /// Creates a new Uniteon object and sets the correct image.
    /// </summary>
    public void InitialiseUniteon()
    {
        Uniteon = new Uniteon(uniteonBase, level);
        if (isGamerUniteon)
            GetComponent<Image>().sprite = Uniteon.UniteonBase.BackSprite;
        else
            GetComponent<Image>().sprite = Uniteon.UniteonBase.FrontSprite;
    }
}
