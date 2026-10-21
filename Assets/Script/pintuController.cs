using System.Collections;
using UnityEngine;

public class PintuPenumpangController : MonoBehaviour
{
    [Header("Referensi Controller Kereta")]
    public keretaController2 keretaController;

    [Header("Pengaturan Animasi & Indikator Pintu KANAN")]
    public Animator pintuKananAnimator;
    public GameObject indikatorPintuKananTerbuka;
    public GameObject indikatorPintuKananTertutup;

    [Header("Pengaturan Animasi & Indikator Pintu KIRI")]
    public Animator pintuKiriAnimator;
    public GameObject indikatorPintuKiriTerbuka;
    public GameObject indikatorPintuKiriTertutup;

    [Header("Pengaturan UI Stasiun Akhir")]
    [Tooltip("Drag GameObject UI yang ingin dinyalakan setelah pintu tertutup di stasiun akhir")]
    public GameObject uiStasiunEnd;

    [Header("Pengaturan Input & Waktu")]
    public KeyCode tombolPintu = KeyCode.T;
    public float durasiGerakPintu = 7f;
    public float durasiProsesPenumpang = 10f;

    [Header("Pengaturan Deteksi Area Peron Manual")]
    [Tooltip("Ukuran kotak deteksi (X: Lebar, Y: Tinggi, Z: Panjang)")]
    public Vector3 ukuranKotakDeteksi = new Vector3(3f, 3f, 15f);
    
    [Tooltip("Offset posisi pusat kotak deteksi relatif terhadap kereta")]
    public Vector3 offsetKotakDeteksi = Vector3.zero;

    [Tooltip("LayerMask tempat Box Collider Stasiun/Peron berada")]
    public LayerMask layerStasiun = ~0; // Default: Semua Layer

    [Header("Status Sistem")]
    public bool beradaDiAreaTriggerKanan = false;
    public bool beradaDiAreaEndKanan = false;
    public bool beradaDiAreaTriggerKiri = false;
    public bool beradaDiAreaEndKiri = false;

    public bool pintuTerbuka = false;
    public bool sedangDalamProses = false;

    // Menyimpan sisi pintu mana yang sedang aktif dibuka ("Kanan" atau "Kiri")
    private string sisiPintuAktif = "";
    private bool isEndStationActive = false;

    void Start()
    {
        if (keretaController == null)
        {
            keretaController = GetComponentInParent<keretaController2>();
        }

        if (keretaController == null)
        {
            Debug.LogError("[PintuController] ERROR: keretaController2 tidak ditemukan di Parent!");
        }

        if (uiStasiunEnd != null && uiStasiunEnd.activeSelf)
        {
            uiStasiunEnd.SetActive(false);
        }

        AturIndikatorAwal();
    }

    void Update()
    {
        CekAreaPeronManual();

        bool diAreaPeron = beradaDiAreaTriggerKanan || beradaDiAreaEndKanan || beradaDiAreaTriggerKiri || beradaDiAreaEndKiri;

        // Eksekusi Buka/Tutup Pintu dengan Tombol T
        if (diAreaPeron && !sedangDalamProses && Input.GetKeyDown(tombolPintu))
        {
            if (keretaController != null && keretaController.currentSpeed <= 0.001f)
            {
                if (!pintuTerbuka)
                {
                    // Tentukan sisi pintu yang akan dibuka berdasarkan area peron
                    if (beradaDiAreaTriggerKanan || beradaDiAreaEndKanan)
                    {
                        sisiPintuAktif = "Kanan";
                        isEndStationActive = beradaDiAreaEndKanan;
                    }
                    else if (beradaDiAreaTriggerKiri || beradaDiAreaEndKiri)
                    {
                        sisiPintuAktif = "Kiri";
                        isEndStationActive = beradaDiAreaEndKiri;
                    }

                    StartCoroutine(ProsesBukaDanPenumpang());
                }
                else
                {
                    StartCoroutine(ProsesTutupPintu());
                }
            }
            else
            {
                float speed = (keretaController != null) ? keretaController.currentSpeed : 0f;
                Debug.LogWarning($"[PintuController] Gagal! Kereta masih bergerak ({speed:F1} km/jam). Berhentikan kereta sepenuhnya!");
            }
        }
    }

    private void AturIndikatorAwal()
    {
        // Kanan
        if (indikatorPintuKananTerbuka != null) indikatorPintuKananTerbuka.SetActive(false);
        if (indikatorPintuKananTertutup != null) indikatorPintuKananTertutup.SetActive(true);

        // Kiri
        if (indikatorPintuKiriTerbuka != null) indikatorPintuKiriTerbuka.SetActive(false);
        if (indikatorPintuKiriTertutup != null) indikatorPintuKiriTertutup.SetActive(true);
    }

    private void CekAreaPeronManual()
    {
        Vector3 center = transform.TransformPoint(offsetKotakDeteksi);
        Vector3 halfExtents = ukuranKotakDeteksi * 0.5f;

        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, transform.rotation, layerStasiun);

        bool terdeteksiBiasaKanan = false;
        bool terdeteksiEndKanan = false;
        bool terdeteksiBiasaKiri = false;
        bool terdeteksiEndKiri = false;

        foreach (var col in hitColliders)
        {
            if (col.gameObject != gameObject)
            {
                string tagObjek = col.tag;

                // Peron Kanan
                if (tagObjek == "AreaStasiun")
                {
                    terdeteksiBiasaKanan = true;
                }
                else if (tagObjek == "AreaStasiunEnd" || col.name == "AreaStasiunEnd")
                {
                    terdeteksiEndKanan = true;
                }
                // Peron Kiri
                else if (tagObjek == "AreaStasiunKiri")
                {
                    terdeteksiBiasaKiri = true;
                }
                else if (tagObjek == "AreaStasiunEndKiri" || col.name == "AreaStasiunEndKiri")
                {
                    terdeteksiEndKiri = true;
                }
            }
        }

        // --- UPDATE STATUS & DEBUG LOG MASUK/KELUAR AREA ---

        // 1. Peron Kanan Biasa
        if (terdeteksiBiasaKanan && !beradaDiAreaTriggerKanan)
        {
            beradaDiAreaTriggerKanan = true;
            Debug.Log("[PintuController] >>> BERHASIL DETEKSI PERON KANAN! Siap tekan 'T' jika kereta berhenti. <<<");
        }
        else if (!terdeteksiBiasaKanan && beradaDiAreaTriggerKanan)
        {
            beradaDiAreaTriggerKanan = false;
            Debug.Log("[PintuController] <<< Keluar dari area Peron Kanan. >>>");
        }

        // 2. Stasiun Akhir Kanan
        if (terdeteksiEndKanan && !beradaDiAreaEndKanan)
        {
            beradaDiAreaEndKanan = true;
            Debug.Log("[PintuController] >>> BERHASIL DETEKSI STASIUN AKHIR KANAN! Siap tekan 'T' jika kereta berhenti. <<<");
        }
        else if (!terdeteksiEndKanan && beradaDiAreaEndKanan)
        {
            beradaDiAreaEndKanan = false;
            Debug.Log("[PintuController] <<< Keluar dari area Stasiun Akhir Kanan. >>>");
        }

        // 3. Peron Kiri Biasa
        if (terdeteksiBiasaKiri && !beradaDiAreaTriggerKiri)
        {
            beradaDiAreaTriggerKiri = true;
            Debug.Log("[PintuController] >>> BERHASIL DETEKSI PERON KIRI! Siap tekan 'T' jika kereta berhenti. <<<");
        }
        else if (!terdeteksiBiasaKiri && beradaDiAreaTriggerKiri)
        {
            beradaDiAreaTriggerKiri = false;
            Debug.Log("[PintuController] <<< Keluar dari area Peron Kiri. >>>");
        }

        // 4. Stasiun Akhir Kiri
        if (terdeteksiEndKiri && !beradaDiAreaEndKiri)
        {
            beradaDiAreaEndKiri = true;
            Debug.Log("[PintuController] >>> BERHASIL DETEKSI STASIUN AKHIR KIRI! Siap tekan 'T' jika kereta berhenti. <<<");
        }
        else if (!terdeteksiEndKiri && beradaDiAreaEndKiri)
        {
            beradaDiAreaEndKiri = false;
            Debug.Log("[PintuController] <<< Keluar dari area Stasiun Akhir Kiri. >>>");
        }
    }

    private IEnumerator ProsesBukaDanPenumpang()
    {
        sedangDalamProses = true;

        if (keretaController != null)
        {
            keretaController.enabled = false;
            Debug.Log("[PintuController] KONTROL KERETA DIMATIKAN.");
        }

        // Aktifkan Indikator & Animasi Sesuai Sisi Pintu yang Dipilih
        if (sisiPintuAktif == "Kanan")
        {
            if (indikatorPintuKananTerbuka != null) indikatorPintuKananTerbuka.SetActive(true);
            if (indikatorPintuKananTertutup != null) indikatorPintuKananTertutup.SetActive(false);

            if (pintuKananAnimator != null)
            {
                pintuKananAnimator.SetTrigger("BukaPintuKanan");
                Debug.Log("[PintuController] Animasi Buka Pintu Kanan Dijalankan.");
            }
        }
        else if (sisiPintuAktif == "Kiri")
        {
            if (indikatorPintuKiriTerbuka != null) indikatorPintuKiriTerbuka.SetActive(true);
            if (indikatorPintuKiriTertutup != null) indikatorPintuKiriTertutup.SetActive(false);

            if (pintuKiriAnimator != null)
            {
                pintuKiriAnimator.SetTrigger("BukaPintuKiri");
                Debug.Log("[PintuController] Animasi Buka Pintu Kiri Dijalankan.");
            }
        }

        // Countdown Durasi Gerak Pintu (7 detik)
        float sisaBuka = durasiGerakPintu;
        while (sisaBuka > 0)
        {
            Debug.Log($"[PintuController] Membuka Pintu {sisiPintuAktif}... Sisa waktu: {Mathf.CeilToInt(sisaBuka)} detik.");
            yield return new WaitForSeconds(1f);
            sisaBuka -= 1f;
        }

        pintuTerbuka = true;
        Debug.Log($"[PintuController] Pintu {sisiPintuAktif} BERHASIL DIBUKA! Memulai proses naik-turun penumpang...");

        // Countdown Proses Penumpang (10 detik)
        float sisaPenumpang = durasiProsesPenumpang;
        while (sisaPenumpang > 0)
        {
            Debug.Log($"[PintuController] Proses Naik-Turun Penumpang... Tombol T terkunci: {Mathf.CeilToInt(sisaPenumpang)} detik.");
            yield return new WaitForSeconds(1f);
            sisaPenumpang -= 1f;
        }

        sedangDalamProses = false;
        Debug.Log("[PintuController] Proses penumpang selesai. Silakan tekan 'T' lagi untuk MENUTUP pintu.");
    }

    private IEnumerator ProsesTutupPintu()
    {
        sedangDalamProses = true;
        Debug.Log($"[PintuController] Memulai proses MENUTUP pintu {sisiPintuAktif}...");

        // Matikan Indikator Terbuka & Nyalakan Indikator Tertutup, Serta Jalankan Animasi Tutup
        if (sisiPintuAktif == "Kanan")
        {
            if (indikatorPintuKananTerbuka != null) indikatorPintuKananTerbuka.SetActive(false);
            if (indikatorPintuKananTertutup != null) indikatorPintuKananTertutup.SetActive(true);

            if (pintuKananAnimator != null)
            {
                pintuKananAnimator.SetTrigger("TutupPintuKanan");
                Debug.Log("[PintuController] Animasi Tutup Pintu Kanan Dijalankan.");
            }
        }
        else if (sisiPintuAktif == "Kiri")
        {
            if (indikatorPintuKiriTerbuka != null) indikatorPintuKiriTerbuka.SetActive(false);
            if (indikatorPintuKiriTertutup != null) indikatorPintuKiriTertutup.SetActive(true);

            if (pintuKiriAnimator != null)
            {
                pintuKiriAnimator.SetTrigger("TutupPintuKiri");
                Debug.Log("[PintuController] Animasi Tutup Pintu Kiri Dijalankan.");
            }
        }

        // Countdown Durasi Gerak Tutup Pintu (7 detik)
        float sisaTutup = durasiGerakPintu;
        while (sisaTutup > 0)
        {
            Debug.Log($"[PintuController] Menutup Pintu {sisiPintuAktif}... Sisa waktu: {Mathf.CeilToInt(sisaTutup)} detik.");
            yield return new WaitForSeconds(1f);
            sisaTutup -= 1f;
        }

        pintuTerbuka = false;
        Debug.Log($"[PintuController] Pintu {sisiPintuAktif} BERHASIL DITUTUP SEPENUHNYA!");

        if (keretaController != null)
        {
            keretaController.enabled = true;
            Debug.Log("[PintuController] KONTROL KERETA DIAKTIFKAN KEMBALI.");
        }

        // KHUSUS STASIUN AKHIR: Nyalakan UI setelah pintu tertutup
        if (isEndStationActive && uiStasiunEnd != null)
        {
            uiStasiunEnd.SetActive(true);
            Debug.Log("[PintuController] UI Stasiun End BERHASIL diaktifkan!");
        }

        sedangDalamProses = false;
    }

    private void OnDrawGizmosSelected()
    {
        bool adaInteraksi = beradaDiAreaTriggerKanan || beradaDiAreaEndKanan || beradaDiAreaTriggerKiri || beradaDiAreaEndKiri;
        Gizmos.color = adaInteraksi ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.TransformPoint(offsetKotakDeteksi), transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawCube(Vector3.zero, ukuranKotakDeteksi);
        Gizmos.DrawWireCube(Vector3.zero, ukuranKotakDeteksi);
    }
}