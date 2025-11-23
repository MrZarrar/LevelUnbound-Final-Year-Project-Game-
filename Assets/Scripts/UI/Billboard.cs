using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponent<Canvas>();

        if (canvas.worldCamera == null)
        {
            canvas.worldCamera = Camera.main;
        }
    }

    void LateUpdate()
    {
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.forward);
        }
    }
}
