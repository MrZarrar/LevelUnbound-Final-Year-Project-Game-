using System.Collections; 
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public enum SpawnTargetType
    {
        PortalID,
        SpawnPointID
    }
    public string portalID;
    [SerializeField] private string sceneToLoad;

    [SerializeField] private SpawnTargetType spawnType;

    [SerializeField] private string targetPortalID;

    [SerializeField] private string targetSpawnPointID;

    private Collider portalCollider;




    [Header("VFX")]
    [SerializeField] private GameObject activationVFX;

    private bool isPlayerInTrigger = false;

    void Awake()
    {
        portalCollider = GetComponent<Collider>();
    }

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

        GameManager.instance.LoadScene(sceneToLoad, spawnType, targetPortalID, targetSpawnPointID);


        Debug.Log("PORTAL: Loading scene: " + sceneToLoad);
    }

    public void StartCooldown(float duration = 10f)
    {
        StartCoroutine(CooldownRoutine(duration));
    }
    
    private IEnumerator CooldownRoutine(float duration)
    {
        if (portalCollider != null)
            portalCollider.enabled = false;
        
        yield return new WaitForSeconds(duration);

        if (portalCollider != null)
            portalCollider.enabled = true;
    }
}