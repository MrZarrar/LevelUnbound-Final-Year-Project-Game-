using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public class Enemy : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    [SerializeField] GameObject hitVFX;

    [SerializeField] GameObject ragdoll;

    [SerializeField] HealthBar healthBar;

    [SerializeField] private EnemyDamageDealer leftHandDealer;
    [SerializeField] private EnemyDamageDealer rightHandDealer;
    [SerializeField] private Transform projectileSpawnPoint;

    private enum EnemyState { Patrolling, Chasing }
    private EnemyState currentState;

    [SerializeField] private float patrolRadius = 20f;


    private int XPDrop;
    private float meleeAttackCD, meleeAttackRange, meleeWeaponDamage;
    private float rangedAttackCD, rangedAttackRange, rangedWeaponDamage;
    private bool hasRangedAttack;

    private float meleeTimePassed;
    private float rangedTimePassed;
    private int playerLayer;
    private Collider ownCollider;
    private float aggroRange;
    private float health;

    public static event Action OnEnemyDied;

    GameObject player;
    NavMeshAgent agent;
    Animator animator;
    float timePassed;

    private bool isDying = false;

    private bool hasMeleeAttack;
    
    private bool teleportOnCooldown = false;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");

        playerLayer = LayerMask.NameToLayer("Player");
        ownCollider = GetComponent<Collider>();

        if (enemyData.spawnVFX != null)
        {
            Instantiate(enemyData.spawnVFX, transform.position, transform.rotation);
        }

        health = enemyData.health;

        hasMeleeAttack = enemyData.hasMeleeAttack;
        meleeAttackCD = enemyData.meleeAttackCD;
        meleeAttackRange = enemyData.meleeAttackRange;
        meleeWeaponDamage = enemyData.meleeWeaponDamage;


        hasRangedAttack = enemyData.hasRangedAttack;
        rangedAttackCD = enemyData.rangedAttackCD;
        rangedAttackRange = enemyData.rangedAttackRange;
        rangedWeaponDamage = enemyData.rangedWeaponDamage;


        aggroRange = enemyData.aggroRange;

        XPDrop = enemyData.XPDrop;

        EnemyDamageDealer[] allDealers = GetComponentsInChildren<EnemyDamageDealer>();
        foreach (EnemyDamageDealer dealer in allDealers)
        {
            dealer.Setup(enemyData); // Pass all data
        }


        healthBar.SetMaxHP((int)health);

        currentState = EnemyState.Patrolling;
        agent.speed = enemyData.patrolSpeed;
        SetNewPatrolPoint(); // Get the first random point to walk to

    }

    void Update()
    {

        if (isDying) return;

        animator.SetFloat("speed", agent.velocity.magnitude / agent.speed);
        if (player == null) return;
        animator.SetFloat("speed", agent.velocity.magnitude / agent.speed);
        if (player == null) return;

        meleeTimePassed += Time.deltaTime;
        rangedTimePassed += Time.deltaTime;

        float distanceToPlayer = Vector3.Distance(player.transform.position, transform.position);

        if (distanceToPlayer <= aggroRange)
        {
            currentState = EnemyState.Chasing;
        }
        else
        {
            currentState = EnemyState.Patrolling;
        }

        if (currentState == EnemyState.Chasing)
        {
            agent.speed = enemyData.agentSpeed;
            agent.stoppingDistance = enemyData.stoppingDistance;
            HandleChasingAI(distanceToPlayer);
        }
        else
        {
            agent.speed = enemyData.patrolSpeed;
            agent.stoppingDistance = 2f;
            HandlePatrollingAI();
        }
    }

    private void HandlePatrollingAI()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            SetNewPatrolPoint();
        }
    }

    private void SetNewPatrolPoint()
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
        {
            agent.SetDestination(hit.position);
        }
    }


    private void HandleChasingAI(float distanceToPlayer)
    {
        transform.LookAt(player.transform);

        // 1. MELEE LOGIC
        if (hasMeleeAttack && distanceToPlayer <= meleeAttackRange)
        {
            agent.stoppingDistance = 2;
            agent.SetDestination(player.transform.position); 
            
            if (meleeTimePassed >= meleeAttackCD)
            {
                animator.SetTrigger("meleeAttack"); 
                meleeTimePassed = 0;
            }
        }
        // 2. RANGED LOGIC
        else if (hasRangedAttack && distanceToPlayer <= rangedAttackRange)
        {
            // If no melee, run
            if (!hasMeleeAttack && distanceToPlayer < enemyData.fleeDistance)
            {
                // Flee: Get direction away from player
                Vector3 fleeDirection = (transform.position - player.transform.position).normalized;
                Vector3 fleeTarget = transform.position + fleeDirection * enemyData.fleeDistance;
                agent.SetDestination(fleeTarget);
            }
            // If we are in range (and not fleeing), stop and shoot
            else
            {
                agent.stoppingDistance = enemyData.stoppingDistance;
                agent.SetDestination(transform.position); // Stop moving
            }

            // Fire projectile
            if (rangedTimePassed >= rangedAttackCD)
            {
                animator.SetTrigger("rangedAttack"); 
                rangedTimePassed = 0;
            }
        }
        // 3. CHASE LOGIC (Too far to attack)
        else
        {
            // If we are a mage, our "chase" is to get to rangedAttackRange
            if (!hasMeleeAttack && hasRangedAttack)
            {
                agent.stoppingDistance = enemyData.stoppingDistance;
            }
            else // We are melee, get in close
            {
                agent.stoppingDistance = 2;
            }
            agent.SetDestination(player.transform.position);
        }
    }


    public void SpawnProjectile()
    {

        if (isDying) return;

        if (enemyData.projectilePrefab == null) return;

        Vector3 aimTarget = player.transform.position + Vector3.up * 1f;
        Vector3 direction = (aimTarget - projectileSpawnPoint.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        GameObject blast = Instantiate(
            enemyData.projectilePrefab,
            projectileSpawnPoint.position,
            lookRotation
        );

        float damage = rangedWeaponDamage;

        Projectiles projectileScript = blast.GetComponent<Projectiles>();
        if (projectileScript != null)
        {
            projectileScript.Setup(damage, playerLayer, ownCollider);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            print(true);
            player = collision.gameObject;
        }
    }

    void Die()
    {
        if (isDying) return;
        isDying = true;

        agent.enabled = false;
        if (GetComponent<Collider>() != null)
        {
            GetComponent<Collider>().enabled = false;
        }

        EnemyDamageDealer[] allDealers = GetComponentsInChildren<EnemyDamageDealer>();
        foreach (EnemyDamageDealer dealer in allDealers)
        {
            dealer.enabled = false;
        }

        StartCoroutine(DeathRoutine());
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDying) return;

        health -= damageAmount;
        healthBar.SetHP((int)health);
        animator.SetTrigger("damage");
        ShowHealthBar();

        aggroRange = enemyData.aggroRange * 2f;

        if (enemyData.specialAbility == SpecialAbility.TeleportOnHit && !teleportOnCooldown)
        {
            StartCoroutine(TeleportRoutine());
        }

        if (health <= 0)
        {
            Die();
        }
    }

    private IEnumerator TeleportRoutine()
    {
        teleportOnCooldown = true;
        
        if (enemyData.teleportOutVFX != null)
        {
            Instantiate(enemyData.teleportOutVFX, transform.position, transform.rotation);
        }
        
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 newPosition = transform.position; 
        
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, 1))
        {
            newPosition = hit.position;
        }

        agent.Warp(newPosition);
        
        if (enemyData.teleportInVFX != null)
        {
            Instantiate(enemyData.teleportInVFX, newPosition, Quaternion.identity);
        }
        
        yield return new WaitForSeconds(enemyData.teleportCooldown);
        teleportOnCooldown = false;
    }

    private void ShowHealthBar()
    {
        healthBar.gameObject.SetActive(true);
    }

    public void StartLeftHandDamage()
    {
        if (leftHandDealer != null)
            leftHandDealer.StartDealDamage();
    }

    public void EndLeftHandDamage()
    {
        if (leftHandDealer != null)
            leftHandDealer.EndDealDamage();
    }

    public void StartRightHandDamage()
    {
        if (rightHandDealer != null)
            rightHandDealer.StartDealDamage();
    }

    public void EndRightHandDamage()
    {
        if (rightHandDealer != null)
            rightHandDealer.EndDealDamage();
    }

    public void HitVFX(Vector3 hitPosition)
    {
        GameObject hit = Instantiate(hitVFX, hitPosition, Quaternion.identity);
    }

    private void OnDrawGizmos()
    {
        if (enemyData == null)
        {
            return;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, enemyData.meleeAttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, enemyData.aggroRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, enemyData.stoppingDistance);

        if (enemyData.hasRangedAttack)
        {
            Gizmos.color = Color.purple;
            Gizmos.DrawWireSphere(transform.position, enemyData.rangedAttackRange);
        }
    }


    private IEnumerator DeathRoutine()
    {
        float animationLength = GetAnimationClipLength(); 
        
        if (animationLength > 0)
        {
            animator.SetTrigger("death"); 
            yield return new WaitForSeconds(animationLength);
        }

        
        OnEnemyDied?.Invoke();

        GameObject ragdollInstance = Instantiate(ragdoll, transform.position, transform.rotation);
        Destroy(ragdollInstance, 10f);

        if (player != null)
        {
            player.GetComponent<PlayerStats>().AddXP(XPDrop);
        }

        Destroy(this.gameObject);
    }

    private float GetAnimationClipLength()
    {
        if (string.IsNullOrEmpty(enemyData.deathAnimationName))
        {
            return 0.0f; // No name = 0 second wait
        }

        AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
        foreach (AnimationClip clip in clips)
        {
            if (clip.name == enemyData.deathAnimationName)
            {
                return clip.length; 
            }
        }
        
        Debug.LogWarning("Enemy '" + gameObject.name + "' specified death animation '" + enemyData.deathAnimationName + "' but it was not found in the Animator.");
        return 0.0f; // Return 0 so the game doesn't stall
    }

} 

