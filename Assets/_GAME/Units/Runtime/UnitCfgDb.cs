using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Units/Create Unit Database")]
public class UnitCfgDb : ScriptableObject
{
    private static UnitCfgDb _database;
    private static UnitCfgDb database
    {
        get
        {
            if (_database == null)
            {
                _database = Resources.Load<UnitCfgDb>("UnitCfgDb");
            }
            return _database;
        }
    }
    
    public UnitCfg[] units;

    public static UnitCfg GetUnitCfg(EnemyUnit unit, EnemyLevel level)
    {
        foreach (var e in database.units)
        {
            if (e.unitType == unit && e.unitLevel == level)
            {
                return e;
            }
        }

        Debug.LogWarning(
            $"Unit Config Database: Failed to find unit '{unit.ToString()}' of level '{level.ToString()}'");
        return null;
    }

    public static UnitCfg GetUnitCfgById(string id)
    {
        foreach (var unit in database.units)
        {
            if (unit.name == id)
            {
                return unit;
            }
        }
        return null;
    }

    public static UnitCfg GetRandomCfg()
    {
        if (database.units.Length == 0) return null;
        return database.units[Random.Range(0, database.units.Length - 1)];
    }
}