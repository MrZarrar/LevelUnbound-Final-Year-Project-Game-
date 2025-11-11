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


    private float attackCD;
    private int XPDrop;
    private float attackRange;
    private float aggroRange;
    private float health;

    public static event Action OnEnemyDied;

    GameObject player;
    NavMeshAgent agent; 
    Animator animator;
    float timePassed;
    float newDestinationCD = 0.5f;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); 
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");

        health = enemyData.health;
        attackCD = enemyData.attackCD;
        attackRange = enemyData.attackRange;
        aggroRange = enemyData.aggroRange;
        agent.speed = enemyData.agentSpeed;

        EnemyDamageDealer[] allDealers = GetComponentsInChildren<EnemyDamageDealer>();

        foreach (EnemyDamageDealer dealer in allDealers)
        {
            dealer.SetDamage(enemyData.weaponDamage);
        }


        healthBar.SetMaxHP((int)health);
    }

    void Update()
    {
        animator.SetFloat("speed", agent.velocity.magnitude / agent.speed);

        if (player == null)
        {
            return;
        }

        HandleMeleeAI();
    }

    private void HandleMeleeAI(){

        if (timePassed >= attackCD)
        {
            if (Vector3.Distance(player.transform.position, transform.position) <= attackRange)
            {
                animator.SetTrigger("attack");
                timePassed = 0;
            }
        }
        timePassed += Time.deltaTime;

        if (newDestinationCD <= 0 && Vector3.Distance(player.transform.position, transform.position) <= aggroRange)
        {
            newDestinationCD = 0.5f;
            agent.SetDestination(player.transform.position);
        }
        newDestinationCD -= Time.deltaTime;
        transform.LookAt(player.transform);
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

        OnEnemyDied?.Invoke();

        GameObject ragdollInstance = Instantiate(ragdoll, transform.position, transform.rotation);

        Destroy(this.gameObject);

        player.GetComponent<PlayerStats>().AddXP(XPDrop);

        Destroy(ragdollInstance, 10f);
        
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        healthBar.SetHP((int)health);
        animator.SetTrigger("damage");
        ShowHealthBar();

        if (health <= 0)
        {
            Die();
        }
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
        Destroy(hit, 3f);
    }  

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    } 
} 