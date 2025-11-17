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
    [SerializeField] private LayerMask lineOfSightMask;

    private enum EnemyState { Patrolling, Chasing }
    private EnemyState currentState;

    [SerializeField] private float patrolRadius = 20f;

    private int XPDrop;
    private float meleeAttackCD, meleeAttackRange, meleeWeaponDamage;
    private float rangedAttackCD, rangedAttackRange, rangedWeaponDamage, projectileSpeed;
    private bool hasRangedAttack;
    private float meleeTimePassed;
    private float rangedTimePassed;
    private int playerLayer;
    private Collider[] allColliders;
    private float aggroRange;
    private float health;

    public static event Action OnEnemyDied;

    GameObject player;
    NavMeshAgent agent;
    Animator animator;

    private bool isDying = false;
    private bool hasMeleeAttack;
    private bool teleportOnCooldown = false;

    private bool isRepositioning = false;
    private Vector3 lastSeenPosition;
    private bool hasLineOfSight = false;

    private Vector3 debug_LoS_Start;
    private Vector3 debug_LoS_Direction;
    private float debug_LoS_Distance;
    private float debug_LoS_Radius;
    private bool debug_LoS_Active = false;

    private float strafeTimer = 0f;
    private float strafeChangeInterval = 2f;
    private int strafeDirection = 1;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");

        playerLayer = LayerMask.NameToLayer("Player");
        allColliders = GetComponentsInChildren<Collider>();

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
        projectileSpeed = enemyData.projectileSpeed;


        aggroRange = enemyData.aggroRange;

        XPDrop = enemyData.XPDrop;

        EnemyDamageDealer[] allDealers = GetComponentsInChildren<EnemyDamageDealer>();
        foreach (EnemyDamageDealer dealer in allDealers)
        {
            dealer.Setup(enemyData);
        }


        healthBar.SetMaxHP((int)health);

        currentState = EnemyState.Patrolling;
        agent.speed = enemyData.patrolSpeed;
        SetNewPatrolPoint(); 

    }

    void Update()
    {
        if (isDying) return;

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
            isRepositioning = false;
        }

        if (currentState == EnemyState.Chasing)
        {
            hasLineOfSight = CheckLineOfSight();

            if (hasLineOfSight)
            {
                lastSeenPosition = player.transform.position;

            }
            agent.speed = enemyData.agentSpeed;
            HandleChasingAI(distanceToPlayer);
        }
        else
        {
            agent.isStopped = false; // Tell agent to GO
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

        // 1. REPOSITIONING 
        if (isRepositioning)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                isRepositioning = false;
            }
            return; 
        }

        // 2. MELEE ATTACK
        if (hasMeleeAttack && distanceToPlayer <= meleeAttackRange)
        {
            agent.isStopped = false;
            agent.stoppingDistance = 0.1f; 
            agent.SetDestination(player.transform.position); 
            
            if (meleeTimePassed >= meleeAttackCD)
            {
                animator.SetTrigger("meleeAttack"); 
                meleeTimePassed = 0;
            }
        }
        // 3. RANGED LOGIC 
        else if (hasRangedAttack)
        {
            // A) FLEE (If no melee and player is too close)
            if (!hasMeleeAttack && distanceToPlayer < enemyData.fleeDistance)
            {
                agent.isStopped = false;
                agent.stoppingDistance = 0.1f; 
                Vector3 fleeDirection = (transform.position - player.transform.position).normalized;
                Vector3 fleeTarget = transform.position + fleeDirection * enemyData.fleeDistance;
                agent.SetDestination(fleeTarget);
            }
            // B) IN RANGE
            else if (distanceToPlayer <= rangedAttackRange)
            {
                if (hasLineOfSight)
                {
                    // Decide if we should shoot or move
                    
                    if (rangedTimePassed >= rangedAttackCD)
                    {
                        // COOLDOWN IS READY: STOP AND SHOOT 
                        agent.isStopped = true; 
                        agent.stoppingDistance = enemyData.stoppingDistance;

                        animator.SetTrigger("rangedAttack");
                        rangedTimePassed = 0;
                    }
                    else
                    {
                        // COOLDOWN IS ACTIVE: MAINTAIN DISTANCE 

                        MaintainDistance(distanceToPlayer, enemyData.stoppingDistance);
                    }
                }
                else
                {
                    StartRepositioning();
                }
            }
            // C) CHASE (Too far to attack)
            else
            {
                agent.isStopped = false; 
                agent.stoppingDistance = enemyData.stoppingDistance; 
                agent.SetDestination(player.transform.position);
            }
        }
        // 4. MELEE-ONLY CHASE 
        else if (hasMeleeAttack)
        {
            agent.isStopped = false; 
            agent.stoppingDistance = 0.1f;
            agent.SetDestination(player.transform.position);
        }
    }

    private void MaintainDistance(float distanceToPlayer, float preferredDistance)
    {
        Debug.LogWarning("Maintaining distance from player...");   
        Vector3 retreatDirection = (transform.position - player.transform.position).normalized;
        Vector3 retreatTarget = transform.position + retreatDirection * (preferredDistance - distanceToPlayer);
        NavMeshHit hit;
        if (NavMesh.SamplePosition(retreatTarget, out hit, 2f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.stoppingDistance = enemyData.stoppingDistance;
            agent.SetDestination(hit.position);
        }

    }
    
    private void StartRepositioning()
    {
        Debug.LogWarning("Repositioning to find line of sight...");
        isRepositioning = true;
        agent.isStopped = false;
        agent.stoppingDistance = 2;

        Vector3 strafeDirection = (UnityEngine.Random.value > 0.5f) ? transform.right : -transform.right;
        Vector3 randomPoint = transform.position + (strafeDirection * UnityEngine.Random.Range(5f, 10f));

        NavMeshHit fallbackHit;
        if (NavMesh.SamplePosition(randomPoint, out fallbackHit, 10f, NavMesh.AllAreas))
        {
            agent.SetDestination(fallbackHit.position);
        }
        else if (lastSeenPosition != Vector3.zero)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(lastSeenPosition, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                return;
            }
        }
        else
        {
            agent.SetDestination(player.transform.position);
        }
    }

    private bool CheckLineOfSight()
    {
        if (player == null || projectileSpawnPoint == null || enemyData == null)
        {
            return false;
        }

        Vector3 startPoint = projectileSpawnPoint.position;
        Vector3 targetPoint = player.transform.position + Vector3.up * 1f;
        Vector3 direction = (targetPoint - startPoint).normalized;
        float distance = Vector3.Distance(startPoint, targetPoint);
        float projectileRadius = enemyData.projectileRadius;
        

        DebugDrawLineOfSight(startPoint, direction, distance, projectileRadius);


        if (Physics.SphereCast(startPoint, projectileRadius, direction, out RaycastHit hit, distance, lineOfSightMask))
        {
            Debug.DrawLine(startPoint, hit.point, Color.red, 0.1f);
            return false;
        }

        return true;
    }

    private void DebugDrawLineOfSight(Vector3 start, Vector3 direction, float distance, float radius)
    {
        debug_LoS_Start = start;
        debug_LoS_Direction = direction;
        debug_LoS_Distance = distance;
        debug_LoS_Radius = radius;
        debug_LoS_Active = true;
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
        float speed = projectileSpeed;

        Projectiles projectileScript = blast.GetComponent<Projectiles>();
        if (projectileScript != null)
        {
            projectileScript.Setup(damage, playerLayer, allColliders, speed);
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

        if (debug_LoS_Active)
        {
            Gizmos.color = Color.cyan;
            Vector3 currentDrawPoint = debug_LoS_Start;
            float step = 1.5f;
            for (float d = 0; d < debug_LoS_Distance; d += step)
            {
                currentDrawPoint = debug_LoS_Start + debug_LoS_Direction * d;
                Gizmos.DrawWireSphere(currentDrawPoint, debug_LoS_Radius);
            }
            Gizmos.DrawWireSphere(debug_LoS_Start + debug_LoS_Direction * debug_LoS_Distance, debug_LoS_Radius);
            Gizmos.DrawLine(debug_LoS_Start, debug_LoS_Start + debug_LoS_Direction * debug_LoS_Distance);

            debug_LoS_Active = false; 
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
            return 0.0f; 
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
        return 0.0f; 
    }
}



