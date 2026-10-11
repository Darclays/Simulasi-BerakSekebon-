using UnityEngine;
using Unity.Cinemachine; 
using System.Collections; // Dibutuhkan untuk Coroutine (mengurangi getaran perlahan)

public class TrainCameraShake : MonoBehaviour
{
    [Header("Referensi Kamera")]
    [Tooltip("Masukkan Cam_MainMenu ke sini")]
    public CinemachineCamera targetCamera;

    [Header("Pengaturan Getaran (Shake)")]
    public float shakeAmplitude = 2.0f; // Kekuatan getaran
    public float shakeFrequency = 2.0f; // Kecepatan getaran
    public float fadeOutTime = 1.5f;    // Waktu getaran memudar setelah kereta lewat

    // Komponen Noise bawaan Cinemachine 3
    private CinemachineBasicMultiChannelPerlin cameraNoise;

    private void Start()
    {
        // Mengambil komponen Noise dari CinemachineCamera
        if (targetCamera != null)
        {
            cameraNoise = targetCamera.GetComponent<CinemachineBasicMultiChannelPerlin>();
            
            // Pastikan getaran mati saat mulai
            if (cameraNoise != null)
            {
                cameraNoise.AmplitudeGain = 0f;
                cameraNoise.FrequencyGain = 0f;
            }
        }
    }

    // Fungsi yang terpanggil otomatis saat objek menabrak/memasuki Trigger
    private void OnTriggerEnter(Collider other)
    {
        // Pastikan yang menabrak adalah kereta (beri Tag "Train" pada objek keretamu)
        if (other.CompareTag("Train") && cameraNoise != null)
        {
            StopAllCoroutines(); // Hentikan proses meredakan getaran jika masih berjalan
            cameraNoise.AmplitudeGain = shakeAmplitude;
            cameraNoise.FrequencyGain = shakeFrequency;
        }
    }

    // Fungsi yang terpanggil saat objek keluar dari Trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Train") && cameraNoise != null)
        {
            // Mulai proses memudarkan getaran perlahan
            StartCoroutine(FadeOutShake());
        }
    }

    // Coroutine untuk menurunkan getaran secara halus ke 0
    private IEnumerator FadeOutShake()
    {
        float currentAmplitude = cameraNoise.AmplitudeGain;
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            
            // Lerp (Linear Interpolation) menurunkan angka secara bertahap
            cameraNoise.AmplitudeGain = Mathf.Lerp(currentAmplitude, 0f, elapsedTime / fadeOutTime);
            
            yield return null; // Tunggu frame selanjutnya
        }

        // Pastikan benar-benar kembali ke 0
        cameraNoise.AmplitudeGain = 0f;
        cameraNoise.FrequencyGain = 0f;
    }
}