using UnityEngine;
using UnityEngine.UI;
using TMPro; // Dibutuhkan untuk UI TextMeshPro

public class UIKeretaManager : MonoBehaviour
{
    [Header("Referensi Script Utama")]
    public keretaController2 trainController;
    public engineSoundKiha30 soundController;
    public PintuPenumpangController pintuController;

    [Header("Referensi UI Tombol (Kiri)")]
    public Button tombolKlakson;
    public Button tombolPintu;

    [Header("Referensi UI Indikator (Kanan)")]
    public TMP_Text teksSpeed;
    public TMP_Text teksNotch;

    private void Start()
    {
        // 1. Menyambungkan tombol UI ke fungsi klakson dan pintu
        if (tombolKlakson != null)
        {
            tombolKlakson.onClick.AddListener(BunyikanKlaksonViaUI);
        }
        else
        {
            Debug.LogWarning("[UIKeretaManager] Tombol Klakson belum di-assign di Inspector.");
        }
        
        if (tombolPintu != null)
        {
            tombolPintu.onClick.AddListener(TogglePintuViaUI);
        }
        else
        {
            Debug.LogWarning("[UIKeretaManager] Tombol Pintu belum di-assign di Inspector.");
        }

        // Peringatan jika referensi script utama kosong
        if (trainController == null) Debug.LogError("[UIKeretaManager] trainController belum di-assign!");
        if (soundController == null) Debug.LogError("[UIKeretaManager] soundController belum di-assign!");
        if (pintuController == null) Debug.LogError("[UIKeretaManager] pintuController belum di-assign!");
    }

    private void Update()
    {
        // 2. Mengambil data dari keretaController2 setiap frame dan menampilkannya ke UI
        if (trainController != null)
        {
            UpdateIndikatorSpeed(trainController.currentSpeed);
            UpdateIndikatorNotch(trainController.notchDisplay);
        }
    }

    // --- FUNGSI AKSI DARI TOMBOL UI ---

    private void BunyikanKlaksonViaUI()
    {
        if (soundController != null)
        {
            soundController.TriggerHornFromUI(); 
        }
    }

    private void TogglePintuViaUI()
    {
        if (pintuController != null)
        {
            pintuController.TriggerPintuFromUI();
        }
    }

    // --- FUNGSI UPDATE TEKS UI ---

    private void UpdateIndikatorSpeed(float speed)
    {
        if (teksSpeed != null)
        {
            // Membulatkan kecepatan ke bawah agar terlihat rapi (misal: 69.8 jadi 69)
            teksSpeed.text = Mathf.FloorToInt(speed).ToString();
        }
    }

    private void UpdateIndikatorNotch(string notch)
    {
        if (teksNotch != null)
        {
            teksNotch.text = notch;
        }
    }
}