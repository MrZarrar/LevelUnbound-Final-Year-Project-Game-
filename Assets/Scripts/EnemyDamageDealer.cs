using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamageDealer : MonoBehaviour
{
    bool canDealDamage;
    bool hasDealtDamage;

    [SerializeField] float weaponLength = 1f;
    [SerializeField] float weaponDamage = 1f;

    void Start()
    {
        canDealDamage = false;
        hasDealtDamage = false;
    }

    void Update()
    {
        Debug.DrawRay(transform.position, -transform.up * weaponLength, Color.red);

        if (canDealDamage && !hasDealtDamage)
        {
            RaycastHit hit;
            int layerMask = 1 << 8; 

            if (Physics.Raycast(transform.position, -transform.up, out hit, weaponLength, layerMask))
            {
                if (hit.transform.TryGetComponent(out HealthSystem health))
                {
                    health.TakeDamage(weaponDamage);
                    Debug.Log($"Enemy dealt {weaponDamage} damage to {hit.transform.name}");
                    hasDealtDamage = true;
                }
            }
        }
    }

    public void StartDealDamage()
    {
        canDealDamage = true;
        hasDealtDamage = false;
    }

    public void EndDealDamage()
    {
        canDealDamage = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * weaponLength);
    }
}
