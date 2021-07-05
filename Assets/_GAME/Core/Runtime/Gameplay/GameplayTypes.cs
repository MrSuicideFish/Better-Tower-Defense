
using System;
using UnityEngine;

public enum GameState
{
    PreGame,
    PreWave,
    Wave,
    PostWave,
    PostGame
}

public enum StatusEffect
{
    None,
    Burned,
    Poisoned,
    Paralyzed
}

public interface IBoardUnit
{
    int GetHealth();
    int GetArmor();
    StatusEffect GetStatus();
    void OnSpawned();
    void OnKilled();
}

#region Enemies
public enum EnemyUnit
{
    Dragon,
    Giant_Bee,
    Golem,
    King_Kobra,
    Magma,
    One_Eyed_Bat,
    Spiderling,
    Treant,
    Wolf
}

public enum EnemyLevel
{
    Level_1,
    Level_2,
    Level_3
}

[Serializable]
public class UnitParty
{
    [SerializeField] public EnemyUnit unit;
    [SerializeField] public EnemyLevel level;
    [SerializeField] public int count;
    [SerializeField] public float timeBetweenUnitSpawn = 2.0f;
}

[Serializable]
public class WaveInfo
{
    [SerializeField] public float timeBetweenParties = 10.0f;
    [SerializeField] public UnitParty[] parties;
}
#endregion

#region GameState
public interface IGameState
{
    void OnStateEnter(IGameState lastState);
    void OnStateExit(IGameState nextState);
    void OnStateUpdate();
}
#endregion
