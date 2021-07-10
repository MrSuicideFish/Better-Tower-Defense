
using System;
using System.Collections.Generic;
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

public struct HitInfo
{
    public int damage;
    public StatusEffect statusEffect;
    public float statusEffectChance;
}

public interface IBoardUnit
{
    int GetHealth();
    int GetArmor();
    StatusEffect GetStatus();
    void OnSpawned();
    void OnKilled();
    void OnHit(HitInfo hitInfo);
    void OnBoardUnitUpdate();
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
public class PartyUnitInfo
{
    [SerializeField] public EnemyUnit unit;
    [SerializeField] public EnemyLevel level;
    [SerializeField] public int count;
}

[Serializable]
public class UnitParty
{
    [SerializeField] public PartyUnitInfo[] units;
    [SerializeField] public float timeBetweenUnitSpawn = 2.0f;
}

[Serializable]
public class WaveInfo
{
    [SerializeField] public float timeBetweenParties = 10.0f;
    [SerializeField] public UnitParty[] parties;
}

public class PartySpawnTracker
{
    public float timeToNextUnitSpawn = 0;
    
    private UnitParty _partyInfo;
    private int _unitCloneIdx;
    private int _unitIdx;

    public PartySpawnTracker(UnitParty partyInfo)
    {
        _unitCloneIdx = 0;
        _unitIdx = 0;
        _partyInfo = partyInfo;
        timeToNextUnitSpawn = 0.0f;
    }

    public UnitParty GetPartyInfo()
    {
        return _partyInfo;
    }

    public bool StepTracker(float deltaTime, out int unitIdxToSpawn)
    {
        if (_unitIdx < _partyInfo.units.Length)
        {
            timeToNextUnitSpawn -= deltaTime;
            if (timeToNextUnitSpawn <= 0.0f)
            {
                unitIdxToSpawn = _unitIdx;
                _unitCloneIdx++;
                
                if (_unitCloneIdx >= _partyInfo.units[_unitIdx].count)
                {
                    _unitCloneIdx = 0;
                    _unitIdx++;
                }

                timeToNextUnitSpawn = _partyInfo.timeBetweenUnitSpawn;
                return true;
            }
        }

        unitIdxToSpawn = -1;
        return false;
    }
}
#endregion

#region GameState

public enum GameWinStatus
{
    NoResult = 0,
    Win = 1,
    Lose = 2
}

public interface IGameState
{
    void OnStateEnter(IGameState lastState);
    void OnStateExit(IGameState nextState);
    void OnStateUpdate(float timeInState, float deltaTime);
}
#endregion
