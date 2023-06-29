using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class UniteonBattle : MonoBehaviour
{
    #region Fields
    [SerializeField] private UniteonUnit uniteonUnitGamer;
    [SerializeField] private UniteonUnit uniteonUnitFoe;
    [SerializeField] private BattleDialogBox battleDialogBox;
    [SerializeField] private List<UniteonSfx> audioClips;
    [SerializeField] private PartyScreen partyScreen;
    private BattleSequenceState _battleSequenceState;
    private int _actionSelection;
    private int _moveSelection;
    private int _memberSelection;
    private UniteonParty _gamerParty;
    private Uniteon _wildUniteon;
    private Dictionary<string, AudioClip> _audioClips;
    #endregion
    
    #region Events
    public event Action<bool> OnBattleOver;
    #endregion

    #region Initialisation
    /// <summary>
    /// Starts a wild Uniteon battle.
    /// </summary>
    public void StartBattle(UniteonParty gamerParty, Uniteon wildUniteon )
    {
        _gamerParty = gamerParty;
        _wildUniteon = wildUniteon;
        _audioClips = UniteonSfx.ConvertListToDictionary(audioClips);
        StartCoroutine(InitialiseBattle());
    }
   

    /// <summary>
    /// Initializes the battle scene.
    /// </summary>
    private IEnumerator InitialiseBattle()
    {
        // Initialise Uniteons
        uniteonUnitGamer.InitialiseUniteonUnit(_gamerParty.GetHealthyUniteon());
        uniteonUnitFoe.InitialiseUniteonUnit(_wildUniteon);
        // Initialise battle dialog box
        battleDialogBox.SetMoveNames(uniteonUnitGamer.Uniteon.Moves);
        // Initialise party screen
        partyScreen.InitialisePartyScreen();
        // Wait until wild encounter text has printed out
        StartCoroutine(uniteonUnitFoe.Uniteon.PlayCry(0.72f));
        yield return StartCoroutine(battleDialogBox.TypeOutDialog($"A wild {uniteonUnitFoe.Uniteon.UniteonBase.UniteonName} appeared!"));
        // Wait an additional second after the text is done printing
        yield return new WaitForSeconds(1f);
        CheckFirstTurn();
    }
    #endregion
    
    #region Selection
    /// <summary>
    /// Every frame, check for a change in action selection, if set active by the GameController.
    /// </summary>
    public void ControllerUpdate()
    {
        switch (_battleSequenceState)
        {
            case BattleSequenceState.ActionSelection:
                HandleActionSelection();
                break;
            case BattleSequenceState.MoveSelection:
                HandleMoveSelection();
                break;
            case BattleSequenceState.PartyScreenSelect or BattleSequenceState.PartyScreenFaint:
                HandlePartyScreenSelection();
                break;
        }
    }
    
    /// <summary>
    /// Transitions the scene state to action.
    /// </summary>
    private void ActionSelection()
    {
        _battleSequenceState = BattleSequenceState.ActionSelection;
        battleDialogBox.SetDialogText("Choose a strategic move...");
        battleDialogBox.EnableActionSelector(true);
    }

    /// <summary>
    /// Transitions the scene state to move.
    /// </summary>
    private void MoveSelection()
    {
        _battleSequenceState = BattleSequenceState.MoveSelection;
        battleDialogBox.EnableActionSelector(false);
        battleDialogBox.EnableDialogText(false);
        battleDialogBox.EnableMoveSelector(true);
    }
    
    /// <summary>
    /// Opens the party screen.
    /// </summary>
    /// <param name="state">The party screen state (either party screen from selection or from fainted Uniteon)</param>
    private void OpenPartyScreen(BattleSequenceState state)
    {
        _battleSequenceState = state;
        partyScreen.gameObject.SetActive(true);
        partyScreen.AddUniteonsToPartySlots(_gamerParty.Uniteons);
    }

    /// <summary>
    /// Sets the state to battle over.
    /// </summary>
    /// <param name="won"></param>
    private void BattleOver(bool won)
    {
        _battleSequenceState = BattleSequenceState.BattleOver;
        _gamerParty.Uniteons.ForEach(u => u.OnBattleOver());
        OnBattleOver?.Invoke(won);
    }
    #endregion

    #region Selection Buttons
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
                AudioManager.Instance.PlaySfx(_audioClips["aButton"]);
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
                AudioManager.Instance.PlaySfx(_audioClips["aButton"]);
                MoveSelection();
            }
            else if (_actionSelection == 1) // Switch
            {
                AudioManager.Instance.PlaySfx(_audioClips["aButton"]);
                OpenPartyScreen(BattleSequenceState.PartyScreenSelect);
            }
            else if (_actionSelection == 2) // Pack
            {
                throw new NotImplementedException();
            }
            else if (_actionSelection == 3) // Run
            {
                AudioManager.Instance.StopMusic();
                AudioManager.Instance.StopSfx(2);
                AudioManager.Instance.PlaySfx(_audioClips["run"]);
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
            AudioManager.Instance.PlaySfx(_audioClips["aButton"]);
            battleDialogBox.EnableMoveSelector(false);
            battleDialogBox.EnableDialogText(true);
            StartCoroutine(ExecuteGamerMove());
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioManager.Instance.PlaySfx(_audioClips["aButton"]);
            battleDialogBox.EnableMoveSelector(false);
            battleDialogBox.EnableDialogText(true);
            ActionSelection();
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
                AudioManager.Instance.PlaySfx(_audioClips["thud"]);
                partyScreen.SetMessageText($"{selectedMember.UniteonBase.UniteonName} can't battle right now!");
                return;
            }
            if (selectedMember == uniteonUnitGamer.Uniteon)
            {
                AudioManager.Instance.PlaySfx(_audioClips["thud"]);
                partyScreen.SetMessageText($"{selectedMember.UniteonBase.UniteonName} is already on the field!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            StartCoroutine(SwitchUniteon(selectedMember));
        }
        // Enable going back to the action screen
        else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioManager.Instance.PlaySfx(_audioClips["thud"]);
            partyScreen.gameObject.SetActive(false);
            ActionSelection(); 
        }
    }
    #endregion

    #region Move Sequences
    /// <summary>
    /// The sequence for when the gamer attacks the foe.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator ExecuteGamerMove()
    {
        _battleSequenceState = BattleSequenceState.GamerIsMoving;
        var move = uniteonUnitGamer.Uniteon.Moves[_moveSelection];
        yield return ExecuteMove(uniteonUnitGamer, uniteonUnitFoe, move);
        // If the battle state was not changed by ExecuteMove, go to the next step
        if (_battleSequenceState == BattleSequenceState.GamerIsMoving)
            StartCoroutine(ExecuteFoeMove());
    }


    /// <summary>
    /// The sequence for when the foe attacks the gamer.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator ExecuteFoeMove()
    {
        _battleSequenceState = BattleSequenceState.FoeIsMoving;
        // Quite simple battle AI - but get a random move of the foe
        int randomMoveIndex = Random.Range(0, uniteonUnitFoe.Uniteon.Moves.Count);
        Move move = uniteonUnitFoe.Uniteon.Moves[randomMoveIndex];
        yield return ExecuteMove(uniteonUnitFoe, uniteonUnitGamer, move);
        // If the battle state was not changed by ExecuteMove, go to the next step
        if (_battleSequenceState == BattleSequenceState.FoeIsMoving)
            ActionSelection();
    }

    /// <summary>
    /// Encapsulates all the logic for executing a move sequence on the battlefield.
    /// </summary>
    /// <param name="attackingUnit">The attacking Uniteon.</param>
    /// <param name="defendingUnit">The defending Uniteon.</param>
    /// <param name="move">The executed move.</param>
    /// <returns></returns>
    private IEnumerator ExecuteMove(UniteonUnit attackingUnit, UniteonUnit defendingUnit, Move move)
    {
        // Determine the audio channel panning depending on who is attacking
        float panning = attackingUnit.IsGamerUniteon ? -0.72f : 0.72f;
        // Deduct PP for the executed move
        move.PowerPoints--;
        // Write move to the dialog box
        yield return battleDialogBox.TypeOutDialog($"{attackingUnit.Uniteon.UniteonBase.UniteonName} used {move.MoveBase.MoveName}!");
        // Play Uniteon attack animation and sfx
        AudioManager.Instance.PlaySfx(_audioClips["tackle"], panning: panning);
        yield return attackingUnit.PlayAttackAnimations();
        // Save health points before taking any damage
        int previousHealthPoints = defendingUnit.Uniteon.HealthPoints;
        // Check if move is a status move
        if (move.MoveBase.MoveCategory == MoveCategory.Status)
        {
            yield return MoveEffects(attackingUnit, defendingUnit, move, panning);
        }
        // If not status move, calculate damage
        else
        {
            DamageData damageData = defendingUnit.Uniteon.TakeDamage(move, attackingUnit.Uniteon);
            // Play Uniteon damage animation and sfx
            defendingUnit.PlayHitAnimation();
            PlayHitSfx(damageData, _battleSequenceState);
            // Show damage decay on health bar
            yield return defendingUnit.UniteonHud.UpdateHealthPoints(previousHealthPoints);
            // Write effectiveness/critical hit to the dialog box
            yield return WriteDamageData(damageData);
        }
        // If Uniteon fainted, write to dialog box, play faint animation and cry
        if (defendingUnit.Uniteon.HealthPoints <= 0)
        {
            AudioManager.Instance.StopSfx(2); // Stop low health sfx
            yield return battleDialogBox.TypeOutDialog($"{defendingUnit.Uniteon.UniteonBase.UniteonName} has fainted!");
            yield return defendingUnit.Uniteon.PlayCry(panning: -panning, fainted: true);
            AudioManager.Instance.PlaySfx(_audioClips["faint"], panning: -panning);
            yield return defendingUnit.PlayFaintAnimation();
            CheckBattleOver(defendingUnit);
        } 
    }

    /// <summary>
    /// Applies move effects to the battlefield.
    /// </summary>
    /// <param name="attacking">The attacking Uniteon.</param>
    /// <param name="defending">The defending Uniteon.</param>
    /// <param name="move">The executed move.</param>
    /// <param name="panning">The audio balance value for the sfx.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator MoveEffects(UniteonUnit attackingUnit, UniteonUnit defendingUnit, Move move, float panning)
    {
        MoveEffects effects = move.MoveBase.MoveEffects;
        if (effects != null)
        {
            bool statsRaised;
            if (move.MoveBase.MoveTarget == MoveTarget.Gamer)
            {
                statsRaised = attackingUnit.Uniteon.ApplyBoosts(effects.Boosts);
                if (statsRaised)
                {
                    attackingUnit.PlayStatRaisedAnimation(true);
                    AudioManager.Instance.PlaySfx(_audioClips["statRaised"], panning: panning);
                }
                else
                {
                    attackingUnit.PlayStatRaisedAnimation(false);
                    AudioManager.Instance.PlaySfx(_audioClips["statFell"], panning: panning);
                }
            }
            else
            {
                statsRaised = defendingUnit.Uniteon.ApplyBoosts(effects.Boosts);
                if (statsRaised)
                {
                    defendingUnit.PlayStatRaisedAnimation(true);
                    AudioManager.Instance.PlaySfx(_audioClips["statRaised"], panning: -panning);
                }
                else
                {
                    defendingUnit.PlayStatRaisedAnimation(false);
                    AudioManager.Instance.PlaySfx(_audioClips["statFell"], panning: -panning);
                }
            }
            yield return new WaitForSeconds(_audioClips["statRaised"].length); // It doesn't matter which one we use since they're the same length
        }
        yield return WriteStatusMessages(attackingUnit.Uniteon);
        yield return WriteStatusMessages(defendingUnit.Uniteon);
    }

    /// <summary>
    /// Checks if the battle is over and either sends out a new Uniteon or ends the battle.
    /// </summary>
    /// <param name="faintedUnit"></param>
    private void CheckBattleOver(UniteonUnit faintedUnit)
    {
        if (faintedUnit.IsGamerUniteon)
        { // Check if gamer has more Uniteon in it's party
            Uniteon nextUniteon = _gamerParty.GetHealthyUniteon();
            if (nextUniteon != null)
                OpenPartyScreen(BattleSequenceState.PartyScreenFaint);
            else
                BattleOver(false);
        }
        else
            BattleOver(true);
    }

    /// <summary>
    /// Checks who is allowed to move first.
    /// </summary>
    private void CheckFirstTurn()
    {
        bool gamerFirst = false;
        if (uniteonUnitGamer.Uniteon.Speed == uniteonUnitFoe.Uniteon.Speed) // If speeds are tied, randomize who goes first
            gamerFirst = (Random.Range(0, 2) == 0);
        if (uniteonUnitGamer.Uniteon.Speed > uniteonUnitFoe.Uniteon.Speed || gamerFirst)
            ActionSelection();
        else
            StartCoroutine(ExecuteFoeMove());
    }
    
    /// <summary>
    /// Send out new Uniteon into the battlefield.
    /// </summary>
    /// <param name="uniteon">The Uniteon that needs to be sent out.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator SwitchUniteon(Uniteon uniteon)
    {
        // Obtain if Uniteon fainted before switching and then set state to busy
        bool fainted = (_battleSequenceState == BattleSequenceState.PartyScreenFaint);
        _battleSequenceState = BattleSequenceState.Busy;
        // Only call back Uniteon if Uniteon didn't faint
        if (uniteonUnitGamer.Uniteon.HealthPoints > 0)
        {
            battleDialogBox.EnableActionSelector(false);
            // Call back Uniteon
            yield return battleDialogBox.TypeOutDialog(
                $"You've done well, {uniteonUnitGamer.Uniteon.UniteonBase.UniteonName}!");
            AudioManager.Instance.PlaySfx(_audioClips["switchOut"]);
            uniteonUnitGamer.PlayBattleLeaveAnimation();
            yield return new WaitForSeconds(2f);
        }
        // Send out new Uniteon
        yield return SendOutUniteon(uniteon);
        // Depending if Uniteon fainted in the last turn
        if (fainted)
            CheckFirstTurn();
        else
            StartCoroutine(ExecuteFoeMove()); // Give turn to foe
    }

    /// <summary>
    /// Sends out the next Uniteon into the battlefield.
    /// </summary>
    /// <param name="nextUniteon">The next Uniteon.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator SendOutUniteon(Uniteon nextUniteon)
    {
        // Initialise next uniteon
        uniteonUnitGamer.InitialiseUniteonUnit(nextUniteon);
        // Initialise the gamer side of battlefield
        battleDialogBox.SetMoveNames(nextUniteon.Moves);
        // Play cry
        StartCoroutine(nextUniteon.PlayCry(-0.72f));
        // Wait until motivational text has printed out
        yield return StartCoroutine(battleDialogBox.TypeOutDialog($"You got it, {nextUniteon.UniteonBase.UniteonName}!"));
        // Wait an additional second after the text is done printing
        yield return new WaitForSeconds(1f);
    }
    #endregion
    
    #region Move Helper Functions
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
            BattleSequenceState.GamerIsMoving => 0.72f,
            BattleSequenceState.FoeIsMoving => -0.72f,
            _ => throw new ArgumentOutOfRangeException(nameof(battleSequenceState), battleSequenceState, null)
        };
        AudioClip sfxClip = damageData.EffectivenessModifier switch
        {
            > 1f => _audioClips["hitSuperEffective"],
            < 1f => _audioClips["hitNotVeryEffective"],
            _ => _audioClips["hitNormalEffectiveness"]
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

    /// <summary>
    /// Writes all the status messages in the queue.
    /// </summary>
    /// <param name="uniteon">The Uniteon that has status messages.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator WriteStatusMessages(Uniteon uniteon)
    {
        while (uniteon.StatusMessages.Count > 0)
        {
            string message = uniteon.StatusMessages.Dequeue();
            yield return battleDialogBox.TypeOutDialog(message);
        }
    }
    #endregion
}

/// <summary>
/// All the different states a battle could be in.
/// </summary>
public enum BattleSequenceState
{
    Start,
    ActionSelection,
    MoveSelection,
    GamerIsMoving,
    FoeIsMoving,
    Busy,
    PartyScreenSelect,
    PartyScreenFaint,
    BattleOver
}

/// <summary>
/// Holds the audio clip and their accompanying name.
/// </summary>
[Serializable]
public struct UniteonSfx
{
    [SerializeField] private string clipName;
    [SerializeField] private AudioClip clip;

    public string ClipName => clipName;
    public AudioClip Clip => clip;

    /// <summary>
    /// Converts a list of UniteonSfx structs to a dictionary, since they're functionally the same and easier to manage.
    /// </summary>
    /// <param name="uniteonSfxList">The sfx list full of UniteonSfx structs.</param>
    /// <returns>The generated dictionary.</returns>
    public static Dictionary<string, AudioClip> ConvertListToDictionary(List<UniteonSfx> uniteonSfxList) => 
        uniteonSfxList.ToDictionary(sfx => sfx.ClipName, sfx => sfx.Clip);

}