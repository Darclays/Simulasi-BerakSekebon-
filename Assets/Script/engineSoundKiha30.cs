using UnityEngine;
using System.Collections; 

[RequireComponent(typeof(AudioSource))]
public class engineSoundKiha30 : MonoBehaviour
{
    [Header("Reference to Controller")]
    public keretaController2 trainController;

    [Header("Animator Reference (Opsional untuk Bodi Kereta)")]
    public Animator trainAnimator;
    public string hornAnimatorParam = "PlayHorn";

    [Header("Audio Horn (Klakson)")]
    public AudioClip hornClip;
    public AudioSource hornAudioSource;

    [Header("Audio Idle Clips (Looping)")]
    public AudioClip idle1; 
    public AudioClip idle2; 
    public AudioClip idle3; 
    public AudioClip idle4; 
    public AudioClip idle5; 

    [Header("Audio Akselerasi Clips (One-Shot / Transisi)")]
    public AudioClip up1_2; 
    public AudioClip up2_3; 
    public AudioClip up3_4; 
    public AudioClip up4_5; 

    [Header("Audio Deselerasi Clips (One-Shot / Transisi)")]
    public AudioClip down5_4; 
    public AudioClip down4_3; 
    public AudioClip down3_2; 
    public AudioClip down2_1; 

    private AudioSource engineAudioSource;
    private float currentSpeed = 0f;
    private float previousSpeed = 0f;
    private int previousNotch = 0;

    private bool isTransitioning = false;
    private AudioClip targetIdleAfterTransition = null;
    private bool isHornPlaying = false; 

    void Awake()
    {
        engineAudioSource = GetComponent<AudioSource>();
        engineAudioSource.playOnAwake = false;
        engineAudioSource.loop = true;

        if (hornAudioSource == null)
        {
            hornAudioSource = gameObject.AddComponent<AudioSource>();
            hornAudioSource.playOnAwake = false;
            hornAudioSource.loop = false;
        }

        // PENGAMAN UTAMA: Mencegah script klakson mengendalikan Animator Pintu
        // Jika Anda ingin animasi bodi kereta aktif saat klakson, pastikan trainAnimator 
        // menempel di objek bodi kereta, BUKAN di objek pintu. 
        // Jika tidak punya animator bodi, biarkan baris di bawah aktif untuk keamanan.
        // trainAnimator = null; // Hapus komentar (//) di baris ini jika ingin benar-benar menonaktifkan animator klakson.
    }

    void Start()
    {
        if (trainController == null)
        {
            trainController = GetComponent<keretaController2>();
        }

        PlayIdleSound(idle1);
    }

    void Update()
    {
        HandleHornInput();

        if (trainController == null) return;

        currentSpeed = trainController.currentSpeed;
        int currentNotch = trainController.currentNotch;

        if (isTransitioning)
        {
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

        if (currentSpeed <= 0.05f) PlayIdleSound(idle1);
        else if (currentNotch > 0 && currentSpeed > previousSpeed) HandleAcceleration(currentNotch);
        else if (currentNotch < 0 && currentSpeed < previousSpeed) HandleDeceleration();
        else if (currentNotch == 0) HandleCoastingIdle();

        previousSpeed = currentSpeed;
        previousNotch = currentNotch;
    }

    private void HandleHornInput()
    {
        if (Input.GetKeyDown(KeyCode.H) && !isHornPlaying)
        {
            if (hornClip != null) StartCoroutine(PlayHornRoutine());
        }
    }

    private IEnumerator PlayHornRoutine()
    {
        isHornPlaying = true;
        
        if (hornAudioSource != null && hornClip != null)
        {
            hornAudioSource.PlayOneShot(hornClip);
        }

        // Hanya jalankan animator jika referensinya benar-benar ada dan valid
        if (trainAnimator != null) 
        {
            trainAnimator.SetTrigger(hornAnimatorParam);
        }

        yield return new WaitForSeconds(hornClip != null ? hornClip.length : 1f);
        isHornPlaying = false;
    }

    // --- FUNGSI UNTUK UI MANAGER ---
    public void TriggerHornFromUI()
    {
        if (!isHornPlaying)
        {
            if (hornClip != null) StartCoroutine(PlayHornRoutine());
            else Debug.LogWarning("Audio clip Horn belum dimasukkan di Inspector!");
        }
    }

    private void HandleAcceleration(int currentNotch)
    {
        if (currentNotch == 1 && currentSpeed >= 1f && currentSpeed < 20f)
        {
            if (previousNotch != 1 || engineAudioSource.clip != up1_2) PlayTransitionSound(up1_2, idle2);
        }
        else if (currentNotch == 2 && currentSpeed >= 20f && currentSpeed < 40f)
        {
            if (previousNotch != 2 || engineAudioSource.clip != up2_3) PlayTransitionSound(up2_3, idle3);
        }
        else if (currentNotch == 3 && currentSpeed >= 40f && currentSpeed < 70f)
        {
            if (previousNotch != 3 || engineAudioSource.clip != up3_4) PlayTransitionSound(up3_4, idle4);
        }
        else if (currentNotch == 4 && currentSpeed >= 70f && currentSpeed < 90f)
        {
            if (previousNotch != 4 || engineAudioSource.clip != up4_5) PlayTransitionSound(up4_5, idle5);
        }
        else if (currentSpeed >= 90f && currentNotch == 4) PlayIdleSound(idle5);
    }

    private void HandleDeceleration()
    {
        if (previousSpeed >= 70f && currentSpeed < 100f && currentSpeed >= 70f)
        {
            if (engineAudioSource.clip != down5_4) PlayTransitionSound(down5_4, idle4);
        }
        else if (previousSpeed >= 40f && currentSpeed < 70f && currentSpeed >= 40f)
        {
            if (engineAudioSource.clip != down4_3) PlayTransitionSound(down4_3, idle4);
        }
        else if (previousSpeed >= 20f && currentSpeed < 40f && currentSpeed >= 20f)
        {
            if (previousNotch != 2 || engineAudioSource.clip != down3_2) PlayTransitionSound(down3_2, idle3);
        }
        else if (previousSpeed >= 1f && currentSpeed < 20f && currentSpeed >= 1f)
        {
            if (previousNotch != 1 || engineAudioSource.clip != down2_1) PlayTransitionSound(down2_1, idle2);
        }
    }

    private void HandleCoastingIdle()
    {
        if (currentSpeed >= 90f) PlayIdleSound(idle5);
        else if (currentSpeed >= 70f) PlayIdleSound(idle4);
        else if (currentSpeed >= 40f) PlayIdleSound(idle3);
        else if (currentSpeed >= 20f) PlayIdleSound(idle2);
        else if (currentSpeed > 0.05f) PlayIdleSound(idle2);
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