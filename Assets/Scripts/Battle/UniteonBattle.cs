using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
    
    // Events
    public event Action<bool> OnBattleOver;
    
    /// <summary>
    /// Starts a wild Uniteon battle.
    /// </summary>
    public void InitiateBattle() => StartCoroutine(InitialiseBattle());

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
        // Wait an additional second after the text is done printing
        yield return new WaitForSeconds(1f);
        TransitionToAction();
    }

    /// <summary>
    /// Transitions the scene state to action.
    /// </summary>
    private void TransitionToAction()
    {
        _battleState = BattleState.GamerAction;
        battleDialogBox.SetDialogText("Choose a strategic move...");
        battleDialogBox.EnableActionSelector(true);
    }
    
    /// <summary>
    /// Transitions the scene state to move.
    /// </summary>
    private void TransitionToMove()
    {
        _battleState = BattleState.GamerMove;
        battleDialogBox.EnableActionSelector(false);
        battleDialogBox.EnableDialogText(false);
        battleDialogBox.EnableMoveSelector(true);
    }

    /// <summary>
    /// Every frame, check for a change in action selection, if set active by the GameController.
    /// </summary>
    public void ControllerUpdate()
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
                TransitionToMove();
            }
            else if (_actionSelection == 1) // Run
            {
                OnBattleOver?.Invoke(false); // Just end the battle
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
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            battleDialogBox.EnableMoveSelector(false);
            battleDialogBox.EnableDialogText(true);
            StartCoroutine(ExecuteGamerMove());
        }
    }

    /// <summary>
    /// The sequence for when the gamer attacks the foe.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator ExecuteGamerMove()
    {
        _battleState = BattleState.Attacking;
        var move = uniteonUnitGamer.Uniteon.Moves[_moveSelection]; // Get the selected move
        yield return battleDialogBox.TypeOutDialog($"{uniteonUnitGamer.Uniteon.UniteonBase.UniteonName} used {move.MoveBase.MoveName}!");
        yield return uniteonUnitGamer.PlayAttackAnimations();
        uniteonUnitFoe.PlayHitAnimation();
        int previousHealthPoints = uniteonUnitFoe.Uniteon.HealthPoints;
        DamageData damageData = uniteonUnitFoe.Uniteon.TakeDamage(move, uniteonUnitGamer.Uniteon); // Attack foe
        yield return uniteonHudFoe.UpdateHealthPoints(previousHealthPoints);
        yield return ShowDamageData(damageData);
        if (damageData.Fainted)
        {
            yield return battleDialogBox.TypeOutDialog($"{uniteonUnitFoe.Uniteon.UniteonBase.UniteonName} has fainted!");
            yield return uniteonUnitFoe.PlayFaintAnimation();
            OnBattleOver?.Invoke(true);
            //OnBattleOver(true);
        }
        else
            StartCoroutine(ExecuteFoeMove());
    }

    /// <summary>
    /// The sequence for when the foe attacks the gamer.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator ExecuteFoeMove()
    {
        _battleState = BattleState.FoeMove;
        // Quite simple battle AI - but get a random move of the foe
        int randomMoveIndex = Random.Range(0, uniteonUnitFoe.Uniteon.Moves.Count);
        Move move = uniteonUnitFoe.Uniteon.Moves[randomMoveIndex];
        yield return battleDialogBox.TypeOutDialog($"{uniteonUnitFoe.Uniteon.UniteonBase.UniteonName} used {move.MoveBase.MoveName}!");
        yield return uniteonUnitFoe.PlayAttackAnimations();
        uniteonUnitGamer.PlayHitAnimation();
        int previousHealthPoints = uniteonUnitGamer.Uniteon.HealthPoints;
        DamageData damageData = uniteonUnitGamer.Uniteon.TakeDamage(move, uniteonUnitFoe.Uniteon); // Attack gamer
        yield return uniteonHudGamer.UpdateHealthPoints(previousHealthPoints);
        yield return ShowDamageData(damageData);
        if (damageData.Fainted)
        {
            yield return battleDialogBox.TypeOutDialog($"{uniteonUnitGamer.Uniteon.UniteonBase.UniteonName} has fainted!");
            yield return uniteonUnitGamer.PlayFaintAnimation();
            OnBattleOver?.Invoke(false);
            //OnBattleOver(false);
        }
        else
            TransitionToAction();
    }

    /// <summary>
    /// Prints critical hits and effectiveness to the dialog box.
    /// </summary>
    /// <param name="damageData">The damage data of the Uniteon.</param>
    /// <returns></returns>
    public IEnumerator ShowDamageData(DamageData damageData)
    {
        if (damageData.CriticalHitModifier > 1f)
            yield return battleDialogBox.TypeOutDialog("It's a critical hit!");
        switch (damageData.EffectivenessModifier)
        {
            case > 2f:
                yield return battleDialogBox.TypeOutDialog("It's mega effective!!!");
                break;
            case > 1f:
                yield return battleDialogBox.TypeOutDialog("It's super effective!");
                break;
            case < 1f:
                yield return battleDialogBox.TypeOutDialog("It's not very effective...");
                break;
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
