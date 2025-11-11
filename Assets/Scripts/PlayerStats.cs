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
    [Header("Core Components")]
    [SerializeField] private HealthSystem healthSystem;
    [SerializeField] GameObject levelUpVFX;

    private DamageDealer playerWeapon;
    private Animator animator;

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

    void Awake()
    {
        if (healthSystem == null) healthSystem = GetComponent<HealthSystem>();

        animator = GetComponent<Animator>();


        UpdateVitals();

        if (xpBar != null)
        {
            xpBar.SetLevel(level, currentXP, xpToNextLevel);
        }
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
            Destroy(UpVFX, 1f);
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
            float agilityBonus = agility.GetValue() * 0.01f;
            float totalSwingSpeed = playerWeapon.GetBaseSwingSpeed() + agilityBonus;
            animator.SetFloat("AttackSpeed", totalSwingSpeed);
        }
    }

    public void SetActiveWeapon(DamageDealer newWeapon)
    {
        playerWeapon = newWeapon;
        UpdateWeaponStats();
    }
}

