using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class EnemyDamageDealer : MonoBehaviour
{
    private float weaponDamage = 1f;
    private Collider hitboxCollider;

  
    private List<Collider> collidersAlreadyHit = new List<Collider>();

    void Start()
    {
        // Get the collider on this object
        hitboxCollider = GetComponent<Collider>();
        
        hitboxCollider.isTrigger = true;
        
        hitboxCollider.enabled = false;
    }

    public void StartDealDamage()
    {
        collidersAlreadyHit.Clear();
        
        hitboxCollider.enabled = true;
    }

    public void EndDealDamage()
    {
      
        hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Check if alrdy hit player
            if (collidersAlreadyHit.Contains(other))
            {
                return; 
            }

            collidersAlreadyHit.Add(other);

            if (other.TryGetComponent(out HealthSystem health))
            {
                // Find the closest point on the player's collider for the VFX
                Vector3 hitPoint = other.ClosestPoint(transform.position);

                health.TakeDamage(weaponDamage);
                health.HitVFX(hitPoint);
                Debug.Log($"Enemy dealt {weaponDamage} damage to {other.name}");
            }
        }
    }

    public void SetDamage(float damage)
    {
        weaponDamage = damage;
    }
}