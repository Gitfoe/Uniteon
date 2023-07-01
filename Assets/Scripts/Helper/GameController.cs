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
    private GameState _gameState;
    private MentorController _mentor;
    
    // Singleton Design Pattern
    public static GameController Instance { get; private set; }

    private void Awake() => Instance = this;

    // Start is called before the first frame update
    private void Start()
    {
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
    public void InitiateBattle(MentorController mentor = null)
    {
        _gameState = GameState.Battle;
        uniteonBattle.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        worldUI.gameObject.SetActive(false);
        _mentor = mentor;
        UniteonParty gamerParty = gamer.GetComponent<UniteonParty>();
        if (ReferenceEquals(mentor, null))
        {
            Uniteon wildUniteon = FindObjectOfType<WorldArea>().GetComponent<WorldArea>().GetWildUniteon();
            uniteonBattle.StartBattle(gamerParty, wildUniteon);
            Debug.Log($"Wild battle initiated: {wildUniteon.UniteonBase.UniteonName}");
        }
        else
        {
            UniteonParty mentorParty = mentor.GetComponent<UniteonParty>();
            uniteonBattle.StartBattle(gamerParty, mentorParty);
            Debug.Log($"Mentor battle initiated: {mentor.MentorName}");
        }
    }
    
    private void EndBattle(bool won)
    {
        _gameState = GameState.World;
        if (!ReferenceEquals(_mentor, null) && won)
        {
            _mentor.HandleBattleLost();
        }
        gamer.InitialiseWorld();
        uniteonBattle.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
        worldUI.gameObject.SetActive(true);
        
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
    Cutscene
}