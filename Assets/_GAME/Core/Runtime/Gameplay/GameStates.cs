using UnityEngine;

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
    }

    public void OnStateExit(IGameState nextState)
    {
        _levelInfoScreen.gameObject.SetActive(false);
    }

    public void OnStateUpdate()
    {
        if (_levelInfoScreen.isComplete)
        {
            GameManager.Instance.StartGame();
        }
    }
}

public class GameState_PreWave : IGameState
{
    public void OnStateEnter(IGameState lastState)
    {
        // begin build mode
        ConstructionManager.BeginConstructionMode();
    }

    public void OnStateExit(IGameState nextState)
    {
    }

    public void OnStateUpdate()
    {
    }
}

public class GameState_Wave : IGameState
{
    public void OnStateEnter(IGameState lastState)
    {
        ConstructionManager.EndConstructionMode();
    }

    public void OnStateExit(IGameState nextState)
    {
    }

    public void OnStateUpdate()
    {
    }
}

public class GameState_PostWave : IGameState
{
    public void OnStateEnter(IGameState lastState)
    {
    }

    public void OnStateExit(IGameState nextState)
    {
    }

    public void OnStateUpdate()
    {
    }
}

public class GameState_PostGame : IGameState
{
    public void OnStateEnter(IGameState lastState)
    {
    }

    public void OnStateExit(IGameState nextState)
    {
    }

    public void OnStateUpdate()
    {
    }
}