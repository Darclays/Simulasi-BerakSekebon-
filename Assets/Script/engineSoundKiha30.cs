using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class engineSoundKiha30 : MonoBehaviour
{
    [Header("Reference to Controller")]
    [Tooltip("Masukkan GameObject yang memiliki script keretaController2")]
    public keretaController2 trainController;

    [Header("Audio Horn (Klakson)")]
    [Tooltip("File audio klakson single")]
    public AudioClip hornClip;
    [Tooltip("AudioSource khusus klakson agar tidak memotong suara mesin (Opsional)")]
    public AudioSource hornAudioSource;

    [Header("Audio Idle Clips (Looping)")]
    public AudioClip idle1; // Kecepatan 0 km/h SAJA
    public AudioClip idle2; // Kecepatan 1 - 20 km/h (Coasting / Stagnan)
    public AudioClip idle3; // Kecepatan 20 - 40 km/h (Coasting / Stagnan)
    public AudioClip idle4; // Kecepatan 40 - 70 km/h (Coasting / Stagnan)
    public AudioClip idle5; // Kecepatan 70 - 100 km/h (Coasting / Stagnan)

    [Header("Audio Akselerasi Clips (One-Shot / Transisi)")]
    public AudioClip up1_2; // Transisi P1 (1-20 km/h)
    public AudioClip up2_3; // Transisi P2 (20-40 km/h)
    public AudioClip up3_4; // Transisi P3 (40-70 km/h)
    public AudioClip up4_5; // Transisi P4 (70-90 km/h)

    [Header("Audio Deselerasi Clips (One-Shot / Transisi)")]
    public AudioClip down5_4; // Transisi 100 -> 70 km/h
    public AudioClip down4_3; // Transisi 70 -> 40 km/h
    public AudioClip down3_2; // Transisi 40 -> 20 km/h
    public AudioClip down2_1; // Transisi 20 -> 1 km/h

    private AudioSource engineAudioSource;
    private float currentSpeed = 0f;
    private float previousSpeed = 0f;
    private int previousNotch = 0;

    private bool isTransitioning = false;
    private AudioClip targetIdleAfterTransition = null;

    void Awake()
    {
        engineAudioSource = GetComponent<AudioSource>();
        engineAudioSource.playOnAwake = false;
        engineAudioSource.loop = true;

        // Jika AudioSource khusus horn tidak di-assign di Inspector,
        // buat AudioSource komponen tambahan secara otomatis untuk klakson
        if (hornAudioSource == null)
        {
            hornAudioSource = gameObject.AddComponent<AudioSource>();
            hornAudioSource.playOnAwake = false;
            hornAudioSource.loop = false;
        }
    }

    void Start()
    {
        if (trainController == null)
        {
            trainController = GetComponent<keretaController2>();
        }

        // Jalankan audio Idle 1 saat awal game / berhenti total
        PlayIdleSound(idle1);
    }

    void Update()
    {
        // --- 0. Input Klakson (Tombol H) ---
        HandleHornInput();

        if (trainController == null)
        {
            Debug.LogWarning("keretaController2 belum dipasang di engineSoundKiha30!");
            return;
        }

        // Ambil data kecepatan dan notch langsung dari controller
        currentSpeed = trainController.currentSpeed;
        int currentNotch = trainController.currentNotch;

        // 1. Handling Audio Transisi Mesin (Up / Down)
        if (isTransitioning)
        {
            // Jika klip transisi (Up/Down) selesai diputar, ganti ke Idle target
            if (!engineAudioSource.isPlaying)
            {
                isTransitioning = false;
                if (targetIdleAfterTransition != null)
                {
                    PlayIdleSound(targetIdleAfterTransition);
                    targetIdleAfterTransition = null;
                }
            }

            previousSpeed = currentSpeed;
            previousNotch = currentNotch;
            return;
        }

        // 2. Logika Berhenti Total (0 km/h)
        if (currentSpeed <= 0.05f)
        {
            PlayIdleSound(idle1);
        }
        // 3. Logika Akselerasi (Percepatan / Notch > 0)
        else if (currentNotch > 0 && currentSpeed > previousSpeed)
        {
            HandleAcceleration(currentNotch);
        }
        // 4. Logika Deselerasi (Perlambatan / Notch < 0)
        else if (currentNotch < 0 && currentSpeed < previousSpeed)
        {
            HandleDeceleration();
        }
        // 5. Logika Coasting / Stagnan (Notch N / 0, kecepatan konstan atau meluncur)
        else if (currentNotch == 0)
        {
            HandleCoastingIdle();
        }

        previousSpeed = currentSpeed;
        previousNotch = currentNotch;
    }

    private void HandleHornInput()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            if (hornClip != null)
            {
                // Putar klakson tanpa mengganggu audio mesin
                hornAudioSource.PlayOneShot(hornClip);
            }
            else
            {
                Debug.LogWarning("Audio clip Horn belum dimasukkan di Inspector!");
            }
        }
    }

    private void HandleAcceleration(int currentNotch)
    {
        // P1: Akselerasi 1 - 20 km/h
        if (currentNotch == 1 && currentSpeed >= 1f && currentSpeed < 20f)
        {
            if (previousNotch != 1 || engineAudioSource.clip != up1_2)
            {
                PlayTransitionSound(up1_2, idle2);
            }
        }
        // P2: Akselerasi 20 - 40 km/h
        else if (currentNotch == 2 && currentSpeed >= 20f && currentSpeed < 40f)
        {
            if (previousNotch != 2 || engineAudioSource.clip != up2_3)
            {
                PlayTransitionSound(up2_3, idle3);
            }
        }
        // P3: Akselerasi 40 - 70 km/h
        else if (currentNotch == 3 && currentSpeed >= 40f && currentSpeed < 70f)
        {
            if (previousNotch != 3 || engineAudioSource.clip != up3_4)
            {
                PlayTransitionSound(up3_4, idle4);
            }
        }
        // P4: Akselerasi 70 - 90 km/h
        else if (currentNotch == 4 && currentSpeed >= 70f && currentSpeed < 90f)
        {
            if (previousNotch != 4 || engineAudioSource.clip != up4_5)
            {
                PlayTransitionSound(up4_5, idle5);
            }
        }
        // Jika mencapai batas atas kecepatan tier tanpa transisi aktif, masuk ke Idle Tier tersebut
        else if (currentSpeed >= 90f && currentNotch == 4)
        {
            PlayIdleSound(idle5);
        }
    }

    private void HandleDeceleration()
    {
        // Deselerasi 100 -> 70 km/h
        if (previousSpeed >= 70f && currentSpeed < 100f && currentSpeed >= 70f)
        {
            if (engineAudioSource.clip != down5_4)
            {
                PlayTransitionSound(down5_4, idle4);
            }
        }
        // Deselerasi 70 -> 40 km/h
        else if (previousSpeed >= 40f && currentSpeed < 70f && currentSpeed >= 40f)
        {
            if (engineAudioSource.clip != down4_3)
            {
                PlayTransitionSound(down4_3, idle4);
            }
        }
        // Deselerasi 40 -> 20 km/h
        else if (previousSpeed >= 20f && currentSpeed < 40f && currentSpeed >= 20f)
        {
            if (engineAudioSource.clip != down3_2)
            {
                PlayTransitionSound(down3_2, idle3);
            }
        }
        // Deselerasi 20 -> 1 km/h
        else if (previousSpeed >= 1f && currentSpeed < 20f && currentSpeed >= 1f)
        {
            if (engineAudioSource.clip != down2_1)
            {
                PlayTransitionSound(down2_1, idle2);
            }
        }
    }

    private void HandleCoastingIdle()
    {
        // Menentukan idle mana yang harus diputar berdasarkan kecepatan saat ini di posisi Netral (N)
        if (currentSpeed >= 90f)
        {
            PlayIdleSound(idle5);
        }
        else if (currentSpeed >= 70f)
        {
            PlayIdleSound(idle4);
        }
        else if (currentSpeed >= 40f)
        {
            PlayIdleSound(idle3);
        }
        else if (currentSpeed >= 20f)
        {
            PlayIdleSound(idle2);
        }
        else if (currentSpeed > 0.05f)
        {
            PlayIdleSound(idle2);
        }
    }

    private void PlayIdleSound(AudioClip idleClip)
    {
        if (idleClip == null) return;

        if (engineAudioSource.clip != idleClip || !engineAudioSource.isPlaying)
        {
            engineAudioSource.Stop();
            engineAudioSource.clip = idleClip;
            engineAudioSource.loop = true;
            engineAudioSource.Play();
            isTransitioning = false;
        }
    }

    private void PlayTransitionSound(AudioClip transitionClip, AudioClip nextIdleClip)
    {
        if (transitionClip == null)
        {
            // Jika audio transisi tidak dimasukkan, langsung mainkan audio idle target
            PlayIdleSound(nextIdleClip);
            return;
        }

        engineAudioSource.Stop();
        engineAudioSource.clip = transitionClip;
        engineAudioSource.loop = false;
        engineAudioSource.Play();

        isTransitioning = true;
        targetIdleAfterTransition = nextIdleClip;
    }
}