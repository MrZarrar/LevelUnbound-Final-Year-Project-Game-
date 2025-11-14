using UnityEngine;

public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence instance;

    void Awake()
    {
        // Check if a Player already exists
        if (instance == null)
        {
            // If not, THIS is the main player.
            instance = this;
            
            // Mark it to survive scene loads
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // move to duplicate
            instance.transform.position = transform.position;
            instance.transform.rotation = transform.rotation;
            
            //destroy duplicate
            Destroy(gameObject);
        }
    }
}