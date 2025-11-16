using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class Projectiles : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private bool destroyOnHit = true;

    [Header("Effects")]
    [SerializeField] private GameObject hitVFX;

    private Rigidbody rb;
    private float damage;
    private float speed; 
    private int targetLayer;
    private int playerLayer;
    private int enemyLayer;
    private Collider[] ownerColliders;

    private List<Component> targetsAlreadyHit = new List<Component>();

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        GetComponent<Collider>().isTrigger = true;

        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    public void Setup(float newDamage, int newTargetLayer, Collider[] owners, float newSpeed)
    {
        this.damage = newDamage;
        this.targetLayer = newTargetLayer;
        this.ownerColliders = owners;
        this.speed = newSpeed;

        if (owners != null)
        {
            Collider myCollider = GetComponent<Collider>();
            foreach (Collider col in owners)
            {
                if (col != null)
                {
                    Physics.IgnoreCollision(myCollider, col);
                }
            }
        }
    }


    void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        int hitLayer = other.gameObject.layer;

        if (hitLayer == targetLayer)
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            HealthSystem player = other.GetComponentInParent<HealthSystem>();

            if (enemy != null)
            {
                if (targetsAlreadyHit.Contains(enemy)) return;
                targetsAlreadyHit.Add(enemy);

                enemy.TakeDamage(damage);
                enemy.HitVFX(transform.position);
                Debug.LogError($"PROJECTILE HIT Enemy: {other.name}. Destroying.", other.gameObject);
            }

            else if (player != null)
            {
                if (targetsAlreadyHit.Contains(player)) return;
                targetsAlreadyHit.Add(player);

                player.TakeDamage(damage);
                player.HitVFX(transform.position);
                Debug.LogError($"PROJECTILE HIT Player: {other.name}. Destroying.", other.gameObject);
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
