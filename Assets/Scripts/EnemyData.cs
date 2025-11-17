using UnityEngine;


public enum SpecialAbility { None, Poison, TeleportOnHit }

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    public GameObject enemyPrefab;

    public string deathAnimationName;

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
    public bool hasMeleeAttack = true;
    public float meleeAttackRange = 1.5f;
    public float meleeAttackCD = 2f;
    public float meleeWeaponDamage = 10f;

    [Header("Ranged Attack")]
    public bool hasRangedAttack = false;
    public GameObject projectilePrefab;
    public float rangedAttackRange = 15f; // Max range for ranged attack
    public float rangedAttackCD = 5f;
    public float rangedWeaponDamage = 10f; 
    public float projectileRadius = 0.5f;
    public float projectileSpeed = 10f;
    
    [Header("AI & Abilities")]
    public float fleeDistance = 10f;
    
    public SpecialAbility specialAbility = SpecialAbility.None;

    
    [Header("Poison (for Zombie)")]
    public float poisonDuration = 3f;
    public float poisonSlowAmount = 0.5f; 

    [Header("Teleport (for Vampire)")]
    public float teleportCooldown = 5f;
    public GameObject teleportOutVFX;
    public GameObject teleportInVFX;
}