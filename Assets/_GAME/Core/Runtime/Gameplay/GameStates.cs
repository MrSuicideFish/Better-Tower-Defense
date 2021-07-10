using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameState_PreGame : IGameState
{
    private LevelInfoScreen _levelInfoScreen;
    
    public void OnStateEnter(IGameState lastState)
    {
        // do level intro
        if (_levelInfoScreen == null)
        {
            var infoScrnRes = Resources.Load<LevelInfoScreen>("LevelInfoScreen");
            _levelInfoScreen = GameObject.Instantiate(infoScrnRes);
            _levelInfoScreen.gameObject.SetActive(true);
        }
        
        ConstructionManager.EndConstructionMode();
    }

    public void OnStateExit(IGameState nextState)
    {
        _levelInfoScreen.gameObject.SetActive(false);
    }

    public void OnStateUpdate(float timeInState, float deltaTime)
    {
        if (_levelInfoScreen.isComplete)
        {
            GameManager.Instance.StartGame();
        }
    }
}

public class GameState_PreWave : IGameState
{
    private const float _timeForPreWave = 30;

    public void OnStateEnter(IGameState lastState)
    {
        // begin build mode
        ConstructionManager.BeginConstructionMode();
    }

    public void OnStateExit(IGameState nextState)
    {
        ConstructionManager.EndConstructionMode();
    }

    public void OnStateUpdate(float timeInState, float deltaTime)
    {
        //Debug.Log($"Wave starting in: {_timeForPreWave - timeInState}");
        if (timeInState >= _timeForPreWave)
        {
            GameManager.Instance.StartWave();
        }
    }
}

public class GameState_Wave : IGameState
{
    private List<PartySpawnTracker> _spawnQueue;
    private WaveInfo _waveInfo = null;
    private float partySpawnTimer = 0;

    public void OnStateEnter(IGameState lastState)
    {
        _waveInfo = GameManager.Instance.GetCurrentWaveInfo();
        partySpawnTimer = 0.0f;
        _spawnQueue = new List<PartySpawnTracker>();
    }

    public void OnStateExit(IGameState nextState)
    {
        _spawnQueue?.Clear();
        UnitManager.Get().ClearAllUnits();
    }

    public void OnStateUpdate(float timeInState, float deltaTime)
    {
        partySpawnTimer -= deltaTime;
        if (partySpawnTimer <= 0.0f)
        {
            if (_spawnQueue.Count < _waveInfo.parties.Length)
            {
                UnitParty nextParty = _waveInfo.parties[_spawnQueue.Count];
                if (nextParty.units.Length > 0)
                {
                    _spawnQueue.Add(new PartySpawnTracker(nextParty));
                }
            }

            partySpawnTimer = _waveInfo.timeBetweenParties;
        }

        for (int i = 0; i < _spawnQueue.Count; i++)
        {
            PartySpawnTracker tracker = _spawnQueue[i];
            if (tracker != null)
            {
                int unitIdx = -1;
                if (tracker.StepTracker(deltaTime, out unitIdx))
                {
                    UnitParty party = tracker.GetPartyInfo();
                    if (party != null)
                    {
                        PartyUnitInfo partyUnit = party.units[unitIdx];
                        if (partyUnit != null)
                        {
                            if (UnitManager.Get().TrySpawnUnit(partyUnit.unit, partyUnit.level))
                            {
                                
                            }
                            else
                            {
                                Debug.LogError("Failed to spawn unit!");
                            }
                        }
                    }
                }    
            }
        }
    }
}

public class GameState_PostWave : IGameState
{
    private const float timeForPostWave = 10;
    
    public void OnStateEnter(IGameState lastState)
    {
    }

    public void OnStateExit(IGameState nextState)
    {
    }

    public void OnStateUpdate(float timeInState, float deltaTime)
    {
        if (timeInState >= timeForPostWave)
        {
            GameManager.Instance.PrepareNextWave();
        }
    }
}

public class GameState_PostGame : IGameState
{
    public void OnStateEnter(IGameState lastState)
    {
        if (GameManager.Instance.winStatus == GameWinStatus.Win)
        {
            // do game win
        }
        else
        {
            // do game loss
        }
    }

    public void OnStateExit(IGameState nextState)
    {
    }

    public void OnStateUpdate(float timeInState, float deltaTime)
    {
    }
}