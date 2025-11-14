using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    [SerializeField] private GameObject loadingPanel;

    [SerializeField] private float gameOverDelay = 5f;

    [SerializeField] private TextMeshProUGUI dungeonTitleText;
    [SerializeField] private float dungeonIntroDuration = 3f;

    public TextMeshProUGUI enemiesLeftText;
    public TextMeshProUGUI waveCounterText;
    public TextMeshProUGUI waveAnnouncementText;

    

    private Portal.SpawnTargetType spawnType;
    private string targetPortalID;
    private string targetSpawnPointID;

    private PlayerStatData savedStats;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameOverPanel.SetActive(false);
        Time.timeScale = 1f;

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (enemiesLeftText != null) enemiesLeftText.text = "";
        if (waveCounterText != null) waveCounterText.text = "";
        if (waveAnnouncementText != null) waveAnnouncementText.gameObject.SetActive(false);
        
        if(dungeonTitleText != null) dungeonTitleText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        HealthSystem.OnPlayerDied += HandlePlayerDeath;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        HealthSystem.OnPlayerDied -= HandlePlayerDeath;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(ShowGameOverScreenRoutine());
    }

    private IEnumerator ShowGameOverScreenRoutine()
    {

        yield return new WaitForSeconds(gameOverDelay);

        Camera overviewCamera = GameObject.FindWithTag("OverviewCamera")?.GetComponent<Camera>();

        if (overviewCamera != null)
        {
            overviewCamera.enabled = true;
        }
        else
        {
            Debug.LogWarning("GameManager: No 'OverviewCamera' found. Can't switch camera on death.");
        }


        gameOverPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StoreSavedStats(PlayerStatData data)
    {
        savedStats = data;
    }

    public void RestartGame()
    {

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        StartCoroutine(LoadSceneRoutine(SceneManager.GetActiveScene().name));
        
    }

    public void LoadScene(string sceneName, Portal.SpawnTargetType type, string portalID, string spawnID)
    {
        this.spawnType = type;
        this.targetPortalID = portalID;
        this.targetSpawnPointID = spawnID;
        
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        Time.timeScale = 1f;
        if(loadingPanel != null) loadingPanel.SetActive(true);

        float startTime = Time.time; 

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        while (!asyncLoad.isDone)
        {

            if (asyncLoad.progress >= 0.9f)
            {
                if (Time.time - startTime >= 3.0f)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }
            yield return null; 
        }
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PlayerPersistence.instance != null)
        {
            GameObject player = PlayerPersistence.instance.gameObject;

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = true;
            
            Character character = player.GetComponent<Character>();
            if (character != null) character.enabled = true;

            HealthSystem health = player.GetComponent<HealthSystem>();
            if (health != null && health.playerSkin != null)
            {
                health.playerSkin.SetActive(true);
            }

            Transform spawnPoint = FindSpawnPoint();
            
            if (spawnPoint != null)
            {
                if (controller != null) controller.enabled = false;
                player.transform.position = spawnPoint.position;
                player.transform.rotation = spawnPoint.rotation;
                if (controller != null) controller.enabled = true;
            }
            
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (scene.name == "Village") 
            {
                playerStats.SaveStats(this);
            }
            else if (savedStats != null) 
            {
                playerStats.LoadStats(savedStats);
                health.FullHeal();
            }
        }
        
        if (spawnType == Portal.SpawnTargetType.PortalID && !string.IsNullOrEmpty(targetPortalID))
        {
            Portal[] portals = FindObjectsByType<Portal>(FindObjectsSortMode.None);
            foreach (Portal portal in portals)
            {
                if (portal.portalID == targetPortalID)
                {
                    portal.StartCooldown(10f);
                    break;
                }
            }
        }

        targetPortalID = null;
        targetSpawnPointID = null;
        
        if(loadingPanel != null) loadingPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (scene.name != "Village")
        {
            StartCoroutine(DungeonIntroSequence());
        }
    }
    
    private IEnumerator DungeonIntroSequence()
    {
        // Find the cameras
        GameObject playerCamObject = PlayerPersistence.instance.GetComponentInChildren<Camera>().gameObject;
        Camera overviewCamera = GameObject.FindWithTag("OverviewCamera")?.GetComponent<Camera>();

        // Switch cameras
        if (playerCamObject != null) playerCamObject.SetActive(false);
        if (overviewCamera != null) overviewCamera.enabled = true;
        if (dungeonTitleText != null) dungeonTitleText.gameObject.SetActive(true);

        // Wait
        yield return new WaitForSeconds(dungeonIntroDuration);

        // Switch cameras back
        if (dungeonTitleText != null) dungeonTitleText.gameObject.SetActive(false);
        if (overviewCamera != null) overviewCamera.enabled = false;
        if (playerCamObject != null) playerCamObject.SetActive(true);
        
        WaveSpawner spawner = FindAnyObjectByType<WaveSpawner>();
        if (spawner != null)
        {
            spawner.BeginSpawning();
        }
        else
        {
            Debug.LogError("Dungeon Intro finished, but no WaveSpawner was found!");
        }
    }

    private Transform FindSpawnPoint()
    {
        if (spawnType == Portal.SpawnTargetType.PortalID && !string.IsNullOrEmpty(targetPortalID))
        {
            Portal[] portals = FindObjectsByType<Portal>(FindObjectsSortMode.None);
            foreach (Portal portal in portals)
            {
                if (portal.portalID == targetPortalID)
                {
                    return portal.transform;
                }
            }
            Debug.LogWarning("Portal target '" + targetPortalID + "' not found! Spawning at 0,0,0.");
        }
        else if (spawnType == Portal.SpawnTargetType.SpawnPointID && !string.IsNullOrEmpty(targetSpawnPointID))
        {
            SpawnPoint[] spawners = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            foreach (SpawnPoint spawn in spawners)
            {
                if (spawn.spawnPointID == targetSpawnPointID)
                {
                    return spawn.transform;
                }
            }
            Debug.LogWarning("SpawnPoint target '" + targetSpawnPointID + "' not found! Spawning at 0,0,0.");
        }

        Debug.LogError("No valid spawn point found! Spawning at world origin (0, 0, 0).");
        return null;
    }
}