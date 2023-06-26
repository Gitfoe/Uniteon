using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Uniteon", menuName = "Uniteon/Create new Uniteon")]
public class Base : ScriptableObject
{
    // Fields
    [SerializeField] private int uniteonID;
    [SerializeField] private string uniteonName;
    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private Sprite frontSprite;
    [SerializeField] private Sprite backSprite;
    [SerializeField] private UniteonType uniteonType1; // Temporarily one sprite
    [SerializeField] private UniteonType uniteonType2;
    [SerializeField] private int healthPoints;
    [SerializeField] private int attack;
    [SerializeField] private int defense;
    [SerializeField] private int specialAttack;
    [SerializeField] private int specialDefense;
    [SerializeField] private int speed;
    
    // Properties
    public int UniteonID => uniteonID;
    public string UniteonName => uniteonName;
    public string Description => description;
    public Sprite FrontSprite => frontSprite;
    public Sprite BackSprite => backSprite;
    public UniteonType UniteonType1 => uniteonType1;
    public UniteonType UniteonType2 => uniteonType2;
    public int HealthPoints => healthPoints;
    public int Attack => attack;
    public int Defense => defense;
    public int SpecialAttack => specialAttack;
    public int SpecialDefense => specialDefense;
    public int Speed => speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public enum UniteonType
{
    None,
    Normeon,
    Flamiteon,
    Aquoreon,
    Herbeon,
    Voltineon,
    Battleon,
    Venomeon,
    Terreon,
    Aeroeon,
    Psycoeon,
    Buggeon,
    Stondeon,
    Spectreon,
    Shadoweon,
    Dracineon,
    Ironeon,
    Faerieon
}
