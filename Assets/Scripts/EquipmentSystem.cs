using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipmentSystem : MonoBehaviour
{
    [SerializeField] GameObject weaponHolder;
    [SerializeField] GameObject weapon;
    [SerializeField] GameObject weaponSheath;

    GameObject currentWeaponInHand;
    GameObject currentWeaponInSheath;

    private PlayerStats playerStats;
    private Animator animator;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("EquipmentSystem cannot find PlayerStats on this object!");
            return;
        }

        currentWeaponInSheath = Instantiate(weapon, weaponSheath.transform);
        
        playerStats.SetActiveWeapon(currentWeaponInSheath.GetComponentInChildren<DamageDealer>());
    }

    public void DrawWeapon()
    {
        currentWeaponInHand = Instantiate(weapon, weaponHolder.transform);
        Destroy(currentWeaponInSheath);

        playerStats.SetActiveWeapon(currentWeaponInHand.GetComponentInChildren<DamageDealer>());

    }

    public void SheathWeapon()
    {
        currentWeaponInSheath = Instantiate(weapon, weaponSheath.transform);
        Destroy(currentWeaponInHand);

        playerStats.SetActiveWeapon(currentWeaponInSheath.GetComponentInChildren<DamageDealer>());
        
    }

    public void StartDealDamage()
    {
        currentWeaponInHand.GetComponentInChildren<DamageDealer>().StartDealDamage();
    }
    public void EndDealDamage()
    {
        currentWeaponInHand.GetComponentInChildren<DamageDealer>().EndDealDamage();
    }
}