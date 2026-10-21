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
    public GameObject uiStasiunEnd;

    [Header("Pengaturan Input & Waktu")]
    public KeyCode tombolPintu = KeyCode.T;
    public float durasiGerakPintu = 7f;
    public float durasiProsesPenumpang = 10f;

    [Header("Pengaturan Deteksi Area Peron Manual")]
    public Vector3 ukuranKotakDeteksi = new Vector3(3f, 3f, 15f);
    public Vector3 offsetKotakDeteksi = Vector3.zero;
    public LayerMask layerStasiun = ~0; 

    [Header("Status Sistem")]
    public bool beradaDiAreaTriggerKanan = false;
    public bool beradaDiAreaEndKanan = false;
    public bool beradaDiAreaTriggerKiri = false;
    public bool beradaDiAreaEndKiri = false;

    public bool pintuTerbuka = false;
    public bool sedangDalamProses = false;

    private string sisiPintuAktif = "";
    private bool isEndStationActive = false;

    void Start()
    {
        if (keretaController == null)
        {
            keretaController = GetComponentInParent<keretaController2>();
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

        if (diAreaPeron && !sedangDalamProses && Input.GetKeyDown(tombolPintu))
        {
            if (keretaController != null && keretaController.currentSpeed <= 0.001f)
            {
                if (!pintuTerbuka)
                {
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
        }
    }

    // --- FUNGSI BARU UNTUK UI MANAGER ---
    public void TriggerPintuFromUI()
    {
        bool diAreaPeron = beradaDiAreaTriggerKanan || beradaDiAreaEndKanan || beradaDiAreaTriggerKiri || beradaDiAreaEndKiri;

        if (diAreaPeron && !sedangDalamProses)
        {
            if (keretaController != null && keretaController.currentSpeed <= 0.001f)
            {
                if (!pintuTerbuka)
                {
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
        }
    }

    private void AturIndikatorAwal()
    {
        if (indikatorPintuKananTerbuka != null) indikatorPintuKananTerbuka.SetActive(false);
        if (indikatorPintuKananTertutup != null) indikatorPintuKananTertutup.SetActive(true);

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
                if (tagObjek == "AreaStasiun") terdeteksiBiasaKanan = true;
                else if (tagObjek == "AreaStasiunEnd" || col.name == "AreaStasiunEnd") terdeteksiEndKanan = true;
                else if (tagObjek == "AreaStasiunKiri") terdeteksiBiasaKiri = true;
                else if (tagObjek == "AreaStasiunEndKiri" || col.name == "AreaStasiunEndKiri") terdeteksiEndKiri = true;
            }
        }

        if (terdeteksiBiasaKanan && !beradaDiAreaTriggerKanan) beradaDiAreaTriggerKanan = true;
        else if (!terdeteksiBiasaKanan && beradaDiAreaTriggerKanan) beradaDiAreaTriggerKanan = false;

        if (terdeteksiEndKanan && !beradaDiAreaEndKanan) beradaDiAreaEndKanan = true;
        else if (!terdeteksiEndKanan && beradaDiAreaEndKanan) beradaDiAreaEndKanan = false;

        if (terdeteksiBiasaKiri && !beradaDiAreaTriggerKiri) beradaDiAreaTriggerKiri = true;
        else if (!terdeteksiBiasaKiri && beradaDiAreaTriggerKiri) beradaDiAreaTriggerKiri = false;

        if (terdeteksiEndKiri && !beradaDiAreaEndKiri) beradaDiAreaEndKiri = true;
        else if (!terdeteksiEndKiri && beradaDiAreaEndKiri) beradaDiAreaEndKiri = false;
    }

    private IEnumerator ProsesBukaDanPenumpang()
    {
        sedangDalamProses = true;
        if (keretaController != null) keretaController.enabled = false;

        if (sisiPintuAktif == "Kanan")
        {
            if (indikatorPintuKananTerbuka != null) indikatorPintuKananTerbuka.SetActive(true);
            if (indikatorPintuKananTertutup != null) indikatorPintuKananTertutup.SetActive(false);
            if (pintuKananAnimator != null) pintuKananAnimator.SetTrigger("BukaPintuKanan");
        }
        else if (sisiPintuAktif == "Kiri")
        {
            if (indikatorPintuKiriTerbuka != null) indikatorPintuKiriTerbuka.SetActive(true);
            if (indikatorPintuKiriTertutup != null) indikatorPintuKiriTertutup.SetActive(false);
            if (pintuKiriAnimator != null) pintuKiriAnimator.SetTrigger("BukaPintuKiri");
        }

        yield return new WaitForSeconds(durasiGerakPintu);
        pintuTerbuka = true;
        yield return new WaitForSeconds(durasiProsesPenumpang);
        sedangDalamProses = false;
    }

    private IEnumerator ProsesTutupPintu()
    {
        sedangDalamProses = true;

        if (sisiPintuAktif == "Kanan")
        {
            if (indikatorPintuKananTerbuka != null) indikatorPintuKananTerbuka.SetActive(false);
            if (indikatorPintuKananTertutup != null) indikatorPintuKananTertutup.SetActive(true);
            if (pintuKananAnimator != null) pintuKananAnimator.SetTrigger("TutupPintuKanan");
        }
        else if (sisiPintuAktif == "Kiri")
        {
            if (indikatorPintuKiriTerbuka != null) indikatorPintuKiriTerbuka.SetActive(false);
            if (indikatorPintuKiriTertutup != null) indikatorPintuKiriTertutup.SetActive(true);
            if (pintuKiriAnimator != null) pintuKiriAnimator.SetTrigger("TutupPintuKiri");
        }

        yield return new WaitForSeconds(durasiGerakPintu);
        pintuTerbuka = false;

        if (keretaController != null) keretaController.enabled = true;
        if (isEndStationActive && uiStasiunEnd != null) uiStasiunEnd.SetActive(true);

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