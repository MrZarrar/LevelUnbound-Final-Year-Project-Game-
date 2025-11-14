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

    private Collider[] ownerColliders;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        GetComponent<Collider>().isTrigger = true;

        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("EnemyBody"); 
        if (enemyLayer == -1) // Safety check if "EnemyBody" doesn't exist
        {
            Debug.LogError("Projectile script can't find 'EnemyBody' layer. Check layer names.");
            enemyLayer = LayerMask.NameToLayer("Enemy"); // Fallback
        }
    }

    public void Setup(float newDamage, int newTargetLayer, Collider[] owners)
    {
        this.damage = newDamage;
        this.targetLayer = newTargetLayer;
        this.ownerColliders = owners;
    }

    void Start()
    {

        if (ownerColliders != null)
        {
            Collider myCollider = GetComponent<Collider>();
            foreach (Collider col in ownerColliders)
            {
                if (col != null) // Safety check
                {
                    Physics.IgnoreCollision(myCollider, col);
                }
            }
        }

        rb.linearVelocity = transform.forward * speed;
        
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        int hitLayer = other.gameObject.layer;

        if (hitLayer == targetLayer)
        {
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