using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class UnitSpawner : MonoBehaviour
{
    public UnityEvent<Unit> OnUnitPreSpawn;
    public UnityEvent<Unit> OnUnitSpawn;

    public float spawnRadius;
    public Vector3 spawnDirection;

    public Unit SpawnUnit(UnitCfg unit)
    {
        Vector2 randSpawnPos = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position;
        spawnPos.x += randSpawnPos.x;
        spawnPos.z += randSpawnPos.y;
        
        Unit newUnit = GameObject.Instantiate(
            unit.unitPrefab,
            spawnPos,
            Quaternion.Euler(transform.forward));
        
        // throw event
        OnUnitPreSpawn.Invoke(newUnit);

        StartCoroutine(DoSpawnRoutine(newUnit));
        return newUnit;
    }

    private IEnumerator DoSpawnRoutine(Unit newUnit)
    {
        OnUnitSpawn.Invoke(newUnit);
        newUnit.gameObject.SetActive(true);
        yield return null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UnitSpawner), true)]
public class UnitSpawnerEditor : Editor
{
    private UnitSpawner _spawner;
    public void OnSceneGUI()
    {
        if (_spawner == null)
        {
            _spawner = (UnitSpawner) target;
            return;
        }

        Handles.DrawWireDisc(_spawner.transform.position, Vector3.up, _spawner.spawnRadius);
        Handles.DrawLine(_spawner.transform.position, _spawner.transform.position + (_spawner.spawnDirection * 3.0f));
    }
}
#endif