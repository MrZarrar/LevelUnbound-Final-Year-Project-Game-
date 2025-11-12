using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Enemy Data")]
public class EnemyData : ScriptableObject
{
    public GameObject enemyPrefab;
    
    [Header("Effects")]
    public GameObject spawnVFX;

    
    [Header("Enemy Stats")]
    public float health = 3;

}  