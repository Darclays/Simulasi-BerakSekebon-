using System.Collections;
using UnityEngine;

public class PintuPenumpangController : MonoBehaviour
{
    [Header("Referensi Controller Kereta")]
    public keretaController2 keretaController;

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
    public bool pintuTerbuka = false;
    public bool sedangDalamProses = false;

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
    }

    void Update()
    {
        // 1. Deteksi peron secara fisik manual per frame
        CekAreaPeronManual();

        // Debug tes tombol T
        if (Input.GetKeyDown(tombolPintu))
        {
            float speedSaatIni = (keretaController != null) ? keretaController.currentSpeed : -1f;
            Debug.Log($"[PintuController] Tombol T Ditekan! Status -> Di Area Stasiun: {beradaDiAreaTrigger}, Sedang Proses: {sedangDalamProses}, Kecepatan: {speedSaatIni:F1} km/h");
        }

        // 2. Eksekusi Buka/Tutup Pintu
        if (beradaDiAreaTrigger && !sedangDalamProses && Input.GetKeyDown(tombolPintu))
        {
            if (keretaController != null && keretaController.currentSpeed <= 0.001f)
            {
                if (!pintuTerbuka)
                {
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

        bool terdeteksi = false;
        foreach (var col in hitColliders)
        {
            // Pengecekan apakah collider yang tersentuh memiliki Tag "AreaStasiun"
            if (col.gameObject != gameObject && col.CompareTag("AreaStasiun"))
            {
                terdeteksi = true;
                break;
            }
        }

        // Update status masukan/keluaran area peron
        if (terdeteksi && !beradaDiAreaTrigger)
        {
            beradaDiAreaTrigger = true;
            Debug.Log("[PintuController] >>> BERHASIL DETEKSI AREA PERON/STASIUN! Siap buka pintu jika kereta berhenti. <<<");
        }
        else if (!terdeteksi && beradaDiAreaTrigger)
        {
            beradaDiAreaTrigger = false;
            Debug.Log("[PintuController] <<< Keluar dari area Peron/Stasiun. >>>");
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
            Debug.Log("[PintuController] KONTROL KERETA DIAKTIFKAN KEMBALI. Kereta siap berjalan.");
        }

        sedangDalamProses = false;
    }

    // Menampilkan kotak deteksi berwarna merah transparan di Editor Scene View
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = beradaDiAreaTrigger ? new Color(0f, 1f, 0f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.TransformPoint(offsetKotakDeteksi), transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawCube(Vector3.zero, ukuranKotakDeteksi);
        Gizmos.DrawWireCube(Vector3.zero, ukuranKotakDeteksi);
    }
}