using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] float health = 3;

    [SerializeField] int XPDrop = 10;
    [SerializeField] GameObject hitVFX;

    [SerializeField] GameObject ragdoll;


    [Header("Combat")]
    [SerializeField] float attackCD = 2f;
    [SerializeField] float attackRange = 1.5f;
    [SerializeField] float aggroRange = 4f;

    GameObject player;
    NavMeshAgent agent; 
    Animator animator;
    float timePassed;
    float newDestinationCD = 0.5f;

    public HealthBar healthBar;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>(); 
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player");

        healthBar.SetMaxHP((int)health);
    }

    void Update()
    {
        animator.SetFloat("speed", agent.velocity.magnitude / agent.speed);

 		if (player == null)
		{
            return;
		} 

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

     public void StartDealDamage()
    {
        GetComponentInChildren<EnemyDamageDealer>().StartDealDamage();
    }
    public void EndDealDamage()
    {
        GetComponentInChildren<EnemyDamageDealer>().EndDealDamage();
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