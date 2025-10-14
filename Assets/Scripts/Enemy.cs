using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] float health = 3;

    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        
    }
	

    void Die()
    {
      
        Destroy(gameObject);
}

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        animator.SetTrigger("damage");

        if (health <= 0)
        {
            Die();
        }
    }

} 