using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button restartButton;

    [SerializeField] private GameObject loadingPanel;

    [SerializeField] private float gameOverDelay = 5f;

    private string targetPortalID;
    private Transform defaultSpawnPoint;

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

        gameOverPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;

        // Reload the entire scene from scratch
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadScene(string sceneName, string portalID)
    {
        this.targetPortalID = portalID;
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        Time.timeScale = 1f;

        loadingPanel.SetActive(true);

        yield return null;

        SceneManager.LoadScene(sceneName);


        loadingPanel.SetActive(false);
    }

    public void RegisterDefaultSpawn(Transform spawn)
    {
        defaultSpawnPoint = spawn;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Transform spawnPoint = FindSpawnPoint();

        if (PlayerPersistence.instance != null)
        {
            PlayerPersistence.instance.transform.position = spawnPoint.position;
            PlayerPersistence.instance.transform.rotation = spawnPoint.rotation;
        }
        
        targetPortalID = null;
        defaultSpawnPoint = null;
    }

    private Transform FindSpawnPoint()
    {
        // Try to find the portal ID
        if (!string.IsNullOrEmpty(targetPortalID))
        {
            Portal[] portals = FindObjectsOfType<Portal>();
            foreach (Portal portal in portals)
            {
                if (portal.portalID == targetPortalID)
                {
                    return portal.transform; 
                }
            }
            Debug.LogWarning("Portal target '" + targetPortalID + "' not found! Using default spawn.");
        }

        // If no ID, use the "dummy" player's default spawn
        if (defaultSpawnPoint != null)
        {
            return defaultSpawnPoint;
        }
        
        // If all else fails, just return the player's current spot
        Debug.LogError("No spawn point found! Player will spawn in place.");
        return PlayerPersistence.instance.transform;
    }
}