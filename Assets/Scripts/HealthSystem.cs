using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] float health = 100;

    [SerializeField] GameObject hitVFX;
    [SerializeField] GameObject ragdoll;

    [Header("HUD")]
    [SerializeField] HealthBar playerHealthBar; 

 
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();

        if (playerHealthBar != null)
        {
            playerHealthBar.SetMaxHP((int)health);
        }
        else
        {
            Debug.LogWarning("Player Health Bar UI is not assigned in the Inspector!");
        }
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        animator.SetTrigger("damage");


        if (playerHealthBar != null)
        {
            playerHealthBar.SetHP((int)health);
        }


        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Instantiate(ragdoll, transform.position, transform.rotation);
        Destroy(this.gameObject);
    }
    
    public void HitVFX(Vector3 hitPoint)
    {
        
        GameObject hitInstance = Instantiate(hitVFX, hitPoint, Quaternion.identity);
        Destroy(hitInstance, 1f); 

    }
}
