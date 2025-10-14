using UnityEngine;

public class EnemyDamageDealer : MonoBehaviour
{
    bool canDealDamage;
    bool hasDealtDamage;

    [SerializeField] float weaponLength = 1.03f;
    [SerializeField] float weaponDamage = 1f;
    [SerializeField] float sweepRadius = 0.1f; 

    private Vector3 lastPosition;

    void Start()
    {
        canDealDamage = false;
        hasDealtDamage = false;
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {

        Debug.DrawRay(transform.position, transform.up * weaponLength, Color.red); // show raycast to see if it is aligned with sword in scene iew

        if (canDealDamage && !hasDealtDamage)
        {
            int layerMask = 1 << 8; 
            Vector3 direction = (transform.position - lastPosition).normalized;
            float distance = Vector3.Distance(transform.position, lastPosition);

          
            if (Physics.SphereCast(lastPosition, sweepRadius, direction, out RaycastHit hit, distance + weaponLength, layerMask)) //sweep ray along with the sword's motion
            {
                if (hit.transform.TryGetComponent(out HealthSystem health))
                {
                    health.TakeDamage(weaponDamage);
                    Debug.Log($"Enemy dealt {weaponDamage} damage to {hit.transform.name}");
                    hasDealtDamage = true;
                }
            }
        }

        lastPosition = transform.position;
    }

    public void StartDealDamage()
    {
        canDealDamage = true;
        hasDealtDamage = false;
        lastPosition = transform.position;
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