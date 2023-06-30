using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

/// <summary>
/// The Game Controller class is responsible for giving controller access to different parts of the code.
/// </summary>
public class GameController : MonoBehaviour
{
    // Fields
    [SerializeField] private GamerController gamerController;
    [SerializeField] private UniteonBattle uniteonBattle;
    [SerializeField] private Camera worldCamera;
    private GameState _gameState;
    
    // Start is called before the first frame update
    void Start()
    {
        gamerController.OnEncountered += InitiateBattle;
        gamerController.OnInMentorsView += (Collider2D mentorCollider) =>
        {
            MentorController mentor = mentorCollider.GetComponentInParent<MentorController>();
            if (!ReferenceEquals(mentor, null))
                StartCoroutine(mentor.TriggerMentorBattle(gamerController));
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

    private void InitiateBattle()
    {
        _gameState = GameState.Battle;
        uniteonBattle.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        UniteonParty gamerParty = gamerController.GetComponent<UniteonParty>();
        Uniteon wildUniteon = FindObjectOfType<WorldArea>().GetComponent<WorldArea>().GetWildUniteon();
        uniteonBattle.StartBattle(gamerParty, wildUniteon);
    }
    
    private void EndBattle(bool won)
    {
        _gameState = GameState.World;
        gamerController.InitialiseWorld();
        uniteonBattle.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        switch (_gameState)
        {
            case GameState.World:
                gamerController.ControllerUpdate();
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
    Dialog
}