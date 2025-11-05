using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stat
{
    public string name; 
    public int baseValue; 
    
    
    public int GetValue()
    {
        return baseValue;
    }
}

public class PlayerStats : MonoBehaviour
{
    [Header("Core Components")]
    [SerializeField] private HealthSystem healthSystem;

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

        if (healthSystem == null)
        {
            healthSystem = GetComponent<HealthSystem>();
        }

        UpdateAllStats();
        
        if (xpBar != null)
        {
            xpBar.SetLevel(level, currentXP, xpToNextLevel);
        }
    }

    // enemies call this when defeated
    public void AddXP(int xpAmount)
    {
        currentXP += xpAmount;

        if (xpBar != null)
        {
            xpBar.SetXP(currentXP);
        }


        if (currentXP >= xpToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        currentXP -= xpToNextLevel;

        xpToNextLevel = (int)(xpToNextLevel * 1.2f);

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

        float newMaxHealth = vitality.GetValue() * 15; 
        
        float newMaxMana = intelligence.GetValue() * 10;
        
        float newMaxStamina = agility.GetValue() * 10;
        
        if (healthSystem != null)
        {
            healthSystem.InitializeVitals(newMaxHealth, newMaxMana, newMaxStamina);
        }

    }
    
}
