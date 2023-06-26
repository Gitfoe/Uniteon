using System;
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
    [SerializeField] private BattleDialogBox battleDialogBox;
    private BattleState _battleState;
    private int _actionState; 
    
    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(InitialiseBattle());
    }

    /// <summary>
    /// Initializes the battle scene.
    /// </summary>
    private IEnumerator InitialiseBattle()
    {
        // Initialise battle field and Uniteons
        uniteonUnitGamer.InitialiseUniteon();
        uniteonHudGamer.SetGamerData(uniteonUnitGamer.Uniteon);
        uniteonUnitFoe.InitialiseUniteon();
        uniteonHudFoe.SetGamerData(uniteonUnitFoe.Uniteon);
        // Wait until wild encounter text has printed out
        yield return StartCoroutine(battleDialogBox.TypeOutDialog($"A wild {uniteonUnitFoe.Uniteon.UniteonBase.UniteonName} appeared!"));
        // Wait for some time after the text is done printing
        yield return new WaitForSeconds(2f);
        InitialiseToAction();
    }

    /// <summary>
    /// Transitions the scene state from initialisation to action.
    /// </summary>
    private void InitialiseToAction()
    {
        _battleState = BattleState.GamerAction;
        battleDialogBox.SetDialogText("Choose a strategic move...");
        battleDialogBox.EnableActionSelector(true);
    }
    
    /// <summary>
    /// Transitions the scene state from action to move.
    /// </summary>
    private void ActionToMove()
    {
        _battleState = BattleState.GamerMove;
        battleDialogBox.EnableActionSelector(false);
        battleDialogBox.EnableDialogText(false);
        battleDialogBox.EnableMoveSelector(true);
    }

    /// <summary>
    /// Every frame, check for a change in action selection.
    /// </summary>
    private void Update()
    {
        if (_battleState == BattleState.GamerAction)
        {
            HandleActionSelection();
        }
    }

    /// <summary>
    /// Watches for input of the player and changes the graphics accordingly.
    /// </summary>
    private void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (_actionState < 1)
                _actionState++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (_actionState > 0)
                _actionState--; 
        }
        battleDialogBox.UpdateActionSelection(_actionState);
        // Transition to the next state if an action has been selected
        if (Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_actionState == 0) // Attack
            {
                ActionToMove();
            }
            else if (_actionState == 1) // Run
            {
                throw new NotImplementedException();
            }
        }
        
    }
}

/// <summary>
/// All the different states a battle could be in.
/// </summary>
public enum BattleState
{
    Start,
    GamerAction,
    GamerMove,
    FoeMove,
    Attacking
}
