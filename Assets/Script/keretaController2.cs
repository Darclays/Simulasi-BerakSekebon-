using UnityEngine;
using UnityEngine.Splines;

public class keretaController2 : MonoBehaviour
{
    [Header("Spline References")]
    public SplineContainer splineContainer;

    [Header("Center Pivot Reference")]
    [Tooltip("Pivot tengah (Empty GameObject) yang akan memegang Mesh Kereta sebagai Child")]
    public Transform centerPivot;

    [Header("Train Physical Settings")]
    [Tooltip("Jarak nyata dari roda/bogie depan ke roda/bogie belakang dalam meter")]
    public float wheelBaseLength = 4f;

    [Header("Rotation Settings (360 Degree Offsets)")]
    [Tooltip("Rotasi offset tambahan dalam derajat (X: Pitch, Y: Yaw/Muter 360, Z: Roll/Miring)")]
    public Vector3 rotationOffset = Vector3.zero;

    [Tooltip("Kecepatan rotasi manual (Yaw) menggunakan tombol Q / E")]
    public float rotationSpeed = 90f;

    [Header("Train Status (Notch System)")]
    [Tooltip("Kecepatan saat ini dalam km/jam")]
    public float currentSpeed = 0f;
    [Tooltip("Kecepatan maksimal kereta dalam km/jam")]
    public float maxSpeed = 110f;

    // Notch saat ini: Positif = Power (P), 0 = Neutral (N), Negatif = Brake (B)
    public int currentNotch = 0;
    [Tooltip("String untuk dikirim ke UI Text (N, P1-P4, B1-B6)")]
    public string notchDisplay = "N";

    [Header("Notch Settings")]
    public int maxPowerNotch = 4;
    public int maxBrakeNotch = 6;

    public float maxAccelerationForce = 15f;
    public float maxBrakeForce = 25f;
    public float naturalFriction = 2f; // Kereta melambat perlahan saat posisi N

    [Header("Notch Percentages (0.0 - 1.0)")]
    // Persentase gaya untuk P1, P2, P3, P4
    public float[] powerPercentages = { 0.25f, 0.50f, 0.75f, 1.00f };
    // Persentase gaya untuk B1 hingga B6
    public float[] brakePercentages = { 0.15f, 0.30f, 0.45f, 0.60f, 0.80f, 1.00f };

    private float distanceTraveled = 0f;
    private float splineLength = 100f;

    void Start()
    {
        // Jika centerPivot belum di-assign di Inspector, gunakan transform script ini sendiri
        if (centerPivot == null)
        {
            centerPivot = transform;
        }

        if (splineContainer != null)
        {
            splineLength = splineContainer.CalculateLength();
        }
    }

    void Update()
    {
        if (splineContainer == null || splineLength <= 0f) return;

        splineLength = splineContainer.CalculateLength();

        // 1. Input Notch & Kalkulasi Kecepatan (Logika Script 2)
        HandleNotchInput();
        CalculateTrainSpeed();
        UpdateNotchDisplay();

        // 2. Konversi Kecepatan dari km/h ke m/s untuk pergerakan Spline
        float speedInMetersPerSecond = currentSpeed / 3.6f;
        distanceTraveled += speedInMetersPerSecond * Time.deltaTime;

        // Loop Jalur Rel (Modulus Jarak)
        distanceTraveled = Mathf.Repeat(distanceTraveled, splineLength);

        // 3. Kontrol Input Rotasi Manual 360 Derajat (Logika Script 1)
        HandleManualRotationInput();

        // 4. Update Posisi & Rotasi Kereta pada Spline
        UpdateTrainTransformOnSpline();
    }

    private void HandleNotchInput()
    {
        // Naik Notch (W)
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (currentNotch < maxPowerNotch)
            {
                currentNotch++;
            }
        }

        // Turun Notch / Rem (S)
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (currentNotch > -maxBrakeNotch)
            {
                currentNotch--;
            }
        }
    }

    private void CalculateTrainSpeed()
    {
        if (currentNotch > 0) // Mode Power (P1 - P4)
        {
            float powerMultiplier = powerPercentages[currentNotch - 1];
            float acceleration = maxAccelerationForce * powerMultiplier;

            currentSpeed += acceleration * Time.deltaTime;
        }
        else if (currentNotch < 0) // Mode Brake (B1 - B6)
        {
            int brakeIndex = Mathf.Abs(currentNotch) - 1;
            float brakeMultiplier = brakePercentages[brakeIndex];
            float deceleration = maxBrakeForce * brakeMultiplier;

            currentSpeed -= deceleration * Time.deltaTime;
        }
        else // Mode Neutral (N)
        {
            currentSpeed -= naturalFriction * Time.deltaTime;
        }

        // Batasi kecepatan antara 0 dan maxSpeed (km/h)
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);
    }

    private void UpdateNotchDisplay()
    {
        if (currentNotch > 0)
        {
            notchDisplay = "P" + currentNotch;
        }
        else if (currentNotch < 0)
        {
            notchDisplay = "B" + Mathf.Abs(currentNotch);
        }
        else
        {
            notchDisplay = "N";
        }
    }

    private void HandleManualRotationInput()
    {
        //if (Input.GetKey(KeyCode.Q))
       if (Input.GetKey(KeyCode.Period))
        {
            rotationOffset.y -= rotationSpeed * Time.deltaTime;
        }
        //(Input.GetKey(KeyCode.E))
        else if (Input.GetKey(KeyCode.Comma))
        {
            rotationOffset.y += rotationSpeed * Time.deltaTime;
        }

        // Jaga agar nilai sudut selalu berputar mulus di rentang 0-360 derajat
        rotationOffset.y = Mathf.Repeat(rotationOffset.y, 360f);
    }

    private void UpdateTrainTransformOnSpline()
    {
        // Hitung Jarak Roda Depan & Roda Belakang dari Center Pivot
        float halfWheelBase = wheelBaseLength * 0.5f;
        float frontDistance = Mathf.Repeat(distanceTraveled + halfWheelBase, splineLength);
        float rearDistance = Mathf.Repeat(distanceTraveled - halfWheelBase, splineLength);

        // Evaluasi Posisi Roda Depan & Belakang di Spline
        Vector3 frontPos = GetPointAtDistance(frontDistance, out _, out Vector3 frontUp);
        Vector3 rearPos = GetPointAtDistance(rearDistance, out _, out Vector3 rearUp);

        // Hitung Posisi Tengah (Center Pivot) Langsung Pada Spline
        Vector3 centerPos = GetPointAtDistance(distanceTraveled, out _, out Vector3 centerUp);

        // Pasang Posisi Center Pivot
        centerPivot.position = centerPos;

        // Rotasi Center Pivot dengan Dukungan Rotasi Offset 360 Derajat
        Vector3 trainDirection = frontPos - rearPos;

        if (trainDirection.sqrMagnitude > 0.001f)
        {
            Vector3 blendedUp = Vector3.Lerp(rearUp, frontUp, 0.5f);
            Vector3 finalUp = Vector3.Lerp(blendedUp, centerUp, 0.5f);

            // Rotasi dasar mengikuti arah rel
            Quaternion splineRotation = Quaternion.LookRotation(trainDirection.normalized, finalUp);

            // Rotasi offset lokal 360 derajat (X, Y, Z)
            Quaternion offsetQuaternion = Quaternion.Euler(rotationOffset);

            // Gabungkan rotasi rel dengan rotasi offset 360 derajat
            centerPivot.rotation = splineRotation * offsetQuaternion;
        }
    }

    /// <summary>
    /// Mengambil Posisi, Tangent (Forward), dan Up Vector berdasarkan jarak akurat dalam meter.
    /// </summary>
    private Vector3 GetPointAtDistance(float distanceInMeters, out Vector3 forward, out Vector3 up)
    {
        float rawT = distanceInMeters / splineLength;

        // Mengonversi rasio linear menjadi normalized parameter t yang presisi mengikuti arc-length kurva
        float t = SplineUtility.GetNormalizedInterpolation(splineContainer.Spline, rawT, PathIndexUnit.Normalized);

        splineContainer.Evaluate(t, out var position, out var tangent, out var upVector);

        forward = Vector3.Normalize((Vector3)tangent);
        up = Vector3.Normalize((Vector3)upVector);

        return (Vector3)position;
    }
}