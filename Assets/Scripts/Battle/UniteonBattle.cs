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
    private int _actionSelection;
    private int _moveSelection;
    
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
        battleDialogBox.SetMoveNames(uniteonUnitGamer.Uniteon.Moves);
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
        else if (_battleState == BattleState.GamerMove)
        {
            HandleMoveSelection();
        }
    }

    /// <summary>
    /// Watches for input of the player and changes the graphics accordingly in the action selection state.
    /// </summary>
    private void HandleActionSelection()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (_actionSelection < 1)
                _actionSelection++;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (_actionSelection > 0)
                _actionSelection--; 
        }
        battleDialogBox.UpdateActionSelection(_actionSelection);
        // Transition to the next state if an action has been selected
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_actionSelection == 0) // Attack
            {
                ActionToMove();
            }
            else if (_actionSelection == 1) // Run
            {
                throw new NotImplementedException();
            }
        }
    }
    
    /// <summary>
    /// Watches for input of the player and changes the graphics accordingly in the move selection state.
    /// </summary>
    private void HandleMoveSelection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            if (_moveSelection < uniteonUnitGamer.Uniteon.Moves.Count - 1)
                _moveSelection++;
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) 
        {
            if (_moveSelection > 0)
                _moveSelection--; 
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            if (_moveSelection < uniteonUnitGamer.Uniteon.Moves.Count - 2)
                _moveSelection += 2;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            if (_moveSelection > 0)
                _moveSelection -= 2; 
        }
        battleDialogBox.UpdateMoveSelection(_moveSelection, uniteonUnitGamer.Uniteon.Moves[_moveSelection]); 
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
