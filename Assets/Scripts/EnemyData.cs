using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    public GameObject enemyPrefab;

    public GameObject spawnVFX;
    
    [Header("Enemy Stats")]
    public float health = 3;
    public int XPDrop = 10;

    public float agentSpeed = 3.5f;

    public float patrolSpeed = 2f;

    public float attackCD = 2f;

    public float attackRange = 1.5f;
    public float aggroRange = 4f;

    public float weaponDamage = 1f;

    public bool isRanged = false;

    public GameObject projectilePrefab;
}






