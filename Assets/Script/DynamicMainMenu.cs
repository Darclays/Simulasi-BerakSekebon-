using UnityEngine;
using Unity.Cinemachine; 
using System.Collections; 
using UnityEngine.SceneManagement; // Wajib ditambahkan untuk mengurus perpindahan Scene

public class DynamicMainMenu : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    public CinemachineCamera camAwalMula; 
    public CinemachineCamera camTitle;
    public CinemachineCamera camMainMenu;
    public CinemachineCamera camSettings;

    [Header("UI Panels")]
    public GameObject panelTitle;
    public GameObject panelMainMenu;
    public GameObject panelSettings;

    [Header("Pengaturan Awal")]
    [Tooltip("Berapa detik sebelum kamera bergerak dari Awal ke Title?")]
    public float waktuTungguAwal = 2.0f; 

    private void Start()
    {
        ShowAwalMulaScreen();
        StartCoroutine(TransisiKeTitle());
    }

    private void Update()
    {
        if (panelTitle.activeSelf && Input.anyKeyDown)
        {
            ShowMainMenu();
        }
    }

    public void ShowAwalMulaScreen()
    {
        camAwalMula.Priority = 20;
        camTitle.Priority = 10;
        camMainMenu.Priority = 10;
        camSettings.Priority = 10;

        panelTitle.SetActive(false);
        panelMainMenu.SetActive(false);
        panelSettings.SetActive(false);
    }

    private IEnumerator TransisiKeTitle()
    {
        yield return new WaitForSeconds(waktuTungguAwal);
        ShowTitleScreen();
    }

    public void ShowTitleScreen()
    {
        camAwalMula.Priority = 10; 
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

    // --- FUNGSI BARU UNTUK NEW GAME ---
    public void StartNewGame()
    {
        Debug.Log("Memuat Scene: KiHa40_Scenario...");
        SceneManager.LoadScene("KiHa40_Scenario");
    }

    // --- FUNGSI QUIT YANG DIPERBARUI ---
    public void QuitGame()
    {
        Debug.Log("Keluar dari Game!");
        
        // Mematikan game saat sudah di-build (aplikasi sungguhan)
        Application.Quit(); 

        // Mematikan play mode saat dites di dalam Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}