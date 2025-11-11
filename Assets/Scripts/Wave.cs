using UnityEngine;

[CreateAssetMenu(fileName = "New Wave", menuName = "Wave")]
public class Wave : ScriptableObject
{
    public EnemyData[] enemiesToSpawn; 
    public int spawnCount;            
    public float timeBetweenSpawns = 1f;
}