using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectiles : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool destroyOnHit = true;

    [Header("Effects")]
    [SerializeField] private GameObject hitVFX; 

    private Rigidbody rb;
    private float damage;
    private string targetTag;

    private Collider ownerCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        GetComponent<Collider>().isTrigger = true;
    }

    public void Setup(float newDamage, string newTargetTag, Collider owner)
    {
        this.damage = newDamage;
        this.targetTag = newTargetTag;
        this.ownerCollider = owner;
    }

    void Start()
    {

        if (ownerCollider != null)
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), ownerCollider);
        }

        rb.linearVelocity = transform.forward * speed;
        
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.LogWarning($"PROJECTILE HIT: {other.name} (Tag: {other.tag})", other.gameObject);
        // Check if we hit our intended target 
        if (other.CompareTag(targetTag))
        {
            // Try to damage an Enemy
            if (other.TryGetComponent(out Enemy enemy))
            {
                enemy.TakeDamage(damage);
                enemy.HitVFX(transform.position);
                Debug.Log($"Projectile dealt {damage} damage to Enemy");
            }

            // Try to damage a Player
            if (other.TryGetComponent(out HealthSystem player))
            {
                player.TakeDamage(damage);
                player.HitVFX(transform.position);
                Debug.Log($"Projectile dealt {damage} damage to Player");
            }

            // We hit our target, destroy the projectile
            if (destroyOnHit)
            {
                DestroyProjectile();
            }
        }
        // Check if we hit a wall or something else
        else if (!other.CompareTag("Player") && !other.CompareTag("Enemy") && !other.isTrigger)
        {
            Debug.Log($"PROJECTILE HIT OBSTACLE: {other.name}. Destroying.", other.gameObject);
            if (destroyOnHit)
            {
                DestroyProjectile();
            }
        }
    }

    void DestroyProjectile()
    {
        rb.linearVelocity = Vector3.zero;
        
        if (hitVFX != null)
        {
            Instantiate(hitVFX, transform.position, Quaternion.identity);
        }
        
        Destroy(gameObject);
    }
}