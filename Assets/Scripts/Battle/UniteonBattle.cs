using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UniteonBattle : MonoBehaviour
{
    #region Fields
    [SerializeField] private Image fade;
    [SerializeField] private UniteonUnit uniteonUnitGamer;
    [SerializeField] private UniteonUnit uniteonUnitFoe;
    [SerializeField] private BattleDialogBox battleDialogBox;
    [SerializeField] private PartyScreen partyScreen;
    [SerializeField] private MoveBase defaultMove;
    [SerializeField] private Image gamerSprite; 
    [SerializeField] private Image foeSprite;
    [SerializeField] private MoveSelectionScreen moveSelectionScreen; 
    [SerializeField] private Image platforms;
    private Sprite _wildBattlePlatforms;
    private BattleSequenceState _battleState;
    private BattleSequenceState _prevBattleState;
    private CurrentBattleTurn _currentTurn;
    private int _actionSelection;
    private int _moveSelection;
    private int _memberSelection;
    private int _newMoveSelection;
    private Uniteon _wildUniteon;
    private UniteonParty _gamerParty;
    private UniteonParty _mentorParty;
    private bool _isMentorBattle;
    private bool _obtainUniteonAfterWin;
    private GamerController _gamerController;
    private MentorController _mentorController;
    private Vector3 _orgPosGamerSprite;
    private Vector3 _orgPosFoeSprite;
    private MoveBase _moveToLearn;
    #endregion
    
    #region Debug
    private BattleSequenceState BattleState
    {
        get => _battleState;
        set
        {
            _battleState = value;
            Debug.Log("BattleState changed: " + _battleState);
        }
    }

    private CurrentBattleTurn CurrentTurn
    {
        get => _currentTurn;
        set
        {
            _currentTurn = value;
            Debug.Log("CurrentTurn changed: " + _currentTurn);
        }
    }
    #endregion
    
    #region Events
    public event Action<BattleOverState> OnBattleOver;
    #endregion

    #region Initialisation
    /// <summary>
    /// Starts a wild Uniteon battle.
    /// </summary>
    public void StartBattle(UniteonParty gamerParty, Uniteon wildUniteon, bool obtainUniteonAfterWin = false)
    {
        _gamerParty = gamerParty;
        _wildUniteon = wildUniteon;
        _obtainUniteonAfterWin = obtainUniteonAfterWin;
        if (!ReferenceEquals(_wildBattlePlatforms, null))
            platforms.sprite = _wildBattlePlatforms;
        StartCoroutine(InitialiseBattle());
    }
    
    /// <summary>
    /// Starts a Mentor battle.
    /// </summary>
    public void StartBattle(UniteonParty gamerParty, UniteonParty mentorParty)
    {
        _gamerParty = gamerParty;
        _mentorParty = mentorParty;
        _isMentorBattle = true;
        _gamerController = gamerParty.GetComponent<GamerController>();
        _mentorController = mentorParty.GetComponent<MentorController>();
        _wildBattlePlatforms ??= platforms.sprite; // Cache wild battle platforms for wild Uniteon battles
        platforms.sprite = _mentorController.BattlePlatforms;
        StartCoroutine(InitialiseBattle());
    }

    /// <summary>
    /// Initializes the battle scene by sending out Uniteon(s).
    /// </summary>
    private IEnumerator InitialiseBattle()
    {
        BattleState = BattleSequenceState.Start;
        // Fade in
        fade.gameObject.SetActive(true);
        fade.color = new Color(1f, 1f, 1f, 1f);
        fade.DOFade(0f, 0.72f);
        // Disable HUD
        uniteonUnitGamer.DisableHud();
        uniteonUnitFoe.DisableHud();
        // Initialise party screen
        partyScreen.InitialisePartyScreen();
        if (_isMentorBattle)
        {
            // Show gamer and foe sprites
            uniteonUnitGamer.gameObject.SetActive(false);
            uniteonUnitFoe.gameObject.SetActive(false);
            gamerSprite.gameObject.SetActive(true);
            foeSprite.gameObject.SetActive(true);
            _orgPosGamerSprite = gamerSprite.rectTransform.localPosition;
            _orgPosFoeSprite = foeSprite.rectTransform.localPosition;
            gamerSprite.sprite = _gamerController.Sprite;
            foeSprite.sprite = _mentorController.Sprite;
            gamerSprite.color = Color.white;
            foeSprite.color = Color.white;
            yield return battleDialogBox.TypeOutDialog($"{_mentorController.MentorName} wants to fight!", 1.72f);
            // Send out first Uniteon of the foe
            PlayCharacterLeaveAnimation(false);
            uniteonUnitFoe.gameObject.SetActive(true);
            Uniteon foeUniteon = _mentorParty.GetHealthyUniteon();
            uniteonUnitFoe.InitialiseUniteonUnit(foeUniteon);
            StartCoroutine(uniteonUnitFoe.Uniteon.PlayCry(0.72f));
            yield return battleDialogBox.TypeOutDialog($"{_mentorController.MentorName} sent out {foeUniteon.UniteonBase.UniteonName}!", 1.27f);
            // Send out first Uniteon of the gamer
            PlayCharacterLeaveAnimation(true);
            uniteonUnitGamer.gameObject.SetActive(true);
            Uniteon gamerUniteon = _gamerParty.GetHealthyUniteon();
            uniteonUnitGamer.InitialiseUniteonUnit(gamerUniteon);
            battleDialogBox.SetMoveNames(gamerUniteon.Moves);
            StartCoroutine(uniteonUnitGamer.Uniteon.PlayCry(-0.72f));
            yield return battleDialogBox.TypeOutDialog($"Go get 'em, {gamerUniteon.UniteonBase.UniteonName}!", 1.27f);
        }
        else
        {
            // Initialise Uniteons
            uniteonUnitGamer.InitialiseUniteonUnit(_gamerParty.GetHealthyUniteon());
            uniteonUnitFoe.InitialiseUniteonUnit(_wildUniteon);
            // Initialise battle dialog box
            battleDialogBox.SetMoveNames(uniteonUnitGamer.Uniteon.Moves);
            // Wait until wild encounter text has printed out
            StartCoroutine(uniteonUnitFoe.Uniteon.PlayCry(0.72f));
            yield return StartCoroutine(battleDialogBox.TypeOutDialog($"A wild {uniteonUnitFoe.Uniteon.UniteonBase.UniteonName} appeared!", 1.72f));
        }
        ActionSelection(); // Let gamer select action
    }
    #endregion
    
    #region Update
    /// <summary>
    /// Every frame, check for a change in state, and then handle accordingly by the GameController.
    /// </summary>
    public void ControllerUpdate()
    {
        switch (BattleState)
        {
            case BattleSequenceState.ActionSelection:
                HandleActionSelection();
                break;
            case BattleSequenceState.MoveSelection:
                HandleMoveSelection();
                break;
            case BattleSequenceState.NoMoveSelection:
                HandleNoMovesLeft();
                break;
            case BattleSequenceState.PartyScreenFromSelection or BattleSequenceState.PartyScreenFromFaint:
                HandlePartyScreenSelection();
                break;
            case BattleSequenceState.MoveToForget:
                HandleNewMoveSelection();
                break;
        }
    }
    #endregion
    
    #region State Selection
    /// <summary>
    /// Transitions the scene state to action.
    /// </summary>
    private void ActionSelection()
    {
        Debug.Log($"{nameof(ActionSelection)} called in battle state: {BattleState}");
        BattleState = BattleSequenceState.ActionSelection;
        battleDialogBox.SetDialogText("Choose a strategic move...");
        battleDialogBox.EnableActionSelector(true);
    }

    /// <summary>
    /// Transitions the scene state to move.
    /// </summary>
    private void MoveSelection()
    {
        battleDialogBox.EnableActionSelector(false);
        if (uniteonUnitGamer.Uniteon.GetRandomMove() == null)
            BattleState = BattleSequenceState.NoMoveSelection;
        else
        {
            BattleState = BattleSequenceState.MoveSelection;
            battleDialogBox.EnableDialogText(false);
            battleDialogBox.EnableMoveSelector(true);
        }
    }

    /// <summary>
    /// Opens the party screen.
    /// </summary>
    private void OpenPartyScreen(BattleSequenceState stateWhichOpenedPartyScreen)
    {
        BattleState = stateWhichOpenedPartyScreen;
        partyScreen.gameObject.SetActive(true);
        partyScreen.AddUniteonsToPartySlots(_gamerParty.Uniteons);
    }

    /// <summary>
    /// Initiates the move to forget UI.
    /// </summary>
    /// <param name="uniteon">The Uniteon that wants to learn a new move.</param>
    /// <param name="newMove">The new move it wants to learn.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator ChooseMoveToForget(Uniteon uniteon, MoveBase newMove)
    {
        BattleState = BattleSequenceState.MoveToForget;
        yield return battleDialogBox.TypeOutDialog($"Choose a move you want to forget.");
        moveSelectionScreen.gameObject.SetActive(true);
        moveSelectionScreen.SetMoveNames(uniteon.Moves.Select(x => x.MoveBase).ToList(), newMove);
        _moveToLearn = newMove;
    }

    /// <summary>
    /// Handles running away from the battlefield.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator RunAway()
    {
        if (_isMentorBattle)
        {
            AudioManager.Instance.PlaySfx("thud");
            BattleState = BattleSequenceState.Printing;
            battleDialogBox.EnableActionSelector(false);
            yield return battleDialogBox.TypeOutDialog("You can't run from mentor battles!", lineWidth: 20);
            battleDialogBox.EnableActionSelector(true);
            BattleState = BattleSequenceState.ActionSelection;
        }
        else
        {
            BattleState = BattleSequenceState.BattleOver;
            AudioManager.Instance.PlaySfx("run");
            battleDialogBox.EnableActionSelector(false);
            yield return battleDialogBox.TypeOutDialog("You ran away!");
            yield return HandleBattleOver(BattleOverState.Ran);
        }
    }

    /// <summary>
    /// Sets the state to battle over and clean up the class.
    /// </summary>
    private IEnumerator HandleBattleOver(BattleOverState overState)
    {
        // Wait until the gamer continues
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));
        // Add Uniteon to gamer's party if they won and party isn't full
        if (overState == BattleOverState.Won && _obtainUniteonAfterWin && _gamerParty.Uniteons.Count < 6)
        {
            AudioManager.Instance.PlaySfx("levelUp");
            yield return battleDialogBox.TypeOutDialog(
                $"{uniteonUnitFoe.Uniteon.UniteonBase.UniteonName} has been added to your party!");
            _gamerParty.Uniteons.Add(uniteonUnitFoe.Uniteon);
            _gamerParty.FullHealAllUniteons();
        }
        // Fade out screen and music, disable channel 2 sfx
        AudioManager.Instance.StopSfx(2); // Stop any playing channel 2 SFX (low health)
        fade.color = new Color(0f, 0f, 0f, 0f);
        StartCoroutine(AudioManager.Instance.StopMusic(true, 0.72f));
        yield return fade.DOFade(1f, 0.72f).WaitForCompletion();
        _gamerParty.Uniteons.ForEach(u => u.ResetBoosts());
        // Heal party Uniteon if gamer has lost, and the battle didn't end because of running away
        if (overState == BattleOverState.Lost)
            _gamerParty.FullHealAllUniteons();
        OnBattleOver?.Invoke(overState);
        // Reset variables
        partyScreen.ResetAllPartyMemberSlots();
        _actionSelection = 0;
        _moveSelection = 0;
        _memberSelection = 0;
        _wildUniteon = null;
        _gamerParty = null;
        _mentorParty = null;
        _isMentorBattle = false;
        _gamerController = null;
        _mentorController = null;
        _obtainUniteonAfterWin = false;
    }
    #endregion

    #region Selection Buttons
    /// <summary>
    /// Handles the selection of multiple buttons in a grid array.
    /// </summary>
    /// <param name="selection">The selection parameter you want to modify.</param>
    /// <param name="upperBound">The maximum value of selectable buttons.</param>
    /// <returns>The current selection.</returns>
    private int HandleGridSelectionButtons(int selection, int upperBound)
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
                AudioManager.Instance.PlaySfx("aButton");
            }
        }
        return Mathf.Clamp(selection, 0, upperBound);
    }

    /// <summary>
    /// Handles the selection of multiple buttons in a list array.
    /// </summary>
    /// <param name="selection">The selection parameter you want to modify.</param>
    /// <param name="upperBound">The maximum value of selectable buttons.</param>
    /// <returns>The current selection.</returns>
    private int HandleListSelectionButtons(int selection, int upperBound)
    {
        int previousSelection = selection;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            selection++;
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            selection--;
        // Clamp the selection within the valid range
        selection = Mathf.Clamp(selection, 0, upperBound);
        if (selection != previousSelection)
            AudioManager.Instance.PlaySfx("aButton");
        return selection;
    }
    
    /// <summary>
    /// Watches for input of the player and changes the graphics accordingly in the action selection state.
    /// </summary>
    private void HandleActionSelection()
    {
        _actionSelection = HandleGridSelectionButtons(_actionSelection, 3);
        battleDialogBox.UpdateActionSelection(_actionSelection);
        // Transition to the next state if an action has been selected
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_actionSelection == 0) // Attack
            {
                AudioManager.Instance.PlaySfx("aButton");
                MoveSelection();
            }
            else if (_actionSelection == 1) // Switch
            {
                AudioManager.Instance.PlaySfx("aButton");
                OpenPartyScreen(BattleSequenceState.PartyScreenFromSelection);
            }
            else if (_actionSelection == 2) // Pack
            {
                AudioManager.Instance.PlaySfx("thud"); // Not implemented
            }
            else if (_actionSelection == 3) // Run
            {
                StartCoroutine(RunAway());
            }
        }
    }

    /// <summary>
    /// Watches for input of the player and changes the graphics accordingly in the move selection state.
    /// </summary>
    private void HandleMoveSelection()
    {
        _moveSelection = HandleGridSelectionButtons(_moveSelection, uniteonUnitGamer.Uniteon.Moves.Count - 1);
        battleDialogBox.UpdateMoveSelection(_moveSelection, uniteonUnitGamer.Uniteon.Moves[_moveSelection]);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            var move = uniteonUnitGamer.Uniteon.Moves[_moveSelection];
            if (move.PowerPoints == 0) 
                AudioManager.Instance.PlaySfx("thud");
            else
            {
                AudioManager.Instance.PlaySfx("aButton");
                battleDialogBox.EnableMoveSelector(false);
                battleDialogBox.EnableDialogText(true);
                StartCoroutine(ExecuteBattleTurns(BattleAction.Move));
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioManager.Instance.PlaySfx("aButton");
            battleDialogBox.EnableMoveSelector(false);
            battleDialogBox.EnableDialogText(true);
            ActionSelection();
        }
    }
    
    private void HandleNewMoveSelection()
    {
        _newMoveSelection = HandleListSelectionButtons(_newMoveSelection, UniteonBase.MaxMoves);
        moveSelectionScreen.HighlightSelectionInList(_newMoveSelection);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            AudioManager.Instance.PlaySfx("aButton");
            StartCoroutine(LearnNewMoveWhenMoveSlotsFull());
        }
    }

    /// <summary>
    /// Watches for inputs in the party selection screen and sends out a new Uniteon if one got selected.
    /// </summary>
    private void HandlePartyScreenSelection()
    {
        _memberSelection = HandleGridSelectionButtons(_memberSelection, _gamerParty.Uniteons.Count - 1);
        partyScreen.UpdateMemberSelection(_memberSelection);
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Uniteon selectedUniteon = _gamerParty.Uniteons[_memberSelection];
            if (selectedUniteon.HealthPoints <= 0)
            {
                AudioManager.Instance.PlaySfx("thud");
                partyScreen.SetMessageText($"{selectedUniteon.UniteonBase.UniteonName} can't battle right now!");
                return;
            }
            if (selectedUniteon == uniteonUnitGamer.Uniteon)
            {
                AudioManager.Instance.PlaySfx("thud");
                partyScreen.SetMessageText($"{selectedUniteon.UniteonBase.UniteonName} is already on the field!");
                return;
            }
            partyScreen.gameObject.SetActive(false);
            // Check in which state the party selection screen was opened and then determine what to do
            if (BattleState == BattleSequenceState.PartyScreenFromSelection)
            {
                AudioManager.Instance.PlaySfx("aButton");
                StartCoroutine(ExecuteBattleTurns(BattleAction.Switch));
            }
            else if (BattleState == BattleSequenceState.PartyScreenFromFaint)
            {
                AudioManager.Instance.PlaySfx("aButton");
                StartCoroutine(SwitchUniteon(selectedUniteon));
            }
        }
        // Enable going back to the action screen
        else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Backspace))
        {
            AudioManager.Instance.PlaySfx("aButton");
            partyScreen.gameObject.SetActive(false);
            ActionSelection(); 
        }
    }
    
    /// <summary>
    /// Handles when the gamer clicked on Attack without any usable moves left.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private void HandleNoMovesLeft()
    {
        StartCoroutine(ExecuteBattleTurns(BattleAction.Move));
    }
    #endregion

    #region Move Sequences
    private IEnumerator ExecuteBattleTurns(BattleAction gamerAction) // Only gamers action as foe cannot perform any other action than move
    {
        if (gamerAction == BattleAction.Move)
        {
            // Get the selected move for the gamer, or if none available, get Struggle
            if (BattleState == BattleSequenceState.NoMoveSelection)
                uniteonUnitGamer.Uniteon.ExecutingMove = new Move(defaultMove);
            else
                uniteonUnitGamer.Uniteon.ExecutingMove = uniteonUnitGamer.Uniteon.Moves[_moveSelection];
            BattleState = BattleSequenceState.ExecutingTurns;
            // Quite simple battle AI - but get a random move of the foe, and if no move is available, struggle
            uniteonUnitFoe.Uniteon.ExecutingMove = uniteonUnitFoe.Uniteon.GetRandomMove() ?? new Move(defaultMove);
            // Check who goes first
            int gamerMovePriority = uniteonUnitGamer.Uniteon.ExecutingMove.MoveBase.Priority;
            int foeMovePriority = uniteonUnitFoe.Uniteon.ExecutingMove.MoveBase.Priority;
            bool gamerFirst = false;
            if (gamerMovePriority > foeMovePriority)
                gamerFirst = true;
            else if (gamerMovePriority == foeMovePriority)
            {
                if (uniteonUnitGamer.Uniteon.Speed ==
                    uniteonUnitFoe.Uniteon.Speed) // If speeds are tied, randomize who goes first
                    gamerFirst = (Random.Range(0, 2) == 0);
                if (uniteonUnitGamer.Uniteon.Speed > uniteonUnitFoe.Uniteon.Speed || gamerFirst)
                    gamerFirst = true;
            }
            // Assign first unit var to the unit who is allowed to move first
            UniteonUnit firstUnit = (gamerFirst) ? uniteonUnitGamer : uniteonUnitFoe;
            UniteonUnit secondUnit = (gamerFirst) ? uniteonUnitFoe : uniteonUnitGamer;
            // First turn
            CurrentTurn = CurrentBattleTurn.First; // Declare first turn
            yield return ExecuteMove(firstUnit, secondUnit, firstUnit.Uniteon.ExecutingMove);
            yield return new WaitUntil(() => BattleState == BattleSequenceState.ExecutingTurns);
            if (BattleState == BattleSequenceState.BattleOver)
                yield break; // Stop the coroutine if battle over or second Uniteon has no move
            // Second turn
            if (CurrentTurn != CurrentBattleTurn.NoTurn) // If the second Uniteon has no move, skip turn
            {
                CurrentTurn = CurrentBattleTurn.Second; // Declare second turn
                if (secondUnit.Uniteon.HealthPoints > 0)
                    yield return ExecuteMove(secondUnit, firstUnit, secondUnit.Uniteon.ExecutingMove);
                else if (secondUnit.IsGamerUniteon)
                    BattleState = BattleSequenceState.PartyScreenFromFaint;
            }
        }
        else if (gamerAction == BattleAction.Switch)
        {
            // Switch Uniteon
            CurrentTurn = CurrentBattleTurn.First;
            Uniteon switchingOutUniteon = _gamerParty.Uniteons[_memberSelection];
            yield return SwitchUniteon(switchingOutUniteon);
            // Give turn to foe
            CurrentTurn = CurrentBattleTurn.Second;
            uniteonUnitFoe.Uniteon.ExecutingMove = uniteonUnitFoe.Uniteon.Moves[Random.Range(0, uniteonUnitFoe.Uniteon.Moves.Count)];
            yield return ExecuteMove(uniteonUnitFoe, uniteonUnitGamer, uniteonUnitFoe.Uniteon.ExecutingMove);
        }
        if (BattleState is not BattleSequenceState.BattleOver and not BattleSequenceState.PartyScreenFromFaint)
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
        if (move.MoveBase.PowerPoints < 0) // Only Struggle has -1 PP by default, so if the move is Struggle, no moves are left
            yield return battleDialogBox.TypeOutDialog($"{attackingUnit.Uniteon.UniteonBase.UniteonName} has no moves left!");
        else
            move.PowerPoints--; // Deduct PP for the executed move
        // Write move to the dialog box
        yield return battleDialogBox.TypeOutDialog($"{attackingUnit.Uniteon.UniteonBase.UniteonName} used {move.MoveBase.MoveName}!");
        // Play Uniteon attack animation and sfx
        AudioManager.Instance.PlaySfx("tackle", panning: panning);
        yield return attackingUnit.PlayAttackAnimations();
        // Check if the move missed
        if (!CheckIfMoveHits(move, attackingUnit.Uniteon, defendingUnit.Uniteon))
        {
            yield return battleDialogBox.TypeOutDialog(
                $"{attackingUnit.Uniteon.UniteonBase.UniteonName}'s attack missed!");
            BattleState = BattleSequenceState.ExecutingTurns;
        }
        else // If the move hits
        {
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
                // Play Uniteon damage animation and sfx if the move was effective
                if (damageData.EffectivenessModifier > 0f)
                {
                    defendingUnit.PlayHitAnimation(damageData.EffectivenessModifier);
                    PlayHitSfx(damageData, -panning);
                    // Show damage decay on health bar
                    yield return defendingUnit.UniteonHud.UpdateHealthPoints(previousHealthPoints);
                }
                // Write effectiveness/critical hit to the dialog box
                yield return WriteDamageData(damageData);
            }
            // If Uniteon fainted, write to dialog box, play faint animation and cry
            if (defendingUnit.Uniteon.HealthPoints <= 0)
            {
                if (!_isMentorBattle) // Stop low health sfx if it's not a mentor battle
                    AudioManager.Instance.StopSfx(2);
                yield return battleDialogBox.TypeOutDialog(
                    $"{defendingUnit.Uniteon.UniteonBase.UniteonName} fainted!");
                yield return defendingUnit.Uniteon.PlayCry(panning: -panning, fainted: true);
                AudioManager.Instance.PlaySfx("faint", panning: -panning);
                yield return defendingUnit.PlayFaintAnimation();
                if (!defendingUnit.IsGamerUniteon)
                {
                    // If mentor out of Uniteon, or wild Uniteon fainted, battle over
                    if ((_isMentorBattle && _mentorParty.GetHealthyUniteon() == null) || !_isMentorBattle)
                    {
                        AudioClip victoryIntro = AudioManager.Music["victoryMentorIntro"];
                        AudioClip victoryLoop = AudioManager.Music["victoryMentorLoop"];
                        if (_mentorController != null)
                        {
                            victoryIntro = _mentorController.VictoryIntro;
                            victoryLoop = _mentorController.VictoryLoop;
                        }
                        BattleState = BattleSequenceState.BattleOver;
                        AudioManager.Instance.StopSfx(2); // Stop low health SFX
                        AudioManager.Instance.PlayMusic(victoryIntro, victoryLoop);
                    }
                    // Gain experience for gamer Uniteon if they are below level 100
                    if (uniteonUnitGamer.Uniteon.Level < 100)
                    {
                        var expYield = defendingUnit.Uniteon.UniteonBase.BaseExperience;
                        int foeLevel = defendingUnit.Uniteon.Level;
                        float mentorBonus = (_isMentorBattle) ? 1.5f : 1f; // 1.5x bonus for mentor Uniteon
                        // Calculate experience
                        int experienceGained = Mathf.FloorToInt((expYield * foeLevel * mentorBonus) / 7);
                        uniteonUnitGamer.Uniteon.Experience += experienceGained;
                        // Print experience message
                        yield return battleDialogBox.TypeOutDialog(
                            $"{uniteonUnitGamer.Uniteon.UniteonBase.UniteonName} gained {experienceGained} experience points!");
                        // Update exp bar
                        AudioManager.Instance.PlaySfx("expRaise");
                        yield return uniteonUnitGamer.UniteonHud.UpdateExperienceBar();
                        // Check for level up until level 100
                        while (uniteonUnitGamer.Uniteon.CheckLevelUp() && uniteonUnitGamer.Uniteon.Level < 100)
                        {
                            uniteonUnitGamer.Uniteon.CalculateStats(true);
                            uniteonUnitGamer.UniteonHud.UpdateLevelAndHealth();
                            AudioManager.Instance.PlaySfx("levelUp");
                            yield return battleDialogBox.TypeOutDialog(
                                $"{uniteonUnitGamer.Uniteon.UniteonBase.UniteonName} grew to level {uniteonUnitGamer.Uniteon.Level}!");
                            AudioManager.Instance.PlaySfx("expRaise");
                            yield return uniteonUnitGamer.UniteonHud.UpdateExperienceBar(true);
                            // Learn new move
                            var newMove = uniteonUnitGamer.Uniteon.GetLearnableMoveAtCurrentLevel();
                            if (!ReferenceEquals(newMove, null))
                            {
                                // If there are empty move slots, just learn the move
                                if (uniteonUnitGamer.Uniteon.Moves.Count < UniteonBase.MaxMoves)
                                {
                                    uniteonUnitGamer.Uniteon.LearnMove(newMove);
                                    AudioManager.Instance.PlaySfx("levelUp");
                                    yield return battleDialogBox.TypeOutDialog(
                                        $"{uniteonUnitGamer.Uniteon.UniteonBase.UniteonName} learned {newMove.MoveBase.MoveName}!");
                                    battleDialogBox.SetMoveNames(uniteonUnitGamer.Uniteon.Moves);
                                }
                                // Give gamer selection what move to forgets
                                else
                                {
                                    yield return battleDialogBox.TypeOutDialog(
                                        $"{uniteonUnitGamer.Uniteon.UniteonBase.UniteonName} wants to learn a new move, but it can only learn {UniteonBase.MaxMoves} at a time.");
                                    yield return ChooseMoveToForget(uniteonUnitGamer.Uniteon, newMove.MoveBase);
                                    yield return new WaitUntil(() => BattleState != BattleSequenceState.MoveToForget);
                                    yield return new WaitForSeconds(1.27f);
                                }
                            }
                        }
                    }
                }
                yield return CheckBattleOver(defendingUnit);
            }
        }
    }

    /// <summary>
    /// Checks if the battle is over and either sends out a new Uniteon or ends the battle.
    /// </summary>
    /// <param name="faintedUnit">The Uniteon who fainted.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator CheckBattleOver(UniteonUnit faintedUnit)
    {
        if (BattleState == BattleSequenceState.BattleOver)
            yield return HandleBattleOver(BattleOverState.Won);
        else if (faintedUnit.IsGamerUniteon)
        { // Check if gamer has more Uniteon in it's party
            Uniteon nextUniteon = _gamerParty.GetHealthyUniteon();
            if (nextUniteon != null) // If gamer has, open party screen
                OpenPartyScreen(BattleSequenceState.PartyScreenFromFaint);
            else
            {
                // If not, lost
                yield return HandleBattleOver(BattleOverState.Lost);
            }
        }
        else if (_isMentorBattle) // If it's a mentor battle, just send out next Uniteon
        {
            Uniteon nextUniteon = _mentorParty.GetHealthyUniteon(); // Just get the next Uniteon in the party
            if (!ReferenceEquals(nextUniteon, null))
                yield return SwitchMentorUniteon(nextUniteon);
        }
    }
    
    /// <summary>
    /// Applies move effects to the battlefield.
    /// </summary>
    /// <param name="attackingUnit">The attacking Uniteon unit.</param>
    /// <param name="defendingUnit">The defending Uniteon unit.</param>
    /// <param name="move">The executed move.</param>
    /// <param name="panning">The audio balance value for the sfx.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator MoveEffects(UniteonUnit attackingUnit, UniteonUnit defendingUnit, Move move, float panning)
    {
        MoveEffects effects = move.MoveBase.MoveEffects;
        if (effects != null)
        {
            bool statsRaised = false;
            UniteonUnit targetUnit = (move.MoveBase.MoveTarget == MoveTarget.Gamer) ? attackingUnit : defendingUnit;
            statsRaised = targetUnit.Uniteon.ApplyBoosts(effects.Boosts);
            targetUnit.PlayStatRaisedAnimation(statsRaised);
            string sfxClip = (statsRaised) ? "statRaised" : "statFell";
            float audioPanning = (move.MoveBase.MoveTarget == MoveTarget.Gamer) ? panning : -panning;
            AudioManager.Instance.PlaySfx(sfxClip, panning: audioPanning);
            yield return new WaitForSeconds(AudioManager.Sfx[sfxClip].length);
        }
        yield return WriteStatusMessages(attackingUnit.Uniteon);
        yield return WriteStatusMessages(defendingUnit.Uniteon);
    }

    /// <summary>
    /// Checks if a move hits by checking the accuracy and the modifiers.
    /// </summary>
    /// <param name="move">The move that got executed.</param>
    /// <param name="attacker">The attacking Uniteon to check its accuracy.</param>
    /// <param name="defender">The defending Uniteon to check its evasiveness.</param>
    /// <returns>True if hit and false if miss.</returns>
    private bool CheckIfMoveHits(Move move, Uniteon attacker, Uniteon defender)
    {
        if (move.MoveBase.NeverMisses)
            return true;
        float moveAccuracy = move.MoveBase.Accuracy;
        int accuracy = attacker.StatBoosts[Statistic.Accuracy];
        int evasion = defender.StatBoosts[Statistic.Evasion];
        float[] boostValues = new float[] { 1f, 4f / 3f, 3f, 5f / 3f, 2f, 7f / 3f, 8f / 3f, 3f }; // Multiplication amounts for each boost
        if (accuracy > 0)
            moveAccuracy *= boostValues[accuracy];
        else
            moveAccuracy /= boostValues[-accuracy];
        if (evasion > 0)
            moveAccuracy *= boostValues[evasion];
        else
            moveAccuracy /= boostValues[-evasion];
        return Random.Range(1, 101) <= moveAccuracy;
    }
    #endregion
    
    #region Sending Out Uniteon
    /// <summary>
    /// Send out new Uniteon into the battlefield.
    /// </summary>
    /// <param name="uniteon">The Uniteon that needs to be sent out.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator SwitchUniteon(Uniteon uniteon)
    {
        BattleSequenceState switchFromState = BattleState;
        BattleState = BattleSequenceState.Switching;
        // Call back Uniteon if switched
        if (switchFromState == BattleSequenceState.PartyScreenFromSelection)
        {
            battleDialogBox.EnableActionSelector(false);
            yield return battleDialogBox.TypeOutDialog(
                $"You've done well, {uniteonUnitGamer.Uniteon.UniteonBase.UniteonName}!");
            AudioManager.Instance.PlaySfx("switchOut");
            yield return uniteonUnitGamer.PlayBattleLeaveAnimation();
        }
        // Send out new Uniteon, initialise dialog box of gamer side of battlefield, play cry, print text
        uniteonUnitGamer.InitialiseUniteonUnit(uniteon);
        battleDialogBox.SetMoveNames(uniteon.Moves);
        StartCoroutine(uniteon.PlayCry(-0.72f));
        yield return battleDialogBox.TypeOutDialog($"You got it, {uniteon.UniteonBase.UniteonName}!", 1.27f);
        // If Uniteon was switched by self, continue the turn
        if (switchFromState == BattleSequenceState.PartyScreenFromSelection)
        {
            BattleState = BattleSequenceState.ExecutingTurns;
        }
        // If Uniteon was switched in because previous one fainted...
        else if (switchFromState == BattleSequenceState.PartyScreenFromFaint)
        {
            // If they got switched in during the first turn, continue the turn, but let know that we have no move
            if (CurrentTurn == CurrentBattleTurn.First)
            {
                BattleState = BattleSequenceState.ExecutingTurns;
                CurrentTurn = CurrentBattleTurn.NoTurn;
            }
            // If they got switched in during the second turn, go back to action selection
            else if (CurrentTurn == CurrentBattleTurn.Second)
            {
                ActionSelection();
            }
        }
    }

    /// <summary>
    /// Sends out a mentor's Uniteon into the battlefield. 
    /// </summary>
    /// <param name="uniteon">The Uniteon that needs to be sent out.</param>
    /// <returns>Coroutine.</returns>
    private IEnumerator SwitchMentorUniteon(Uniteon uniteon)
    {
        BattleState = BattleSequenceState.FoeSwitching;
        uniteonUnitFoe.InitialiseUniteonUnit(uniteon);
        StartCoroutine(uniteon.PlayCry(0.72f));
        yield return battleDialogBox.TypeOutDialog(
            $"{_mentorController.MentorName} sent out {uniteon.UniteonBase.UniteonName}!", 1.27f);
        CurrentTurn = CurrentBattleTurn.NoTurn;
        BattleState = BattleSequenceState.ExecutingTurns;
    }
    #endregion
    
    #region Helper Functions
    /// <summary>
    /// Plays the Uniteon hit sound effect.
    /// </summary>
    /// <param name="damageData">The damage data of the Uniteon.</param>
    /// <param name="panning">The balance value from left to right to indicate where the faining sound comes from.</param>
    private void PlayHitSfx(DamageData damageData, float panning)
    {
        string sfxClip = damageData.EffectivenessModifier switch
        {
            > 1f => "hitSuperEffective",
            < 1f => "hitNotVeryEffective",
            _ => "hitNormalEffectiveness"
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
        if (damageData.CriticalHitModifier > 1f && damageData.EffectivenessModifier > 0)
            yield return battleDialogBox.TypeOutDialog("It's a critical hit!");
        switch (damageData.EffectivenessModifier)
        {
            case 4f:
                yield return battleDialogBox.TypeOutDialog("It's mega effective!!!");
                break;
            case 2f:
                yield return battleDialogBox.TypeOutDialog("It's super effective!");
                break;
            case 0.5f or 0.25f:
                yield return battleDialogBox.TypeOutDialog("It's not very effective...");
                break;
            case 0f:
                yield return battleDialogBox.TypeOutDialog("The move had no effect.");
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
    
    /// <summary>
    /// Forgets and learns a new move based on the current selection or decides to not learn it.
    /// </summary>
    /// <returns>Coroutine.</returns>
    private IEnumerator LearnNewMoveWhenMoveSlotsFull()
    {
        moveSelectionScreen.gameObject.SetActive(false);
        // Don't learn new move
        if (_newMoveSelection == UniteonBase.MaxMoves)
        {
            yield return battleDialogBox.TypeOutDialog(
                $"{uniteonUnitGamer.Uniteon.UniteonBase.UniteonName} did not learn {_moveToLearn.MoveName}.");
        }
        // Forget selected move and learn new move
        else
        {
            MoveBase selectedMove = uniteonUnitGamer.Uniteon.Moves[_newMoveSelection].MoveBase;
            yield return battleDialogBox.TypeOutDialog(
                $"{uniteonUnitGamer.Uniteon.UniteonBase.UniteonName} learned {_moveToLearn.MoveName} and forgot {selectedMove.MoveName}!");
            uniteonUnitGamer.Uniteon.Moves[_newMoveSelection] = new Move(_moveToLearn);
            battleDialogBox.SetMoveNames(uniteonUnitGamer.Uniteon.Moves); // Reload moves
        }
        _moveToLearn = null;
        BattleState = _isMentorBattle ? BattleSequenceState.ExecutingTurns : BattleSequenceState.BattleOver;
    }
    #endregion

    #region Animations
    /// <summary>
    /// Animates the leave animation for the gamer and the foe.
    /// </summary>
    /// <param name="isGamer">If the gamer needs to be animated (true) or the foe (false).</param>
    private void PlayCharacterLeaveAnimation(bool isGamer)
    {
        // Set variables
        Image sprite;
        Vector3 orgPosSprite;
        float moveX;
        float moveY;
        const float duration = 0.72f;
        Sequence sequence = DOTween.Sequence();
        // Determine variables
        if (isGamer)
        {
            sprite = gamerSprite;
            orgPosSprite = _orgPosGamerSprite;
            moveX = -75f;
            moveY = -37.5f;
        }
        else
        {
            sprite = foeSprite;
            orgPosSprite = _orgPosFoeSprite;
            moveX = 75f;
            moveY = 37.5f;
        }
        // Execute animation
        sequence.Append(
            sprite.transform.DOLocalMove(new Vector3(orgPosSprite.x + moveX, orgPosSprite.y + moveY), duration));
        sequence.Join(sprite.DOFade(0f, duration));
        // Reset sprite to original position
        sequence.OnComplete(() =>
        {
            sprite.transform.localPosition = orgPosSprite;
        });
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
    NoMoveSelection, // If there are no remaining moves
    ExecutingTurns,
    Switching,
    Printing,
    FoeSwitching,
    PartyScreenFromSelection,
    PartyScreenFromFaint,
    MoveToForget,
    BattleOver,
}

public enum CurrentBattleTurn
{
    First,
    Second,
    NoTurn
}

public enum BattleAction
{
    Move,
    Switch,
    Pack,
    Run
}

public enum BattleOverState
{
    Won,
    Lost,
    Ran
}