using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : MonoBehaviour
{
    private const int MAX_SPAWNED_UNITS = 250;
        
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TrySpawnUnit(EnemyUnit.Spiderling, EnemyLevel.Level_1);
        }
    }

    public bool TrySpawnUnit(EnemyUnit unit, EnemyLevel level)
    {
        if (GameManager.Instance == null) return false;
        if (GameManager.Instance.spawnPoint == null || GameManager.Instance.nexus == null) return false;
        
        bool unitPopulationFull = true;
        int unitSlot = -1;
        for (int i = 0; i < _spawnedUnits.Length; i++)
        {
            if (_spawnedUnits[i] == null)
            {
                unitPopulationFull = false;
                unitSlot = i;
            }
        }

        if (!unitPopulationFull)
        {
            UnitCfg cfg = UnitCfgDb.GetUnitCfg(unit, level);
            if (cfg != null)
            {
                Unit newUnit = GameManager.Instance.spawnPoint.SpawnUnit(cfg);
                _spawnedUnits[unitSlot] = newUnit;
                return true;
            }
        }
        return false;
    }
    
    public bool TrySpawnUnit(string unitId)
    {
        if (GameManager.Instance == null) return false;
        if (GameManager.Instance.spawnPoint == null || GameManager.Instance.nexus == null) return false;

        bool unitPopulationFull = true;
        int unitSlot = -1;
        for (int i = 0; i < _spawnedUnits.Length; i++)
        {
            if (_spawnedUnits[i] == null)
            {
                unitPopulationFull = false;
                unitSlot = i;
            }
        }

        if (!unitPopulationFull)
        {
            UnitCfg cfg = string.IsNullOrEmpty(unitId)
                ? UnitCfgDb.GetRandomCfg()
                : UnitCfgDb.GetUnitCfgById(unitId);

            if (cfg != null)
            {
                Unit newUnit = GameManager.Instance.spawnPoint.SpawnUnit(cfg);
                _spawnedUnits[unitSlot] = newUnit;
                return true;
            }
        }

        return false;
    }
}
