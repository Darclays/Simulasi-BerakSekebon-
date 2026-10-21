using UnityEngine;
using Unity.Cinemachine; 
using System.Collections; 

public class TrainCameraShake : MonoBehaviour
{
    [Header("Pengaturan Kamera 1 (Contoh: MainMenu)")]
    public CinemachineCamera targetCamera1;
    public float shakeAmplitude1 = 2.0f; // Kekuatan getaran kamera 1
    public float shakeFrequency1 = 2.0f; // Kecepatan getaran kamera 1

    [Header("Pengaturan Kamera 2 (Contoh: AwalMula / Title)")]
    public CinemachineCamera targetCamera2;
    public float shakeAmplitude2 = 1.0f; // Kekuatan getaran kamera 2 (bisa diatur lebih pelan)
    public float shakeFrequency2 = 1.0f; // Kecepatan getaran kamera 2

    [Header("Pengaturan Transisi")]
    public float fadeOutTime = 1.5f;    // Waktu getaran memudar setelah kereta lewat

    // Komponen Noise bawaan Cinemachine 3 untuk masing-masing kamera
    private CinemachineBasicMultiChannelPerlin noise1;
    private CinemachineBasicMultiChannelPerlin noise2;

    private void Start()
    {
        // Mengambil komponen Noise dari Kamera 1
        if (targetCamera1 != null)
        {
            noise1 = targetCamera1.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise1 != null)
            {
                noise1.AmplitudeGain = 0f;
                noise1.FrequencyGain = 0f;
            }
        }

        // Mengambil komponen Noise dari Kamera 2
        if (targetCamera2 != null)
        {
            noise2 = targetCamera2.GetComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise2 != null)
            {
                noise2.AmplitudeGain = 0f;
                noise2.FrequencyGain = 0f;
            }
        }
    }

    // Fungsi yang terpanggil otomatis saat objek menabrak/memasuki Trigger
    private void OnTriggerEnter(Collider other)
    {
        // Pastikan yang menabrak adalah kereta
        if (other.CompareTag("Train"))
        {
            StopAllCoroutines(); // Hentikan proses meredakan getaran jika masih berjalan
            
            // Aktifkan getaran Kamera 1
            if (noise1 != null)
            {
                noise1.AmplitudeGain = shakeAmplitude1;
                noise1.FrequencyGain = shakeFrequency1;
            }
            
            // Aktifkan getaran Kamera 2
            if (noise2 != null)
            {
                noise2.AmplitudeGain = shakeAmplitude2;
                noise2.FrequencyGain = shakeFrequency2;
            }
        }
    }

    // Fungsi yang terpanggil saat objek keluar dari Trigger
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Train"))
        {
            // Mulai proses memudarkan getaran perlahan untuk kedua kamera
            StartCoroutine(FadeOutShake());
        }
    }

    // Coroutine untuk menurunkan getaran secara halus ke 0
    private IEnumerator FadeOutShake()
    {
        // Simpan nilai getaran terakhir saat kereta keluar dari trigger
        float startAmp1 = noise1 != null ? noise1.AmplitudeGain : 0f;
        float startAmp2 = noise2 != null ? noise2.AmplitudeGain : 0f;
        
        float elapsedTime = 0f;

        while (elapsedTime < fadeOutTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeOutTime; // Persentase waktu (0.0 sampai 1.0)
            
            // Lerp (Linear Interpolation) menurunkan angka secara bertahap ke 0
            if (noise1 != null) noise1.AmplitudeGain = Mathf.Lerp(startAmp1, 0f, t);
            if (noise2 != null) noise2.AmplitudeGain = Mathf.Lerp(startAmp2, 0f, t);
            
            yield return null; // Tunggu frame selanjutnya
        }

        // Pastikan benar-benar kembali ke 0 agar tidak ada sisa getaran
        if (noise1 != null)
        {
            noise1.AmplitudeGain = 0f;
            noise1.FrequencyGain = 0f;
        }
        if (noise2 != null)
        {
            noise2.AmplitudeGain = 0f;
            noise2.FrequencyGain = 0f;
        }
    }
}