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
    private int targetLayer; 
    private int playerLayer;
    private int enemyLayer;

    private Collider ownerCollider;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        GetComponent<Collider>().isTrigger = true;

        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    public void Setup(float newDamage, int newTargetLayer, Collider owner)
    {
        this.damage = newDamage;
        this.targetLayer = newTargetLayer;
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
        int hitLayer = other.gameObject.layer;

        if (hitLayer == targetLayer)
        {
            if (hitLayer == enemyLayer)
            {
                if (other.TryGetComponent(out Enemy enemy))
                {
                    enemy.TakeDamage(damage);
                    enemy.HitVFX(transform.position);
                }
            }
            else if (hitLayer == playerLayer)
            {
                if (other.TryGetComponent(out HealthSystem player))
                {
                    player.TakeDamage(damage);
                    player.HitVFX(transform.position);
                }
            }

            if (destroyOnHit) DestroyProjectile();
        }

        else if (!other.isTrigger && hitLayer != playerLayer && hitLayer != enemyLayer)
        {
            Debug.LogError($"PROJECTILE HIT OBSTACLE: {other.name}. Destroying.", other.gameObject);
            if (destroyOnHit) DestroyProjectile();
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