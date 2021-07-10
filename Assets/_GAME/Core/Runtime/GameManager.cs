using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }
    
    public UnitSpawner spawnPoint { get; private set; }
    public Nexus nexus { get; private set; }
    public GameState currentGameState  { get; private set; }
    public int currentWave { get; private set; }
    public bool isPaused { get; private set; }

    public delegate void GameStateDelegate(GameState state);
    public static event GameStateDelegate OnGameStateChanged;

    public string levelName;
    public WaveInfo[] waves;
    public WaveInfo GetCurrentWaveInfo()
    {
        int waveIdx = currentWave - 1;
        if (waveIdx < 0 || waveIdx >= waves.Length) return null;
        return waves[waveIdx];
    }
    
    private IGameState _gameState_current = null;
    private IGameState _gameState_preGame = new GameState_PreGame();
    private IGameState _gameState_preWave = new GameState_PreWave();
    private IGameState _gameState_wave = new GameState_Wave();
    private IGameState _gameState_postWave = new GameState_PostWave();
    private IGameState _gameState_postGame = new GameState_PostGame();

    private void Awake()
    {
        spawnPoint = GameObject.FindObjectOfType<UnitSpawner>();
        nexus = GameObject.FindObjectOfType<Nexus>();

        if (spawnPoint == null || nexus == null)
        {
            Debug.LogError("Failed to start game, missing nexus or spawn point!");
            return;
        }
        
        GoToState(GameState.PreGame, true);
    }

    public void StartGame()
    {
        currentWave = 1;
        GoToState(GameState.PreWave);
    }

    public void PrepareNextWave()
    {
        currentWave++;
        GoToState(GameState.PreWave);
    }

    public void StartWave()
    {
        GoToState(GameState.Wave);
    }

    public void EndWave()
    {
        if (currentWave < waves.Length)
        {
            GoToState(GameState.PostWave);
        }
        else
        {
            EndGame(false);
        }
    }

    public void EndGame(bool forceWin)
    {
        GoToState(GameState.PostGame);
    }
    
    public void PauseGame(bool toggle)
    {
        isPaused = toggle;
        if (isPaused)
        {
            
        }
        else
        {
            
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseGame(!isPaused);
        }

        TickStateMachine();
    }

    private void OnGUI()
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 60;
        
        GUIStyle btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.fontSize = 50;

        GUI.Label(new Rect(0, 0, 600, 100), $"{currentGameState.ToString()} (Wave: {currentWave})", labelStyle);
        switch (currentGameState)
        {
            case GameState.PreGame:
                if (GUI.Button(new Rect(0,110, 300, 100), "Start Game", btnStyle))
                {
                    StartGame();
                }
                break;
            case GameState.PreWave:
                if (GUI.Button(new Rect(0,110, 300, 100), "Start Wave", btnStyle))
                {
                    StartWave();
                }
                break;
            case GameState.Wave:
                if (GUI.Button(new Rect(0,110, 500, 100), "End Wave/Game", btnStyle))
                {
                    EndWave();
                }
                break;
            case GameState.PostWave:
                if (GUI.Button(new Rect(0,110, 300, 100), "Next Wave", btnStyle))
                {
                    PrepareNextWave();
                }
                break;
        }

        if (currentGameState != GameState.PostGame)
        {
            if (GUI.Button(new Rect(0, 220, 300, 100), "End Game", btnStyle))
            {
                EndGame(false);
            }
        } 
    }

    #region Game State Machine
    private float _timeInState = 0f;
    private void GoToState(GameState newState, bool force = false)
    {
        if (currentGameState == newState && !force) return;
        
        IGameState nextState = null;
        IGameState lastState = _gameState_current;
        
        switch (newState)
        {
            case GameState.PreGame:
                nextState = _gameState_preGame;
                break;
            case GameState.PreWave:
                nextState = _gameState_preWave;
                break;
            case GameState.Wave:
                nextState = _gameState_wave;
                break;
            case GameState.PostWave:
                nextState = _gameState_postWave;
                break;
            case GameState.PostGame:
                nextState = _gameState_postGame;
                break;
        }
        
        if (_gameState_current != null)
        {
            _gameState_current.OnStateExit(nextState);
        }

        // apply new state
        _gameState_current = nextState;
        _gameState_current.OnStateEnter(lastState);
        
        currentGameState = newState;
        
        OnGameStateChanged?.Invoke(newState);
        _timeInState = 0f;
    }

    private void TickStateMachine()
    {
        _timeInState += Time.deltaTime;
        if (_gameState_current != null)
        {
            _gameState_current.OnStateUpdate(_timeInState, Time.deltaTime);
        }
    }
    #endregion
}
