using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Config")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private Transform[] spawnPoints;

    [Header("Boss Config")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    
    [Header("Boss Minion Config")]
    [SerializeField] private GameObject minionPrefab; 
    [SerializeField] private int minionCount = 4; 
    [SerializeField] private float minionSpawnInterval = 3f; 


    [Header("UI")]
    [SerializeField] private TextMeshProUGUI enemiesLeftText;
    [SerializeField] private TextMeshProUGUI waveCounterText;

    private int currentWaveIndex = 0;
    private int enemiesLeftInWave;
    private bool bossHasSpawned = false;

    private void OnEnable()
    {
        Enemy.OnEnemyDied += HandleEnemyDied;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDied -= HandleEnemyDied;
    }

    void Start()
    {
        StartWave(currentWaveIndex);
    }

    void StartWave(int waveIndex)
    {
        if (waveIndex >= waves.Length)
        {
            if (!bossHasSpawned && bossPrefab != null)
            {
                SpawnBoss();
            }
            else if (bossHasSpawned)
            {
                Debug.Log("All waves and boss cleared!");
            }
            return;
        }

        Wave currentWave = waves[waveIndex];
        enemiesLeftInWave = currentWave.spawnCount;
        UpdateUI();

        StartCoroutine(SpawnWaveRoutine(currentWave));
    }

    IEnumerator SpawnWaveRoutine(Wave wave)
    {
        for (int i = 0; i < wave.spawnCount; i++)
        {
            EnemyData enemyData = wave.enemiesToSpawn[Random.Range(0, wave.enemiesToSpawn.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            Instantiate(enemyData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            yield return new WaitForSeconds(wave.timeBetweenSpawns);
        }
    }

    void SpawnBoss()
    {
        Debug.Log("Spawning BOSS!");
        bossHasSpawned = true;
        
        enemiesLeftInWave = 1; 

        Transform spawnPoint = bossSpawnPoint != null ? bossSpawnPoint : spawnPoints[Random.Range(0, spawnPoints.Length)];
        Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);

        if (minionPrefab != null && minionCount > 0)
        {
            enemiesLeftInWave += minionCount; 
            
            StartCoroutine(SpawnMinionsRoutine());
        }
        
        UpdateUIForBossWave();
    }


    IEnumerator SpawnMinionsRoutine()
    {
        for (int i = 0; i < minionCount; i++)
        {
            yield return new WaitForSeconds(minionSpawnInterval);
            
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Instantiate(minionPrefab, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Spawning minion!");
        }
    }


    void HandleEnemyDied()
    {
        if (bossHasSpawned)
        {
            enemiesLeftInWave--;
            if (enemiesLeftInWave <= 0)
            {
                Debug.Log("BOSS DEFEATED! YOU WIN!");
                enemiesLeftText.text = "YOU WIN!";
                waveCounterText.text = "";
                // TODO: Add game win logic 
            }
            return; 
        }

        enemiesLeftInWave--;
        UpdateUI();

        if (enemiesLeftInWave <= 0)
        {
            currentWaveIndex++;
            StartWave(currentWaveIndex);
        }
    }

    void UpdateUI()
    {
        if (bossHasSpawned) return;

        enemiesLeftText.text = $"Enemies Left: {enemiesLeftInWave}";

        int wavesLeft = waves.Length - currentWaveIndex;

        if (wavesLeft > 0)
        {
            waveCounterText.text = $"Waves until Boss: {wavesLeft}";
        }
        else
        {
            waveCounterText.text = "Next wave is the BOSS!";
        }
        
        
    } 
    void UpdateUIForBossWave()
    {
        enemiesLeftText.text = $"Enemies Left: {enemiesLeftInWave}";
        waveCounterText.text = "!! BOSS WAVE !!";
    }
}