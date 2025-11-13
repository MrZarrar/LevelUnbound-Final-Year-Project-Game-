using UnityEngine;
using UnityEngine.SceneManagement; 

public class Portal : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private string sceneToLoad;

    [Header("VFX")]
    [SerializeField] private GameObject activationVFX;

    private bool isPlayerInTrigger = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            isPlayerInTrigger = true;
            Debug.Log("PORTAL: Player entered portal.");
            StartLoadingScene();
        }
    }

    private void StartLoadingScene()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("This portal has no 'Scene To Load' set!");
            return;
        }

        if (activationVFX != null)
        {
            Instantiate(activationVFX, transform.position, transform.rotation);
        }


        Debug.Log("PORTAL: Loading scene: " + sceneToLoad);
        SceneManager.LoadScene(sceneToLoad);
    }
}