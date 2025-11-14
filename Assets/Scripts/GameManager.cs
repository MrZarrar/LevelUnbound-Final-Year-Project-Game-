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
    }

    private void OnDisable()
    {
        HealthSystem.OnPlayerDied -= HandlePlayerDeath;
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
    
    public void LoadScene(string sceneName)
    {
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
}