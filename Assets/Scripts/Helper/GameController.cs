using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// The Game Controller class is responsible for giving controller access to different parts of the code.
/// </summary>
public class GameController : MonoBehaviour
{
    // Fields
    [SerializeField] private GamerController gamer;
    [SerializeField] private UniteonBattle uniteonBattle;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Canvas worldUI;
    [SerializeField] private Dialog lostDialog;
    private GameState _gameState;
    private MentorController _mentor;
    private OverworldUniteonController _overworldUniteon;
    private GameState _previousGameState;
    private Transition _transition;
    
    // Singleton Design Pattern
    public static GameController Instance { get; private set; }
    
    private void Awake() => Instance = this;

    // Start is called before the first frame update
    private void Start()
    {
        // Play music, currently hard-coded
        const string worldMusic = "eternaLoop";
        AudioManager.Instance.SetPlayingWorldMusic(worldMusic);
        AudioManager.Instance.PlayMusic(worldMusic);
        // Initialise variables and events
        _transition = FindObjectOfType<Transition>();
        gamer.OnEncountered += InitiateBattle;
        gamer.OnInMentorsView += (Collider2D mentorCollider) =>
        {
            MentorController mentor = mentorCollider.GetComponentInParent<MentorController>();
            Debug.Log($"In mentor's view: {mentor.MentorName}");
            // Subscribe to events
            mentor.OnInitiateMentorBattle += gamer.TransitionIntoMentorBattle;
            gamer.OnTransitionDone += mentor.InitiateMentorBattle;
            // Start mentor battle
            if (!mentor.BattleLost)
            {
                _gameState = GameState.Cutscene;
                StartCoroutine(mentor.TriggerMentorBattle(gamer));
            }
        };
        gamer.OnInUniteonsView += (Collider2D overworldUniteonCollider) =>
        {
            OverworldUniteonController overworldUniteon =
                overworldUniteonCollider.GetComponentInParent<OverworldUniteonController>();
            Debug.Log($"In overworld Uniteon's view: {overworldUniteon.UniteonName}");
            // Subscribe to events
            overworldUniteon.OnInitiateOverworldUniteonBattle += gamer.TransitionIntoOverworldUniteonBattle;
            gamer.OnTransitionDone += overworldUniteon.InitiateOverworldUniteonBattle;
            // Start mentor battle
            if (!overworldUniteon.BattleLost)
            {
                _gameState = GameState.Cutscene;
                StartCoroutine(overworldUniteon.TriggerOverworldUniteonBattle(gamer));
            }
        };
        uniteonBattle.OnBattleOver += EndBattle;
        DialogManager.Instance.OnShowDialog += () =>
        {
            _gameState = GameState.Dialog;
        };
        DialogManager.Instance.OnCloseDialog += () =>
        {
            if (_gameState == GameState.Dialog)
                _gameState = GameState.World;
        };
    }

    /// <summary>
    /// Starts either a wild battle or a mentor battle.
    /// </summary>
    /// <param name="mentor">If mentor is entered, it will be a trainer battle instead of a wild battle.</param>
    /// <param name="overworldUniteon">Will enter overworld Uniteon battle.</param>
    public void InitiateBattle(MentorController mentor = null, OverworldUniteonController overworldUniteon = null)
    {
        _gameState = GameState.Battle;
        uniteonBattle.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        worldUI.gameObject.SetActive(false);
        _mentor = mentor;
        _overworldUniteon = overworldUniteon;
        UniteonParty gamerParty = gamer.GetComponent<UniteonParty>();
        if (ReferenceEquals(mentor, null) && ReferenceEquals(overworldUniteon, null))
        {
            Uniteon wildUniteon = FindObjectOfType<WorldArea>().GetComponent<WorldArea>().GetWildUniteon();
            uniteonBattle.StartBattle(gamerParty, wildUniteon);
            Debug.Log($"Wild battle initiated: {wildUniteon.UniteonBase.UniteonName}");
        }
        else if (!ReferenceEquals(mentor, null) && ReferenceEquals(overworldUniteon, null))
        {
            UniteonParty mentorParty = mentor.GetComponent<UniteonParty>();
            uniteonBattle.StartBattle(gamerParty, mentorParty);
            Debug.Log($"Mentor battle initiated: {mentor.MentorName}");
        }
        else
        {
            UniteonParty wildUniteon = overworldUniteon.GetComponent<UniteonParty>();
            uniteonBattle.StartBattle(gamerParty, wildUniteon.Uniteons.FirstOrDefault(), true);
            Debug.Log($"Mentor battle initiated: {overworldUniteon.UniteonName}");
        }
    }
    
    /// <summary>
    /// Gets called once the battle is over.
    /// </summary>
    /// <param name="won">If the gamer won or not.</param>
    private void EndBattle(bool won)
    {
        _gameState = GameState.World;
        uniteonBattle.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        worldUI.gameObject.SetActive(true);
        if (won)
        {
            if (!ReferenceEquals(_mentor, null))
                _mentor.HandleBattleLost();
            if (!ReferenceEquals(_overworldUniteon, null))
                _overworldUniteon.HandleBattleLost();
            StartCoroutine(_transition.FadeOut(0.72f, Color.black));
            AudioManager.Instance.PlayMusic(AudioManager.PlayingWorldMusic);
        }
        else
        {
            _transition.SetTransitionColor(Color.black);
            StartCoroutine(DialogManager.Instance.PrintDialog(lostDialog,
                () =>
                {
                    StartCoroutine(HealUniteonsTransition("potion", false,
                        () => { AudioManager.Instance.PlayMusic(AudioManager.PlayingWorldMusic); }));
                }));
        }
    }

    /// <summary>
    /// Plays the fading, sfx and music sequence for healing all party Uniteons. Doesn't actually heal the Uniteons.
    /// </summary>
    /// <returns>Coroutine.</returns>
    public IEnumerator HealUniteonsTransition(string sfxName, bool playFadeIn = true, Action onComplete = null)
    {
        float sfxLength = AudioManager.Sfx[sfxName].length;
        float fadeTime = 0.4f;
        StartCoroutine(AudioManager.Instance.FadeMuteMusicVolume(fadeTime, sfxLength));
        if (playFadeIn)
            yield return _transition.FadeIn(fadeTime, Color.black);
        AudioManager.Instance.PlaySfx(sfxName);
        yield return new WaitForSeconds(sfxLength);
        onComplete?.Invoke();
        yield return _transition.FadeOut(fadeTime, Color.black);
    }

    /// <summary>
    /// Sets the gamestate to paused, or back to its previous state.
    /// </summary>
    /// <param name="pause">Whether to pause or continue.</param>
    public void PauseGame(bool pause)
    {
        if (pause)
        {
            _previousGameState = _gameState;
            _gameState = GameState.Paused;
        }
        else
            _gameState = _previousGameState;
    } 

    // Update is called once per frame
    void Update()
    {
        switch (_gameState)
        {
            case GameState.World:
                gamer.ControllerUpdate();
                break;
            case GameState.Battle:
                uniteonBattle.ControllerUpdate();
                break;
            case GameState.Dialog:
                DialogManager.Instance.ControllerUpdate();
                break;
        }
    }
}

public enum GameState
{
    World,
    Battle,
    Dialog,
    Cutscene,
    Paused
}