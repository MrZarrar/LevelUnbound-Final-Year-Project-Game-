using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HealthSystem : MonoBehaviour
{

    public static event Action OnPlayerDied;

    private float maxHealth;
    private float maxMana;
    private float maxStamina;

    private float currentHealth;
    private float currentMana;
    private float currentStamina;

    [Header("UI References")]
    [SerializeField] HealthBar playerHealthBar;
    [SerializeField] ManaBar playerManaBar;
    [SerializeField] StaminaBar playerStaminaBar;

    [Header("Effects")]
    [SerializeField] GameObject hitVFX;
    [SerializeField] GameObject ragdoll;

    Animator animator;

    private bool isDying = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void InitializeVitals(float newMaxHealth, float newMaxMana, float newMaxStamina)
    {
        maxHealth = newMaxHealth;
        maxMana = newMaxMana;
        maxStamina = newMaxStamina;

        // Heal to full
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;

        // Update UI
        if (playerHealthBar != null) playerHealthBar.SetMaxHP((int)maxHealth);
        if (playerManaBar != null) playerManaBar.SetMaxMana((int)maxMana);
        if (playerStaminaBar != null) playerStaminaBar.SetMaxStamina(maxStamina);
    }

    public bool TryUseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            if (playerStaminaBar != null) playerStaminaBar.SetStamina(currentStamina);
            return true;
        }
        return false;
    }

    public void RegenerateStamina(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        if (playerStaminaBar != null) playerStaminaBar.SetStamina(currentStamina);
    }

    public float GetCurrentStamina() { return currentStamina; }
    public bool IsStaminaFull() { return currentStamina >= maxStamina; }

    public bool TryUseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            if (playerManaBar != null) playerManaBar.SetMana((int)currentMana);
            return true;
        }
        Debug.Log("Not enough mana!");
        return false;
    }

    public void RegenerateMana(float amount)
    {
        if (currentMana >= maxMana)
        {
            return;
        }

        currentMana += amount;
        currentMana = Mathf.Clamp(currentMana, 0, maxMana); // Don't go over max

        if (playerManaBar != null)
        {
            playerManaBar.SetMana((int)currentMana);
        }
    }

    public bool IsManaFull()
    {
        return currentMana >= maxMana;
    }



    public void RegenerateHealth(float amount)
    {
        if (currentHealth >= maxHealth)
        {
            return;
        }

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Don't go over max

        if (playerHealthBar != null)
        {
            playerHealthBar.SetHP((int)currentHealth);
        }
    }

    public bool IsHealthFull()
    {
        return currentHealth >= maxHealth;
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;
        
        if (playerHealthBar != null) playerHealthBar.SetHP((int)currentHealth);
        if (playerManaBar != null) playerManaBar.SetMana((int)currentMana);
        if (playerStaminaBar != null) playerStaminaBar.SetStamina(currentStamina);
    }


    public void TakeDamage(float damageAmount)
    {
        if (isDying) return;
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        animator.SetTrigger("damage");

        if (playerHealthBar != null) playerHealthBar.SetHP((int)currentHealth);
        if (currentHealth <= 0 && !isDying)
        {
            isDying = true;
            Die();

        }  
    }

    void Die()
    {
        Instantiate(ragdoll, transform.position, transform.rotation);
        OnPlayerDied?.Invoke();
        Debug.Log("Player Died!");
        gameObject.SetActive(false);
    }

    public void HitVFX(Vector3 hitPoint)
    {
        GameObject hitInstance = Instantiate(hitVFX, hitPoint, Quaternion.identity);
    }
}
