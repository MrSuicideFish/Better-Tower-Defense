using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    private const int MAX_SPAWNED_UNITS = 250;

    private static int _unitUpdateIdx;
    private static UnitManager _manager;

    private Unit[] _spawnedUnits = new Unit[MAX_SPAWNED_UNITS];
    
    public static UnitManager Get()
    {
        if (_manager == null)
        {
            _manager = GameObject.FindObjectOfType<UnitManager>();
        }
        return _manager;
    }
        
    public bool TrySpawnUnit(string unitId)
    {
        UnitCfg cfg = string.IsNullOrEmpty(unitId)
            ? UnitCfgDb.GetRandomCfg()
            : UnitCfgDb.GetUnitCfgById(unitId);

        if (cfg != null)
        {
            return TrySpawnUnit(cfg.unitType, cfg.unitLevel);
        }

        return false;
    }

    public bool TrySpawnUnit(EnemyUnit unit, EnemyLevel level)
    {
        if (GameManager.Instance == null) return false;
        if (GameManager.Instance.spawnPoint == null || GameManager.Instance.nexus == null) return false;
        
        bool unitPopulationFull = true;
        int unitSlot = -1;
        for (int i = 0; i < MAX_SPAWNED_UNITS; i++)
        {
            if (_spawnedUnits[i] == null)
            {
                unitPopulationFull = false;
                unitSlot = i;
                break;
            }
        }

        if (!unitPopulationFull)
        {
            UnitCfg cfg = UnitCfgDb.GetUnitCfg(unit, level);
            if (cfg != null)
            {
                Unit newUnit = GameManager.Instance.spawnPoint.SpawnUnit(cfg);
                _spawnedUnits[unitSlot] = newUnit;
                
                newUnit.SetCfg(cfg);
                newUnit.OnSpawned();
                
                return true;
            }
        }

        Debug.LogWarning("Failed to spawn!");
        return false;
    }
    
    public void KillUnit(Unit unit)
    {
        if (_spawnedUnits == null) return;
        for (int i = 0; i < MAX_SPAWNED_UNITS; i++)
        {
            if (_spawnedUnits[i] == unit)
            {
                unit.OnKilled();
                _spawnedUnits[i] = null;
                break;
            }
        }
    }

    public void ClearAllUnits()
    {
        if (_spawnedUnits == null) return;
        for (int i = 0; i < MAX_SPAWNED_UNITS; i++)
        {
            if (_spawnedUnits[i] != null)
            {
                GameObject.Destroy(_spawnedUnits[i].gameObject);
                _spawnedUnits[i] = null;
            }
        }
    }

    private void Update()
    {
        if (_unitUpdateIdx >= 0 && _unitUpdateIdx < MAX_SPAWNED_UNITS)
        {
            if (_spawnedUnits[_unitUpdateIdx] != null)
            {
                _spawnedUnits[_unitUpdateIdx].OnBoardUnitUpdate();
            }
        }else
        {
            _unitUpdateIdx = 0;
        }

        _unitUpdateIdx++;
    }
}
