using System.Collections;
using System.Reflection; 
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EnemyPlayModeTests
{
    [UnityTest]
    public IEnumerator StartDealDamage_EnablesCollider()
    {
        EnemyDamageDealer dealer = CreateDealerOnly(out Collider collider);

        yield return null; 
        Assert.IsFalse(collider.enabled, "Collider should start disabled.");

        dealer.StartDealDamage();
        yield return null;


        Assert.IsTrue(collider.enabled, "StartDealDamage should enable collider.");

        Object.Destroy(dealer.gameObject);
    }



    private EnemyDamageDealer CreateDealerOnly(out Collider collider)
    {
        var dealerGO = new GameObject("DamageDealer_Test");
        
        collider = dealerGO.AddComponent<SphereCollider>();
        collider.isTrigger = true;

        var dealer = dealerGO.AddComponent<EnemyDamageDealer>();
        

        
        return dealer;
    }

    private Enemy CreateEnemyWithDealers(out EnemyDamageDealer leftDealer)
    {

        GameObject enemyGO = new GameObject("Enemy_Test");
        enemyGO.AddComponent<UnityEngine.AI.NavMeshAgent>(); 
        enemyGO.AddComponent<Animator>(); 
        Enemy enemy = enemyGO.AddComponent<Enemy>();

        leftDealer = CreateDealerOnly(out Collider col);
        leftDealer.transform.SetParent(enemyGO.transform);


        FieldInfo leftHandField = typeof(Enemy).GetField("leftHandDealer", BindingFlags.NonPublic | BindingFlags.Instance);
        
        if (leftHandField != null)
        {
            leftHandField.SetValue(enemy, leftDealer); 
        }
        else
        {
            Debug.LogError("Could not find field 'leftHandDealer' on Enemy script.");
        }

        EnemyData dummyData = ScriptableObject.CreateInstance<EnemyData>();
        dummyData.health = 10;
        dummyData.meleeWeaponDamage = 5;
        
        FieldInfo dataField = typeof(Enemy).GetField("enemyData", BindingFlags.NonPublic | BindingFlags.Instance);
        if(dataField != null) dataField.SetValue(enemy, dummyData);

        return enemy;
    }
}