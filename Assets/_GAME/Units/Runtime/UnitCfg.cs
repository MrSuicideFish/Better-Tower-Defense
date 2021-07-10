using UnityEngine;

[CreateAssetMenu(menuName = "Units/Create Unit Config")]
public class UnitCfg : ScriptableObject
{
    public EnemyUnit unitType;
    public EnemyLevel unitLevel;
    public Unit unitPrefab;

    [Header("Vitals")]
    public int health;
    public int armor;

    [Header("Locomotion")]
    public float moveSpeed = 2;

    [Header("Combat")]
    public bool attacksMultipleEnemies = false;
    public float attackRange = 2;
}