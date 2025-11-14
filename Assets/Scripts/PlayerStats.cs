using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
    public string name; 
    public int baseValue; 
    public int GetValue() { return baseValue; }
}

public class PlayerStats : MonoBehaviour


{

    [Header("Movement Stats")]
    [SerializeField] private float baseSprintSpeed = 6.0f;
    [SerializeField] private float baseStaminaRegen = 2f;
    [SerializeField] private float baseStaminaDrain = 90f;
    [Space]
    [SerializeField] private float agiSprintBonus = 0.02f;     // Bonus speed per agility point
    [SerializeField] private float agiRegenBonus = 0.01f;      // Bonus regen per agility point
    [SerializeField] private float agiDrainReduction = 0.005f;  // Drain reduction per agility point
    [SerializeField] private float minStaminaDrain = 2f;

    [Header("Ranged Combat")]
    [SerializeField] private GameObject manaBlastPrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float manaCost = 10f;

    [SerializeField] private RectTransform reticleRectTransform;

    [SerializeField] private LayerMask aimLayerMask;


    [Header("Core Components")]
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] GameObject levelUpVFX;

    private DamageDealer playerWeapon;
    private Animator animator;
    private Character character;

    [Header("Leveling")]
    [SerializeField] private XPBar xpBar;
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    [Header("Primary Stats")]
    public Stat strength;
    public Stat agility;
    public Stat intelligence;
    public Stat vitality;

    private int enemyLayer;

    void Awake()
    {
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();

        animator = GetComponent<Animator>();
        character = GetComponent<Character>();


        UpdateAllStats();

        if (xpBar != null)
        {
            xpBar.SetLevel(level, currentXP, xpToNextLevel);
        }

        enemyLayer = LayerMask.NameToLayer("Enemy");
    }

    public void AddXP(int xpAmount)
    {
        currentXP += xpAmount;
        if (xpBar != null) xpBar.SetXP(currentXP);

        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        if (levelUpVFX != null)
        {
            GameObject UpVFX = Instantiate(levelUpVFX, transform.position, Quaternion.identity);
        }
        currentXP -= xpToNextLevel;
        xpToNextLevel = (int)(xpToNextLevel * 1.2f);

        // Increase stats
        strength.baseValue++;
        agility.baseValue++;
        intelligence.baseValue++;
        vitality.baseValue++;


        UpdateAllStats();

        if (xpBar != null)
        {
            xpBar.SetLevel(level, currentXP, xpToNextLevel);
        }
    }

    private void UpdateAllStats()
    {
        UpdateVitals();
        UpdateWeaponStats();
        UpdateMovementStats();
    }

    private void UpdateMovementStats()
    {
        if (character == null) return; 

        int agi = agility.GetValue();

        float totalSprintSpeed = baseSprintSpeed + (agi * agiSprintBonus);
        float totalStaminaRegen = baseStaminaRegen + (agi * agiRegenBonus);
        
        float totalStaminaDrain = baseStaminaDrain - (agi * agiDrainReduction);
        
        totalStaminaDrain = Mathf.Max(totalStaminaDrain, minStaminaDrain);

        character.SetMovementStats(totalSprintSpeed, totalStaminaRegen, totalStaminaDrain);
    }


    private void UpdateVitals()
    {
        float newMaxHealth = vitality.GetValue() * 15;
        float newMaxMana = intelligence.GetValue() * 10;
        float newMaxStamina = agility.GetValue() * 10;

        if (healthSystem != null)
        {
            healthSystem.InitializeVitals(newMaxHealth, newMaxMana, newMaxStamina);
        }
    }

    private void UpdateWeaponStats()
    {
        // Check if a weapon is actually equipped
        if (playerWeapon == null)
        {
            if (animator != null) animator.SetFloat("AttackSpeed", 1f);
            return;
        }

        // Update Weapon Damage
        float strengthBonus = 1.0f + (strength.GetValue() * 0.1f);
        playerWeapon.UpdateDamage(strengthBonus);

        // Update Swing Speed
        if (animator != null)
        {
            float agilityBonus = agility.GetValue() * 0.005f;
            float totalSwingSpeed = playerWeapon.GetBaseSwingSpeed() + agilityBonus;
            animator.SetFloat("AttackSpeed", totalSwingSpeed);
        }
    }

    public void SetActiveWeapon(DamageDealer newWeapon)
    {
        playerWeapon = newWeapon;
        UpdateWeaponStats();
    }
    
    public void CastManaBlast()
    {
        if (manaBlastPrefab == null)
        {
            Debug.LogError("Mana Blast Prefab is not set in PlayerStats!");
            return;
        }

        if (!healthSystem.TryUseMana(manaCost))
        {
            Debug.Log("Not enough mana to cast!");
            return; 
        }

        Vector3 targetPoint;

        Ray ray = Camera.main.ScreenPointToRay(reticleRectTransform.position);

        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayerMask))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        Vector3 direction = (targetPoint - projectileSpawnPoint.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        GameObject blast = Instantiate(
            manaBlastPrefab, 
            projectileSpawnPoint.position, 
            lookRotation 
        );

        float baseSpellDamage = 5f; 
        float intelligenceMultiplier = 1.0f + (intelligence.GetValue() * 0.1f); 

        float damage = baseSpellDamage * intelligenceMultiplier;

        Projectiles projectileScript = blast.GetComponent<Projectiles>();
        if (projectileScript != null)
        {
            
            projectileScript.Setup(damage, enemyLayer, GetComponent<CharacterController>());
        }
    }
}

