using UnityEngine;
using Unity.Cinemachine;

public class ThirdPersonCameraController : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float zoomLerpSpeed = 10f;
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float maxDistance = 15f;

    private CinemachineCamera cineCam;
    private CinemachineOrbitalFollow orbitalFollow;

    private float targetZoom;
    private float currentZoom;

    void Start()
    {
        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Get Cinemachine components
        cineCam = GetComponent<CinemachineCamera>();
        orbitalFollow = cineCam.GetComponent<CinemachineOrbitalFollow>();

        if (orbitalFollow == null)
        {
            Debug.LogError("No CinemachineOrbitalFollow found on camera!");
            return;
        }

      
        targetZoom = currentZoom = orbitalFollow.Radius;
    }

    void Update()
    {
        HandleZoom();

        // Unlock mouse with ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minDistance, maxDistance);
        }

        currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomLerpSpeed);
        orbitalFollow.Radius = currentZoom;
    }
}