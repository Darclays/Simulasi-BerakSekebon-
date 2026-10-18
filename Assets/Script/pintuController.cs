using System.Collections;
using UnityEngine;

public class PintuPenumpangController : MonoBehaviour
{
    [Header("Referensi Controller Kereta")]
    public keretaController2 keretaController;

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
    public bool beradaDiAreaTrigger = false;
    public bool beradaDiAreaEnd = false; // Status khusus untuk stasiun akhir
    public bool pintuTerbuka = false;
    public bool sedangDalamProses = false;

    private bool isEndStationActive = false; // Penanda apakah saat ini berada di stasiun akhir

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

        // Pastikan UI dalam kondisi mati saat awal game
        if (uiStasiunEnd != null && uiStasiunEnd.activeSelf)
        {
            uiStasiunEnd.SetActive(false);
        }
    }

    void Update()
    {
        // 1. Deteksi peron secara fisik manual per frame
        CekAreaPeronManual();

        // Gabungan status area (stasiun biasa atau stasiun akhir)
        bool diAreaPeron = beradaDiAreaTrigger || beradaDiAreaEnd;

        // Debug tes tombol T
        if (Input.GetKeyDown(tombolPintu))
        {
            float speedSaatIni = (keretaController != null) ? keretaController.currentSpeed : -1f;
            Debug.Log($"[PintuController] Tombol T Ditekan! Status -> Di Area Peron: {diAreaPeron} (Biasa: {beradaDiAreaTrigger}, End: {beradaDiAreaEnd}), Sedang Proses: {sedangDalamProses}, Kecepatan: {speedSaatIni:F1} km/h");
        }

        // 2. Eksekusi Buka/Tutup Pintu (Berlaku untuk stasiun biasa maupun stasiun akhir dengan tombol T)
        if (diAreaPeron && !sedangDalamProses && Input.GetKeyDown(tombolPintu))
        {
            if (keretaController != null && keretaController.currentSpeed <= 0.001f)
            {
                if (!pintuTerbuka)
                {
                    // Catat apakah ini stasiun akhir saat tombol buka ditekan
                    isEndStationActive = beradaDiAreaEnd;
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

    private void CekAreaPeronManual()
    {
        Vector3 center = transform.TransformPoint(offsetKotakDeteksi);
        Vector3 halfExtents = ukuranKotakDeteksi * 0.5f;

        // Mencari semua collider di sekitar kotak deteksi
        Collider[] hitColliders = Physics.OverlapBox(center, halfExtents, transform.rotation, layerStasiun);

        bool terdeteksiBiasa = false;
        bool terdeteksiEnd = false;

        foreach (var col in hitColliders)
        {
            if (col.gameObject != gameObject)
            {
                // Cek apakah menyentuh peron biasa
                if (col.CompareTag("AreaStasiun"))
                {
                    terdeteksiBiasa = true;
                }
                // Cek apakah menyentuh peron akhir
                else if (col.CompareTag("AreaStasiunEnd") || col.name == "AreaStasiunEnd")
                {
                    terdeteksiEnd = true;
                }
            }
        }

        // Update status masuk/keluar area peron biasa
        if (terdeteksiBiasa && !beradaDiAreaTrigger)
        {
            beradaDiAreaTrigger = true;
            Debug.Log("[PintuController] >>> BERHASIL DETEKSI AREA PERON/STASIUN! Siap tekan 'T' jika kereta berhenti. <<<");
        }
        else if (!terdeteksiBiasa && beradaDiAreaTrigger)
        {
            beradaDiAreaTrigger = false;
            Debug.Log("[PintuController] <<< Keluar dari area Peron/Stasiun. >>>");
        }

        // Update status masuk/keluar area stasiun akhir
        if (terdeteksiEnd && !beradaDiAreaEnd)
        {
            beradaDiAreaEnd = true;
            Debug.Log("[PintuController] >>> BERHASIL DETEKSI STASIUN AKHIR (AreaStasiunEnd)! Siap tekan 'T' jika kereta berhenti. <<<");
        }
        else if (!terdeteksiEnd && beradaDiAreaEnd)
        {
            beradaDiAreaEnd = false;
            Debug.Log("[PintuController] <<< Keluar dari area Stasiun Akhir. >>>");
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

        // Countdown Animasi Membuka Pintu (7s)
        float sisaBuka = durasiGerakPintu;
        while (sisaBuka > 0)
        {
            Debug.Log($"[PintuController] Membuka Pintu Otomatis... Sisa waktu: {Mathf.CeilToInt(sisaBuka)} detik.");
            yield return new WaitForSeconds(1f);
            sisaBuka -= 1f;
        }

        pintuTerbuka = true;
        Debug.Log("[PintuController] Pintu BERHASIL DIBUKA! Memulai proses naik-turun penumpang...");

        // Countdown Cooldown Penumpang (10s)
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
        Debug.Log("[PintuController] Memulai proses MENUTUP pintu...");

        // Countdown Animasi Menutup Pintu (7s)
        float sisaTutup = durasiGerakPintu;
        while (sisaTutup > 0)
        {
            Debug.Log($"[PintuController] Menutup Pintu Otomatis... Sisa waktu: {Mathf.CeilToInt(sisaTutup)} detik.");
            yield return new WaitForSeconds(1f);
            sisaTutup -= 1f;
        }

        pintuTerbuka = false;
        Debug.Log("[PintuController] Pintu BERHASIL DITUTUP SEPENUHNYA!");

        if (keretaController != null)
        {
            keretaController.enabled = true;
            Debug.Log("[PintuController] KONTROL KERETA DIAKTIFKAN KEMBALI.");
        }

        // KHUSUS STASIUN AKHIR: Nyalakan UI setelah pintu tertutup sepenuhnya
        if (isEndStationActive && uiStasiunEnd != null)
        {
            uiStasiunEnd.SetActive(true);
            Debug.Log("[PintuController] UI Stasiun End BERHASIL diaktifkan (Nyala)!");
        }

        sedangDalamProses = false;
    }

    // Menampilkan kotak deteksi berwarna hijau/merah transparan di Editor Scene View
    private void OnDrawGizmosSelected()
    {
        bool adaInteraksi = beradaDiAreaTrigger || beradaDiAreaEnd;
        Gizmos.color = adaInteraksi ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.TransformPoint(offsetKotakDeteksi), transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawCube(Vector3.zero, ukuranKotakDeteksi);
        Gizmos.DrawWireCube(Vector3.zero, ukuranKotakDeteksi);
    }
}