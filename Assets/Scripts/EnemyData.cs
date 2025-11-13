using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    public GameObject enemyPrefab;
    
    [Header("Effects")]
    public GameObject spawnVFX;
    
    [Header("Enemy Stats")]
    public float health = 3;
    public int XPDrop = 10;
    
    [Header("Movement")]
    public float agentSpeed = 3.5f; // Chase speed
    public float patrolSpeed = 1.5f;
    public float aggroRange = 20f; // Standard aggro range
    public float stoppingDistance = 2f;


    [Header("Melee Attack")]
    public float meleeAttackRange = 1.5f;
    public float meleeAttackCD = 2f;
    public float meleeWeaponDamage = 10f;

    [Header("Ranged Attack")]
    public bool hasRangedAttack = false; 
    public GameObject projectilePrefab;
    public float rangedAttackRange = 15f; // Max range for ranged attack
    public float rangedAttackCD = 5f;
    public float rangedWeaponDamage = 10f; 
}