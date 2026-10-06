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

    private CinemachineOrbitalFollow orbitalFollow;
    private CinemachineFollow followComponent;

    private void Awake()
    {
        if (cinemachineCam == null)
        {
            cinemachineCam = GetComponent<CinemachineCamera>();
        }

        if (cinemachineCam != null)
        {
            orbitalFollow = cinemachineCam.GetComponent<CinemachineOrbitalFollow>();
            followComponent = cinemachineCam.GetComponent<CinemachineFollow>();
        }
    }

    private void Update()
    {
        HandleCameraRotation();
        HandleCameraZoom();
    }

    /// <summary>
    /// Mengatur rotasi kamera ke SEGALA ARAH saat Klik Kanan (Mouse 1) ditekan dan ditahan.
    /// </summary>
    private void HandleCameraRotation()
    {
        if (Input.GetMouseButton(1)) // Klik Kanan ditahan
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Mode 1: Jika menggunakan CinemachineOrbitalFollow
            if (orbitalFollow != null)
            {
                orbitalFollow.HorizontalAxis.Value += mouseX * rotationSpeedX * Time.deltaTime;
                orbitalFollow.VerticalAxis.Value -= mouseY * rotationSpeedY * Time.deltaTime;
            }
            // Mode 2: Rotasi bebas ke segala arah berpatokan pada target
            else if (cinemachineCam != null)
            {
                Transform targetTransform = cinemachineCam.LookAt != null ? cinemachineCam.LookAt : cinemachineCam.Follow;

                if (targetTransform != null)
                {
                    // Rotasi Horizontal (Sumbu Y Dunia)
                    cinemachineCam.transform.RotateAround(targetTransform.position, Vector3.up, mouseX * rotationSpeedX * Time.deltaTime);

                    // Rotasi Vertikal (Sumbu X Lokal Kamera)
                    cinemachineCam.transform.RotateAround(targetTransform.position, cinemachineCam.transform.right, -mouseY * rotationSpeedY * Time.deltaTime);
                }
            }
        }
    }

    /// <summary>
    /// Mengatur Scroll Mouse untuk Zoom In dan Zoom Out pada Cinemachine 3.x
    /// </summary>
    private void HandleCameraZoom()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            // Mode 1: Jika menggunakan CinemachineOrbitalFollow (Mengatur Radius Orbits Struct)
            if (orbitalFollow != null)
            {
                // Mengambil nilai Radius dari Middle Orbit
                float currentRadius = orbitalFollow.Orbits.Center.Radius;
                float targetRadius = currentRadius - (scrollInput * zoomSpeed);
                targetRadius = Mathf.Clamp(targetRadius, minZoom, maxZoom);

                // Update radius untuk Top, Center, dan Bottom Orbits
                var orbits = orbitalFollow.Orbits;
                orbits.Top.Radius = targetRadius * 0.5f;
                orbits.Center.Radius = targetRadius;
                orbits.Bottom.Radius = targetRadius * 1.2f;

                // Terapkan kembali struct Orbits yang sudah diupdate
                orbitalFollow.Orbits = orbits;
            }
            // Mode 2: Jika menggunakan CinemachineFollow biasa (Mengatur Offset Z)
            else if (followComponent != null)
            {
                Vector3 currentOffset = followComponent.FollowOffset;
                currentOffset.z += scrollInput * zoomSpeed;
                currentOffset.z = Mathf.Clamp(currentOffset.z, -maxZoom, -minZoom);
                followComponent.FollowOffset = currentOffset;
            }
            // Mode 3: Fallback Lens FOV Zoom
            else if (cinemachineCam != null)
            {
                float targetFOV = cinemachineCam.Lens.FieldOfView - (scrollInput * zoomSpeed * 5f);
                cinemachineCam.Lens.FieldOfView = Mathf.Clamp(targetFOV, 15f, 60f);
            }
        }
    }
}