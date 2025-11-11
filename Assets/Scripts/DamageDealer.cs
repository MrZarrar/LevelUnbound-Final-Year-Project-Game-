using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DamageDealer : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] private float baseWeaponDamage = 10f;
    [SerializeField] private float baseSwingSpeed = 1.0f; // 1.0 = 100% speed
    
    [Header("VFX")]
    [SerializeField] private GameObject slashVFX;
    [SerializeField] private float vfxDuration = 0.5f;

    private Collider hitboxCollider;
    private List<Collider> collidersAlreadyHit = new List<Collider>();
    
    private float totalDamage;

    void Start()
    {
        hitboxCollider = GetComponent<Collider>();
        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false;
        
    }

    public void StartDealDamage()
    {
        collidersAlreadyHit.Clear();
        hitboxCollider.enabled = true;

        if (slashVFX != null)
        {
            GameObject vfx = Instantiate(slashVFX, transform.position, transform.rotation);
            vfx.transform.SetParent(transform); 
            vfx.transform.localPosition = Vector3.zero; 
            Destroy(vfx, vfxDuration); 
        }
    }

    public void EndDealDamage()
    {
        hitboxCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy") && !collidersAlreadyHit.Contains(other))
        {
            collidersAlreadyHit.Add(other);

            if (other.TryGetComponent(out Enemy enemy))
            {
                Vector3 hitPoint = other.ClosestPoint(transform.position);

                enemy.TakeDamage(totalDamage);
                enemy.HitVFX(hitPoint);
                
                Debug.Log($"Player dealt {totalDamage} damage to {other.name}");
            }
        }
    }

    public void UpdateDamage(float strengthBonus)
    {

        if (strengthBonus < 0f)
        {
            totalDamage = baseWeaponDamage * strengthBonus;
        }
        else
        {
            totalDamage = baseWeaponDamage + strengthBonus;
        }

        Debug.Log($"DamageDealer total damage updated to {totalDamage}");
    }

    public float GetBaseSwingSpeed()
    {
        return baseSwingSpeed;
    }
}