using UnityEngine;
using Unity.Cinemachine;

public class cinemachineCameraMovement : MonoBehaviour
{
    [Header("Cinemachine 3.x Reference")]
    [Tooltip("Assign komponen CinemachineCamera Anda di sini")]
    public CinemachineCamera cinemachineCam;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 20f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeedX = 300f;
    [SerializeField] private float rotationSpeedY = 200f;

    [Header("Vertical Rotation Limits")]
    [Tooltip("Sudut minimum kamera melihat ke bawah (derajat)")]
    [SerializeField] private float minVerticalAngle = -10f;
    [Tooltip("Sudut maksimum kamera melihat ke atas (derajat)")]
    [SerializeField] private float maxVerticalAngle = 60f;

    [Header("Smoothing Settings")]
    [Tooltip("Kehalusan rotasi kamera")]
    [SerializeField] private float rotationSmoothness = 15f;
    [Tooltip("Kehalusan zoom kamera")]
    [SerializeField] private float zoomSmoothness = 15f;

    private CinemachineOrbitalFollow orbitalFollow;
    
    private float smoothMouseX = 0f;
    private float smoothMouseY = 0f;
    private float targetZoomRadius = 0f;

    private void Awake()
    {
        if (cinemachineCam == null)
        {
            cinemachineCam = GetComponent<CinemachineCamera>();
        }

        if (cinemachineCam != null)
        {
            orbitalFollow = cinemachineCam.GetComponent<CinemachineOrbitalFollow>();
        }
    }

    private void Start()
    {
        if (orbitalFollow != null)
        {
            targetZoomRadius = orbitalFollow.Orbits.Center.Radius;
            targetZoomRadius = Mathf.Clamp(targetZoomRadius, minZoom, maxZoom);
        }
        else
        {
            Debug.LogWarning("Komponen CinemachineOrbitalFollow tidak ditemukan pada CinemachineCamera! Harap tambahkan komponen tersebut.");
        }
    }

    // Menggunakan LateUpdate dengan Time.smoothDeltaTime untuk hasil pergerakan paling stabil
    private void LateUpdate()
    {
        HandleCameraRotation();
        HandleCameraZoom();
    }

    private void HandleCameraRotation()
    {
        float rawMouseX = 0f;
        float rawMouseY = 0f;

        // Ambil input hanya saat Klik Kanan ditahan
        if (Input.GetMouseButton(1))
        {
            rawMouseX = Input.GetAxis("Mouse X");
            rawMouseY = Input.GetAxis("Mouse Y");
        }

        // Menggunakan Time.smoothDeltaTime agar transisi frame rate tidak menyebabkan jitter/stutter
        float dt = Time.smoothDeltaTime;

        // Interpolasi halus untuk input mouse
        smoothMouseX = Mathf.Lerp(smoothMouseX, rawMouseX, rotationSmoothness * dt);
        smoothMouseY = Mathf.Lerp(smoothMouseY, rawMouseY, rotationSmoothness * dt);

        if (orbitalFollow != null && (Mathf.Abs(smoothMouseX) > 0.001f || Mathf.Abs(smoothMouseY) > 0.001f))
        {
            // Kontrol rotasi horizontal dan vertikal menggunakan sistem orbital Cinemachine 3.x
            orbitalFollow.HorizontalAxis.Value += smoothMouseX * rotationSpeedX * dt;

            float targetVertical = orbitalFollow.VerticalAxis.Value - (smoothMouseY * rotationSpeedY * dt);
            orbitalFollow.VerticalAxis.Value = Mathf.Clamp(targetVertical, minVerticalAngle, maxVerticalAngle);
        }
    }

    private void HandleCameraZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        float dt = Time.smoothDeltaTime;

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            targetZoomRadius -= scrollInput * zoomSpeed;
            targetZoomRadius = Mathf.Clamp(targetZoomRadius, minZoom, maxZoom);
        }

        if (orbitalFollow != null)
        {
            float currentRadius = orbitalFollow.Orbits.Center.Radius;
            float smoothedRadius = Mathf.Lerp(currentRadius, targetZoomRadius, zoomSmoothness * dt);

            var orbits = orbitalFollow.Orbits;
            orbits.Top.Radius = Mathf.Clamp(smoothedRadius * 0.5f, minZoom, maxZoom);
            orbits.Center.Radius = Mathf.Clamp(smoothedRadius, minZoom, maxZoom);
            orbits.Bottom.Radius = Mathf.Clamp(smoothedRadius * 1.2f, minZoom, maxZoom);

            orbitalFollow.Orbits = orbits;
        }
    }
}