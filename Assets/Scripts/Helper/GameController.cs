using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;

public class GameController : MonoBehaviour
{
    // fields
    [SerializeField] private GamerController gamerController;
    [SerializeField] private UniteonBattle uniteonBattle;
    [SerializeField] private Camera worldCamera;
    private GameState _gameState;
    
    // Start is called before the first frame update
    void Start()
    {
        gamerController.OnEncountered += InitiateBattle;
        uniteonBattle.OnBattleOver += EndBattle;
    }

    private void InitiateBattle()
    {
        _gameState = GameState.Battle;
        uniteonBattle.gameObject.SetActive(true);
        worldCamera.gameObject.SetActive(false);
        uniteonBattle.InitiateBattle();
    }
    
    private void EndBattle(bool won)
    {
        _gameState = GameState.World;
        uniteonBattle.gameObject.SetActive(false);
        worldCamera.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (_gameState == GameState.World)
        {
            gamerController.ControllerUpdate();
        }
        else if (_gameState == GameState.Battle)
        {
            uniteonBattle.ControllerUpdate();
        }
    }
}

public enum GameState
{
    World,
    Battle
}