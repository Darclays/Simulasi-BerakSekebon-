using UnityEngine;
using Unity.Cinemachine; 
using System.Collections; // Dibutuhkan untuk IEnumerator (jeda waktu)

public class DynamicMainMenu : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    public CinemachineCamera camAwalMula; // Kamera baru ditambahkan di sini
    public CinemachineCamera camTitle;
    public CinemachineCamera camMainMenu;
    public CinemachineCamera camSettings;

    [Header("UI Panels")]
    public GameObject panelTitle;
    public GameObject panelMainMenu;
    public GameObject panelSettings;

    [Header("Pengaturan Awal")]
    [Tooltip("Berapa detik sebelum kamera bergerak dari Awal ke Title?")]
    public float waktuTungguAwal = 2.0f; // Bisa diatur di Inspector

    private void Start()
    {
        // Mulai dari kamera awal tanpa menampilkan UI
        ShowAwalMulaScreen();

        // Mulai transisi ke Title Screen setelah jeda
        StartCoroutine(TransisiKeTitle());
    }

    private void Update()
    {
        // Deteksi input "Press Any Button" HANYA jika panel Title aktif
        if (panelTitle.activeSelf && Input.anyKeyDown)
        {
            ShowMainMenu();
        }
    }

    public void ShowAwalMulaScreen()
    {
        // Cam Awal Mula mendapat prioritas tertinggi di awal
        camAwalMula.Priority = 20;
        camTitle.Priority = 10;
        camMainMenu.Priority = 10;
        camSettings.Priority = 10;

        // Matikan semua UI di awal agar terlihat sinematik
        panelTitle.SetActive(false);
        panelMainMenu.SetActive(false);
        panelSettings.SetActive(false);
    }

    private IEnumerator TransisiKeTitle()
    {
        // Tunggu beberapa detik di posisi awal
        yield return new WaitForSeconds(waktuTungguAwal);

        // Setelah menunggu, pindah ke layar Title (Press Any Button)
        ShowTitleScreen();
    }

    public void ShowTitleScreen()
    {
        camAwalMula.Priority = 10; // Turunkan prioritas kamera awal
        camTitle.Priority = 20;
        camMainMenu.Priority = 10;
        camSettings.Priority = 10;

        panelTitle.SetActive(true);
        panelMainMenu.SetActive(false);
        panelSettings.SetActive(false);
    }

    public void ShowMainMenu()
    {
        camAwalMula.Priority = 10;
        camTitle.Priority = 10;
        camMainMenu.Priority = 20;
        camSettings.Priority = 10;

        panelTitle.SetActive(false);
        panelMainMenu.SetActive(true);
        panelSettings.SetActive(false);
    }

    public void ShowSettings()
    {
        camAwalMula.Priority = 10;
        camTitle.Priority = 10;
        camMainMenu.Priority = 10;
        camSettings.Priority = 20;

        panelTitle.SetActive(false);
        panelMainMenu.SetActive(false);
        panelSettings.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Keluar dari Game!");
    }
}