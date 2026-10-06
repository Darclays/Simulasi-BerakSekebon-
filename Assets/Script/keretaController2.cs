using UnityEngine;
using UnityEngine.Splines;

public class keretaController2 : MonoBehaviour
{
    [Header("Spline References")]
    public SplineContainer splineContainer;

    [Header("Center Pivot Reference")]
    [Tooltip("Pivot tengah (Empty GameObject) yang akan memegang Mesh Kereta sebagai Child")]
    public Transform centerPivot;

    [Header("Transform Offset Settings")]
    [Tooltip("Offset posisi lokal kereta terhadap rel (X: Kiri/Kanan, Y: Ketinggian/Atas-Bawah, Z: Maju/Mundur)")]
    public Vector3 positionOffset = Vector3.zero;

    [Tooltip("Posisi awal start kereta pada jalur spline (dalam meter)")]
    public float startDistance = 0f;

    [Header("Train Physical Settings")]
    [Tooltip("Jarak nyata dari roda/bogie depan ke roda/bogie belakang dalam meter")]
    public float wheelBaseLength = 4f;

    [Header("Rotation Settings (360 Degree Offsets)")]
    [Tooltip("Rotasi offset tambahan dalam derajat (X: Pitch, Y: Yaw/Muter 360, Z: Roll/Miring)")]
    public Vector3 rotationOffset = Vector3.zero;

    [Tooltip("Kecepatan rotasi manual (Yaw) menggunakan tombol Period / Comma")]
    public float rotationSpeed = 90f;

    [Header("Train Status (Notch System)")]
    [Tooltip("Kecepatan saat ini dalam km/jam")]
    public float currentSpeed = 0f;
    [Tooltip("Kecepatan maksimal kereta dalam km/jam")]
    public float maxSpeed = 110f;

    public int currentNotch = 0;
    [Tooltip("String untuk dikirim ke UI Text (N, P1-P4, B1-B6)")]
    public string notchDisplay = "N";

    [Header("Notch Settings")]
    public int maxPowerNotch = 4;
    public int maxBrakeNotch = 6;

    public float maxAccelerationForce = 15f;
    public float maxBrakeForce = 25f;
    public float naturalFriction = 2f;

    [Header("Notch Percentages (0.0 - 1.0)")]
    public float[] powerPercentages = { 0.25f, 0.50f, 0.75f, 1.00f };
    public float[] brakePercentages = { 0.15f, 0.30f, 0.45f, 0.60f, 0.80f, 1.00f };

    private float distanceTraveled = 0f;
    private float splineLength = 100f;

    void Start()
    {
        if (centerPivot == null)
        {
            centerPivot = transform;
        }

        UpdateSplineLength();
        distanceTraveled = startDistance;
    }

    void Update()
    {
        if (splineContainer == null || splineContainer.Spline == null) return;

        UpdateSplineLength();
        if (splineLength <= 0f) return;

        // 1. Input Notch & Kalkulasi Kecepatan
        HandleNotchInput();
        CalculateTrainSpeed();
        UpdateNotchDisplay();

        // 2. Konversi Kecepatan dari km/h ke m/s
        float speedInMetersPerSecond = currentSpeed / 3.6f;
        distanceTraveled += speedInMetersPerSecond * Time.deltaTime;

        // Penanganan jalur melingkar (Loop) vs jalur terbuka
        bool isClosed = splineContainer.Spline.Closed;
        if (isClosed)
        {
            distanceTraveled = Mathf.Repeat(distanceTraveled, splineLength);
        }
        else
        {
            distanceTraveled = Mathf.Clamp(distanceTraveled, 0f, splineLength);
        }

        // 3. Kontrol Input Rotasi Manual 360 Derajat
        HandleManualRotationInput();

        // 4. Update Posisi & Rotasi Kereta pada Spline
        UpdateTrainTransformOnSpline();
    }

    private void UpdateSplineLength()
    {
        if (splineContainer != null)
        {
            splineLength = splineContainer.CalculateLength();
        }
    }

    private void HandleNotchInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (currentNotch < maxPowerNotch)
            {
                currentNotch++;
            }
        }

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
        if (currentNotch > 0)
        {
            float powerMultiplier = powerPercentages[currentNotch - 1];
            float acceleration = maxAccelerationForce * powerMultiplier;
            currentSpeed += acceleration * Time.deltaTime;
        }
        else if (currentNotch < 0)
        {
            int brakeIndex = Mathf.Abs(currentNotch) - 1;
            float brakeMultiplier = brakePercentages[brakeIndex];
            float deceleration = maxBrakeForce * brakeMultiplier;
            currentSpeed -= deceleration * Time.deltaTime;
        }
        else
        {
            currentSpeed -= naturalFriction * Time.deltaTime;
        }

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
        if (Input.GetKey(KeyCode.Period))
        {
            rotationOffset.y -= rotationSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.Comma))
        {
            rotationOffset.y += rotationSpeed * Time.deltaTime;
        }

        rotationOffset.y = Mathf.Repeat(rotationOffset.y, 360f);
    }

    private void UpdateTrainTransformOnSpline()
    {
        bool isClosed = splineContainer.Spline.Closed;
        float halfWheelBase = wheelBaseLength * 0.5f;

        float frontDistance = distanceTraveled + halfWheelBase;
        float rearDistance = distanceTraveled - halfWheelBase;

        // Batasi batas jarak sesuai tipe spline (Loop vs Non-Loop)
        if (isClosed)
        {
            frontDistance = Mathf.Repeat(frontDistance, splineLength);
            rearDistance = Mathf.Repeat(rearDistance, splineLength);
        }
        else
        {
            frontDistance = Mathf.Clamp(frontDistance, 0f, splineLength);
            rearDistance = Mathf.Clamp(rearDistance, 0f, splineLength);
        }

        // Evaluasi Titik Tengah (Center Pivot), Roda Depan, dan Roda Belakang
        Vector3 centerPos = GetPointAtDistance(distanceTraveled, out Vector3 centerForward, out Vector3 centerUp);
        Vector3 frontPos = GetPointAtDistance(frontDistance, out _, out Vector3 frontUp);
        Vector3 rearPos = GetPointAtDistance(rearDistance, out _, out Vector3 rearUp);

        // Arah orientasi kereta berdasarkan posisi roda depan dan belakang
        Vector3 trainDirection = frontPos - rearPos;

        // Validasi agar vektor tidak berbalik arah 180 derajat atau menjadi nol
        if (trainDirection.sqrMagnitude < 0.001f || Vector3.Dot(trainDirection.normalized, centerForward) < 0f)
        {
            trainDirection = centerForward;
        }
        else
        {
            trainDirection.Normalize();
        }

        Vector3 blendedUp = Vector3.Lerp(rearUp, frontUp, 0.5f);
        Vector3 finalUp = Vector3.Lerp(blendedUp, centerUp, 0.5f);

        // Mencegah Gimbal Lock / Vector Up yang sejajar dengan arah jalan
        if (finalUp.sqrMagnitude < 0.001f || Mathf.Abs(Vector3.Dot(trainDirection, finalUp.normalized)) > 0.99f)
        {
            finalUp = Vector3.up;
        }

        Quaternion splineRotation = Quaternion.LookRotation(trainDirection, finalUp);
        Quaternion offsetQuaternion = Quaternion.Euler(rotationOffset);
        Quaternion finalRotation = splineRotation * offsetQuaternion;

        // Terpakan rotasi akhir
        centerPivot.rotation = finalRotation;

        // Terpakan posisi spline dasar + offset posisi lokal (dirotasi mengikuti arah rel)
        centerPivot.position = centerPos + (finalRotation * positionOffset);
    }

    private Vector3 GetPointAtDistance(float distanceInMeters, out Vector3 forward, out Vector3 up)
    {
        // Konversi jarak m ke interpolasi t secara langsung menggunakan PathIndexUnit.Distance
        float t = SplineUtility.GetNormalizedInterpolation(splineContainer.Spline, distanceInMeters, PathIndexUnit.Distance);

        splineContainer.Evaluate(t, out var position, out var tangent, out var upVector);

        forward = Vector3.Normalize((Vector3)tangent);
        up = Vector3.Normalize((Vector3)upVector);

        return (Vector3)position;
    }
}