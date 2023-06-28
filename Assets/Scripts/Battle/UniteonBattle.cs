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
    [SerializeField] private AudioClip hitNormalEffectiveness;
    [SerializeField] private AudioClip hitNotVeryEffective;
    [SerializeField] private AudioClip hitSuperEffective;
    [SerializeField] private AudioClip faint;
    [SerializeField] private AudioClip tackle;
    [SerializeField] private AudioClip aButton;
    [SerializeField] private AudioClip run;
    [SerializeField] private AudioClip switchOut;
    [SerializeField] private PartyScreen partyScreen;
    private BattleSequenceState _battleSequenceState;
    private int _actionSelection;
    private int _moveSelection;
    private int _memberSelection;
    private UniteonParty _gamerParty;
    private Uniteon _wildUniteon;
    
    // Events
    public event Action<bool> OnBattleOver;

    /// <summary>
    /// Starts a wild Uniteon battle.
    /// </summary>
    public void InitiateBattle(UniteonParty gamerParty, Uniteon wildUniteon )
    {
        _gamerParty = gamerParty;
        _wildUniteon = wildUniteon;
        StartCoroutine(InitialiseBattle());
    }
   

    /// <summary>
    /// Initializes the battle scene.
    /// </summary>
    private IEnumerator InitialiseBattle()
    {
        // Initialise Uniteons
        uniteonUnitGamer.InitialiseUniteon(_gamerParty.GetHealthyUniteon());
        uniteonUnitFoe.InitialiseUniteon(_wildUniteon);
        // Initialise battlefield
        uniteonHudGamer.SetGamerData(uniteonUnitGamer.Uniteon);
        uniteonHudFoe.SetGamerData(uniteonUnitFoe.Uniteon);
        battleDialogBox.SetMoveNames(uniteonUnitGamer.Uniteon.Moves);
        // Initialise party screen
        partyScreen.InitialisePartyScreen();
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
        _battleSequenceState = BattleSequenceState.GamerAction;
        battleDialogBox.SetDialogText("Choose a strategic move...");
        battleDialogBox.EnableActionSelector(true);
    }
    
    private void OpenPartyScreen()
    {
        _battleSequenceState = BattleSequenceState.PartyScreen;
        partyScreen.gameObject.SetActive(true);
        partyScreen.AddUniteonsToPartySlots(_gamerParty.Uniteons);
    }
    
    /// <summary>
    /// Transitions the scene state to move.
    /// </summary>
    private void TransitionToMove()
    {
        _battleSequenceState = BattleSequenceState.GamerMove;
        battleDialogBox.EnableActionSelector(false);
        battleDialogBox.EnableDialogText(false);
        battleDialogBox.EnableMoveSelector(true);
    }

    /// <summary>
    /// Every frame, check for a change in action selection, if set active by the GameController.
    /// </summary>
    public void ControllerUpdate()
    {
        if (_battleSequenceState == BattleSequenceState.GamerAction)
        {
            HandleActionSelection();
        }
        else if (_battleSequenceState == BattleSequenceState.GamerMove)
        {
            HandleMoveSelection();
        }
        else if (_battleSequenceState == BattleSequenceState.PartyScreen)
        {
            HandlePartyScreenSelection();
        }
    }

    /// <summary>
    /// Handles the selection of 4 buttons.
    /// </summary>
    /// <param name="selection">The selection parameter you want to modify.</param>
    /// <param name="upperBound">The maximum value of selectable buttons.</param>
    /// <returns>The current selection.</returns>
    private int HandleSelectionButtons(int selection, int upperBound)
    {
        int direction = 0;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            direction = 1;
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            direction = -1;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            direction = 2;
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            direction = -2;
        if (direction != 0)
        {
            int newSelection = selection + direction;
            if (newSelection >= 0 && newSelection <= upperBound)
            {
                selection = newSelection;
                AudioManager.Instance.PlaySfx(aButton);
            }
        }
        return Mathf.Clamp(selection, 0, upperBound);
    }
    
    /// <summary>
    /// Watches for input of the player and changes the graphics accordingly in the action selection state.
    /// </summary>
    private void HandleActionSelection()
    {
        _actionSelection = HandleSelectionButtons(_actionSelection, 3);
        battleDialogBox.UpdateActionSelection(_actionSelection);
        // Transition to the next state if an action has been selected
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_actionSelection == 0) // Attack
            {
                AudioManager.Instance.PlaySfx(aButton);
                TransitionToMove();
            }
            else if (_actionSelection == 1) // Switch
            {
                AudioManager.Instance.PlaySfx(aButton);
                OpenPartyScreen();
            }
            else if (_actionSelection == 2) // Pack
            {
                throw new NotImplementedException();
            }
            else if (_actionSelection == 3) // Run
            {
                AudioManager.Instance.StopMusic();
                AudioManager.Instance.StopSfx(2);
                AudioManager.Instance.PlaySfx(run);
                OnBattleOver?.Invoke(false); // Just end the battle for now
            }
        }
    }

    /// <summary>
    /// Watches for input of the player and changes the graphics accordingly in the move selection state.
    /// </summary>
    private void HandleMoveSelection()
    {
        _moveSelection = HandleSelectionButtons(_moveSelection, uniteonUnitGamer.Uniteon.Moves.Count - 1);
        battleDialogBox.UpdateMoveSelection(_moveSelection, uniteonUnitGamer.Uniteon.Moves[_moveSelection]);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            AudioManager.Instance.PlaySfx(aButton);
            battleDialogBox.EnableMoveSelector(false);
            battleDialogBox.EnableDialogText(true);
            StartCoroutine(ExecuteGamerMove());
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioManager.Instance.PlaySfx(aButton);
            battleDialogBox.EnableMoveSelector(false);
            battleDialogBox.EnableDialogText(true);
            TransitionToAction();
        }
    }

    /// <summary>
    /// Watches for inputs in the party selection screen and sends out a new Uniteon if one got selected.
    /// </summary>
    private void HandlePartyScreenSelection()
    {
        _memberSelection = HandleSelectionButtons(_memberSelection, _gamerParty.Uniteons.Count - 1);
        partyScreen.UpdateMemberSelection(_memberSelection);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Uniteon selectedMember = _gamerParty.Uniteons[_memberSelection];
            if (selectedMember.HealthPoints <= 0)
            {
                partyScreen.SetMessageText($"{selectedMember.UniteonBase.UniteonName} can't battle right now!");
                return;
            }
            if (selectedMember == uniteonUnitGamer.Uniteon)
            {
                partyScreen.SetMessageText($"{selectedMember.UniteonBase.UniteonName} is already on the field!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            _battleSequenceState = BattleSequenceState.Busy;
            StartCoroutine(SwitchUniteon(selectedMember));
        }
        // Enable going back to the action screen
        else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioManager.Instance.PlaySfx(aButton);
            partyScreen.gameObject.SetActive(false);
            TransitionToAction(); 
        }
    }

    private IEnumerator SwitchUniteon(Uniteon uniteon)
    {
        battleDialogBox.EnableActionSelector(false);
        // Call back Uniteon
        yield return battleDialogBox.TypeOutDialog(
            $"You've done well, {uniteonUnitGamer.Uniteon.UniteonBase.UniteonName}!");
        AudioManager.Instance.PlaySfx(switchOut);
        uniteonUnitGamer.PlayBattleLeaveAnimation();
        yield return new WaitForSeconds(2f);
        // Send out new Uniteon
        yield return SendOutUniteon(uniteon);
        // Give turn to foe
        StartCoroutine(ExecuteFoeMove());
    }

    /// <summary>
    /// The sequence for when the gamer attacks the foe.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator ExecuteGamerMove()
    {
        // Set the battle state, get the selected move and deduct PP
        _battleSequenceState = BattleSequenceState.Busy;
        var move = uniteonUnitGamer.Uniteon.Moves[_moveSelection];
        move.PowerPoints--;
        // Write move to the dialog box
        yield return battleDialogBox.TypeOutDialog($"{uniteonUnitGamer.Uniteon.UniteonBase.UniteonName} used {move.MoveBase.MoveName}!");
        // Play Uniteon attack animation and sfx
        AudioManager.Instance.PlaySfx(tackle, panning: -0.72f);
        yield return uniteonUnitGamer.PlayAttackAnimations();
        // Calculate damage
        int previousHealthPoints = uniteonUnitFoe.Uniteon.HealthPoints;
        DamageData damageData = uniteonUnitFoe.Uniteon.TakeDamage(move, uniteonUnitGamer.Uniteon);
        // Play Uniteon damage animation and sfx
        uniteonUnitFoe.PlayHitAnimation();
        PlayHitSfx(damageData, _battleSequenceState);
        // Show damage decay on health bar
        yield return uniteonHudFoe.UpdateHealthPoints(previousHealthPoints);
        // Write effectiveness/critical hit to the dialog box
        yield return WriteDamageData(damageData);
        // If Uniteon fainted, write to dialog box, play faint animation and cry, and end battle
        if (damageData.Fainted)
        {
            AudioManager.Instance.StopSfx(2); // Stop low health sfx
            yield return battleDialogBox.TypeOutDialog($"{uniteonUnitFoe.Uniteon.UniteonBase.UniteonName} has fainted!");
            yield return uniteonUnitFoe.Uniteon.PlayCry(0.72f, true);
            AudioManager.Instance.PlaySfx(faint, panning: 0.72f);
            yield return uniteonUnitFoe.PlayFaintAnimation();
            OnBattleOver?.Invoke(true);
        }
        // If Uniteon not fainted, other Uniteon can move
        else
            StartCoroutine(ExecuteFoeMove());
    }


    /// <summary>
    /// The sequence for when the foe attacks the gamer.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator ExecuteFoeMove()
    {
        _battleSequenceState = BattleSequenceState.FoeMove;
        // Quite simple battle AI - but get a random move of the foe
        int randomMoveIndex = Random.Range(0, uniteonUnitFoe.Uniteon.Moves.Count);
        Move move = uniteonUnitFoe.Uniteon.Moves[randomMoveIndex];
        move.PowerPoints--;
        yield return battleDialogBox.TypeOutDialog($"{uniteonUnitFoe.Uniteon.UniteonBase.UniteonName} used {move.MoveBase.MoveName}!");
        AudioManager.Instance.PlaySfx(tackle, panning: 0.72f);
        yield return uniteonUnitFoe.PlayAttackAnimations();
        int previousHealthPoints = uniteonUnitGamer.Uniteon.HealthPoints;
        DamageData damageData = uniteonUnitGamer.Uniteon.TakeDamage(move, uniteonUnitFoe.Uniteon);
        uniteonUnitGamer.PlayHitAnimation();
        PlayHitSfx(damageData, _battleSequenceState);
        yield return uniteonHudGamer.UpdateHealthPoints(previousHealthPoints);
        yield return WriteDamageData(damageData);
        if (damageData.Fainted)
        {
            AudioManager.Instance.StopSfx(2);
            yield return battleDialogBox.TypeOutDialog($"{uniteonUnitGamer.Uniteon.UniteonBase.UniteonName} has fainted!");
            yield return uniteonUnitGamer.Uniteon.PlayCry(-0.72f, true);
            AudioManager.Instance.PlaySfx(faint, panning: -0.72f);
            yield return uniteonUnitGamer.PlayFaintAnimation();
            // Check if gamer has more Uniteon in it's party
            Uniteon nextUniteon = _gamerParty.GetHealthyUniteon();
            if (nextUniteon != null)
            {
                yield return SendOutUniteon(nextUniteon);
                TransitionToAction();
            }
            else
                OnBattleOver?.Invoke(false);
        }
        else
            TransitionToAction();
    }

    /// <summary>
    /// Sends out the next Uniteon into the battlefield.
    /// </summary>
    /// <param name="nextUniteon">The next Uniteon.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator SendOutUniteon(Uniteon nextUniteon)
    {
        // Initialise next uniteon
        uniteonUnitGamer.InitialiseUniteon(nextUniteon);
        // Initialise the gamer size of battlefield
        uniteonHudGamer.SetGamerData(nextUniteon);
        battleDialogBox.SetMoveNames(nextUniteon.Moves);
        // Play cry
        StartCoroutine(nextUniteon.PlayCry(-0.72f));
        // Wait until motivational text has printed out
        yield return StartCoroutine(battleDialogBox.TypeOutDialog($"You got it, {nextUniteon.UniteonBase.UniteonName}!"));
        // Wait an additional second after the text is done printing
        yield return new WaitForSeconds(1f);
    }

    /// <summary>
    /// Plays the Uniteon hit sound effect.
    /// </summary>
    /// <param name="damageData">The damage data of the Uniteon.</param>
    /// <param name="battleSequenceState">The current battle state.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the battle state is invalid.</exception>
    private void PlayHitSfx(DamageData damageData, BattleSequenceState battleSequenceState)
    {
        float panning = battleSequenceState switch
        {
            BattleSequenceState.Busy => 0.72f,
            BattleSequenceState.FoeMove => -0.72f,
            _ => throw new ArgumentOutOfRangeException(nameof(battleSequenceState), battleSequenceState, null)
        };
        AudioClip sfxClip = damageData.EffectivenessModifier switch
        {
            > 1f => hitSuperEffective,
            < 1f => hitNotVeryEffective,
            _ => hitNormalEffectiveness
        };
        AudioManager.Instance.PlaySfx(sfxClip, panning: panning);
    }

    /// <summary>
    /// Prints critical hits and effectiveness to the dialog box.
    /// </summary>
    /// <param name="damageData">The damage data of the Uniteon.</param>
    /// <returns></returns>
    private IEnumerator WriteDamageData(DamageData damageData)
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
public enum BattleSequenceState
{
    Start,
    GamerAction,
    GamerMove,
    FoeMove,
    Busy,
    PartyScreen
}
