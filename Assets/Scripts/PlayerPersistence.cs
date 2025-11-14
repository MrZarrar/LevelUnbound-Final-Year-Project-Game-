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
            //destroy duplicate
            Destroy(gameObject);
        }
    }
}