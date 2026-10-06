using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class bogieRotation : MonoBehaviour
{
    [Header("Spline Reference")]
    [Tooltip("Seret GameObject yang memiliki komponen SplineContainer di sini")]
    [SerializeField] private SplineContainer splineContainer;

    [Header("Rotation Settings")]
    [Tooltip("Suaikan rotasi jika model 3D bogie terbalik atau miring (dalam derajat). Contoh: X: 0, Y: -90, Z: 0")]
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;

    [Tooltip("Kecepatan rotasi bogie (0 untuk langsung/instant)")]
    [SerializeField] private float rotationSpeed = 10f;

    private void Update()
    {
        if (splineContainer == null || splineContainer.Spline == null)
            return;

        RotateBogieToSpline();
    }

    private void RotateBogieToSpline()
    {
        // 1. Cari titik terdekat pada spline berdasarkan posisi bogie saat ini
        SplineUtility.GetNearestPoint(
            splineContainer.Spline,
            splineContainer.transform.InverseTransformPoint(transform.position),
            out float3 nearestPointLocal,
            out float t
        );

        // 2. Dapatkan arah (tangent) rel pada titik t tersebut
        Vector3 splineTangentLocal = SplineUtility.EvaluateTangent(splineContainer.Spline, t);
        
        // 3. Ubah arah tangent dari lokal spline ke arah World Space
        Vector3 splineTangentWorld = splineContainer.transform.TransformDirection(splineTangentLocal);

        // Abaikan kemiringan vertikal (Y) agar bogie hanya berotasi mendatar
        splineTangentWorld.y = 0f;

        if (splineTangentWorld.sqrMagnitude < 0.0001f)
            return;

        // 4. Hitung rotasi dasar berdasarkan arah rel
        Quaternion baseTargetRotation = Quaternion.LookRotation(splineTangentWorld.normalized, Vector3.up);

        // 5. Tambahkan offset rotasi dari Inspector
        Quaternion targetRotation = baseTargetRotation * Quaternion.Euler(rotationOffset);

        // 6. Terapkan rotasi tanpa mengubah posisi
        if (rotationSpeed > 0f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
        else
        {
            transform.rotation = targetRotation;
        }
    }
}