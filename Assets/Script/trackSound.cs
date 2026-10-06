using UnityEngine;

public class TrackSound : MonoBehaviour
{
    [Header("Referensi Controller Kereta")]
    [Tooltip("Masukkan GameObject kereta. Jika kosong, script akan mencarinya secara otomatis.")]
    public keretaController2 trainController;

    [Header("Audio Clips - Hentakan Rel (Looping)")]
    public AudioClip idle1; // 1 - 5 km/h
    public AudioClip idle2; // 5 - 10 km/h
    public AudioClip idle3; // 10 - 13 km/h
    public AudioClip idle4; // 13 - 15 km/h
    public AudioClip idle5; // 15 - 20 km/h
    public AudioClip idle6; // 20 - 27 km/h
    public AudioClip idle7; // 27 - 30 km/h
    public AudioClip idle8; // 30 - 50 km/h
    public AudioClip idle9; // 50 - 100 km/h

    [Header("Audio Settings (Decibel)")]
    [Tooltip("Pengaturan volume dalam desibel (dB). Standard: 0 dB (100% volume original).")]
    [Range(-80f, 10f)]
    public float volumeInDB = 0f;

    [Header("3D Sound Settings")]
    [Tooltip("Jarak minimal (dalam meter) sebelum suara mulai mengecil saat kamera/listener menjauh.")]
    public float minDistance = 7.16f;

    [Tooltip("Jarak maksimal di mana suara tidak akan terdengar lagi sama sekali.")]
    public float maxDistance = 500f;

    [Tooltip("Mode penurunan volume berdasarkan jarak.")]
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

    private AudioSource trackAudioSource;
    private AudioClip currentClip;

    void Awake()
    {
        // Buat AudioSource khusus untuk TrackSound agar tidak berebutan dengan Engine / Horn
        trackAudioSource = gameObject.AddComponent<AudioSource>();
        trackAudioSource.playOnAwake = false;
        trackAudioSource.loop = true;
        trackAudioSource.spatialBlend = 1f; // Set ke 3D Sound (1.0 = murni 3D)
    }

    void Start()
    {
        if (trainController == null)
        {
            trainController = GetComponent<keretaController2>();
            if (trainController == null)
            {
                trainController = Object.FindFirstObjectByType<keretaController2>();
            }
        }

        // Inisialisasi pengaturan 3D Audio Source
        Update3DSoundSettings();
    }

    void Update()
    {
        if (trainController == null) return;

        // Sync volume desibel dan nilai 3D distance secara real-time dari Inspector
        trackAudioSource.volume = DecibelsToLinear(volumeInDB);
        Update3DSoundSettings();

        float speed = trainController.currentSpeed;
        AudioClip targetClip = SelectAudioClip(speed);

        // Hanya ganti clip jika tier kecepatan berubah
        if (targetClip != currentClip)
        {
            currentClip = targetClip;

            if (currentClip != null)
            {
                trackAudioSource.clip = currentClip;
                trackAudioSource.Play();
            }
            else
            {
                trackAudioSource.Stop(); // Hentikan audio jika kereta berhenti (0 km/h)
            }
        }
    }

    /// <summary>
    /// Mengaplikasikan nilai Min/Max Distance dan Rolloff Mode ke AudioSource
    /// </summary>
    private void Update3DSoundSettings()
    {
        if (trackAudioSource != null)
        {
            trackAudioSource.minDistance = minDistance;
            trackAudioSource.maxDistance = maxDistance;
            trackAudioSource.rolloffMode = rolloffMode;
        }
    }

    /// <summary>
    /// Mengonversi desibel (dB) ke nilai linear yang digunakan oleh AudioSource.volume
    /// </summary>
    private float DecibelsToLinear(float dB)
    {
        if (dB <= -80f) return 0f; // Mute jika terlalu rendah
        return Mathf.Pow(10f, dB / 20f);
    }

    private AudioClip SelectAudioClip(float speed)
    {
        if (speed > 0f && speed <= 5f) return idle1;
        if (speed > 5f && speed <= 10f) return idle2;
        if (speed > 10f && speed <= 13f) return idle3;
        if (speed > 13f && speed <= 15f) return idle4;
        if (speed > 15f && speed <= 20f) return idle5;
        if (speed > 20f && speed <= 27f) return idle6;
        if (speed > 27f && speed <= 30f) return idle7;
        if (speed > 30f && speed <= 50f) return idle8;
        if (speed > 50f && speed <= 100f) return idle9;
        if (speed > 100f) return idle9; // Menjaga batas atas > 100 km/h

        return null; // Kecepatan <= 0 km/h
    }
}