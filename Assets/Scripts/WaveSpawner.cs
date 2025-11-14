using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WaveSpawner : MonoBehaviour
{
    [Header("Wave Config")]
    [SerializeField] private Wave[] waves;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private GameObject exitPortalPrefab;
    [SerializeField] private Transform exitPortalSpawnPoint;
    [SerializeField] private float portalSpawnHeightOffset = 2f;

    [Header("Boss Config")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;

    [Header("Boss Minion Config")]
    [SerializeField] private GameObject minionPrefab;
    [SerializeField] private int minionCount = 4;
    [SerializeField] private float minionSpawnInterval = 3f;



    [SerializeField] private float announcementDuration = 2.5f;

    private TextMeshProUGUI enemiesLeftText;
    private TextMeshProUGUI waveCounterText;
    private TextMeshProUGUI waveAnnouncementText;

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

        if (GameManager.instance != null)
        {
            if (enemiesLeftText != null)
            {
                enemiesLeftText.text = ""; 
                enemiesLeftText.gameObject.SetActive(false); 
            }
            if (waveCounterText != null)
            {
                waveCounterText.text = ""; 
                waveCounterText.gameObject.SetActive(false); 
            }
            
            if (waveAnnouncementText != null)
            {
                waveAnnouncementText.gameObject.SetActive(false);
            }
        }
    }

    void Start()
    {

        if (GameManager.instance != null)
        {
            enemiesLeftText = GameManager.instance.enemiesLeftText;
            waveCounterText = GameManager.instance.waveCounterText;
            waveAnnouncementText = GameManager.instance.waveAnnouncementText;
        }
        else
        {
            Debug.LogError("WaveSpawner could not find GameManager! UI will not work.");
            // We can return here to prevent a crash
            return;
        }

        if (enemiesLeftText != null)
        {
            enemiesLeftText.gameObject.SetActive(true);
        }
        if (waveCounterText != null)
        {
            waveCounterText.gameObject.SetActive(true);
        }

    }

    public void BeginSpawning()
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

        StartCoroutine(ShowWaveAnnouncement("Wave " + (currentWaveIndex + 1)));

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

        StartCoroutine(ShowWaveAnnouncement("!! BOSS FIGHT !!"));

        if (minionPrefab != null && minionCount > 0)
        {
            enemiesLeftInWave += minionCount;

            StartCoroutine(SpawnMinionsRoutine());
        }

        UpdateUIForBossWave();
    }

    private IEnumerator ShowWaveAnnouncement(string message)
    {
        waveAnnouncementText.text = message;
        waveAnnouncementText.gameObject.SetActive(true);

        yield return new WaitForSeconds(announcementDuration);

        waveAnnouncementText.gameObject.SetActive(false);
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
        enemiesLeftInWave--; 

        if (enemiesLeftInWave <= 0)
        {
            if (bossHasSpawned)
            {
                Debug.Log("BOSS DEFEATED! YOU WIN!");
                
                // Show win message
                StartCoroutine(ShowWaveAnnouncement("!! BOSS DEFEATED !!"));
                enemiesLeftText.text = "YOU WIN!";
                waveCounterText.text = "";
                
                SpawnExitPortal(); 
            }
            else
            {

                currentWaveIndex++;
                StartWave(currentWaveIndex);
            }
        }
        else
        {

            if (bossHasSpawned)
            {
                UpdateUIForBossWave(); 
            }
            else
            {
                UpdateUI();
            }
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
    
    private void SpawnExitPortal()
    {
        if (exitPortalPrefab == null) return;
        
        Transform spawnPoint = exitPortalSpawnPoint != null ? exitPortalSpawnPoint : bossSpawnPoint;

        Vector3 spawnPosition = spawnPoint.position + (Vector3.up * portalSpawnHeightOffset);
        
        Instantiate(exitPortalPrefab, spawnPosition, Quaternion.identity);
    }
}