using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; 

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private Wave[] waves;
    [SerializeField] private Transform[] spawnPoints;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI enemiesLeftText;
    [SerializeField] private TextMeshProUGUI waveCounterText;

    private int currentWaveIndex = 0;
    private int enemiesLeftInWave;

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
            Debug.Log("ALL WAVES DEFEATED!");
            // boss spawn 
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
            // Get random enemy type and spawn point
            EnemyData enemyData = wave.enemiesToSpawn[Random.Range(0, wave.enemiesToSpawn.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // spawn enemy 
            Instantiate(enemyData.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

            yield return new WaitForSeconds(wave.timeBetweenSpawns);
        }
    }

    void HandleEnemyDied()
    {
        enemiesLeftInWave--;
        UpdateUI();

        if (enemiesLeftInWave <= 0)
        {
            // Start next wave
            currentWaveIndex++;
            StartWave(currentWaveIndex);
        }
    }

    void UpdateUI()
    {
        enemiesLeftText.text = $"Enemies Left: {enemiesLeftInWave}";
        
        int wavesLeft = waves.Length - currentWaveIndex;
        waveCounterText.text = $"Waves until Boss: {wavesLeft}";
    }
}