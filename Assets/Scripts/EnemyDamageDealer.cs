using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class EnemyDamageDealer : MonoBehaviour
{
    private float weaponDamage = 1f;
    private Collider hitboxCollider;
    private List<Collider> collidersAlreadyHit = new List<Collider>();

    private EnemyData enemyData; 

    void Start()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false;
    }

    public void Setup(EnemyData data)
    {
        this.enemyData = data;
        this.weaponDamage = data.meleeWeaponDamage;
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
        if (other.CompareTag("Player") && !collidersAlreadyHit.Contains(other))
        {
            collidersAlreadyHit.Add(other);

            if (other.TryGetComponent(out HealthSystem health))
            {
                Vector3 hitPoint = other.ClosestPoint(transform.position);

                health.TakeDamage(weaponDamage);
                health.HitVFX(hitPoint);
                Debug.Log($"Enemy dealt {weaponDamage} damage to {other.name}");

                if (enemyData != null && enemyData.specialAbility == SpecialAbility.Poison)
                {
                    if (other.TryGetComponent(out Character character))
                    {
                        character.ApplySlowdown(enemyData.poisonDuration, enemyData.poisonSlowAmount);
                    }
                }
            }
        }
    }
}