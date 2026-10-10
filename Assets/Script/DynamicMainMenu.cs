using UnityEngine;
using Unity.Cinemachine; // Namespace baru untuk Cinemachine 3.x di Unity 6

public class DynamicMainMenu : MonoBehaviour
{
    [Header("Cinemachine Cameras")]
    public CinemachineCamera camTitle;
    public CinemachineCamera camMainMenu;
    public CinemachineCamera camSettings;

    [Header("UI Panels")]
    public GameObject panelTitle;
    public GameObject panelMainMenu;
    public GameObject panelSettings;

    private void Start()
    {
        // Saat game dimulai, tampilkan layar judul
        ShowTitleScreen();
    }

    private void Update()
    {
        // Deteksi input seperti "Press Any Button" di Persona 5
        if (panelTitle.activeSelf && Input.anyKeyDown)
        {
            ShowMainMenu();
        }
    }

    // Logic Cinemachine: Kamera dengan Priority tertinggi akan aktif, 
    // dan Cinemachine otomatis membuat pergerakan kamera (blend) yang mulus.

    public void ShowTitleScreen()
    {
        // Mengatur Prioritas Kamera
        camTitle.Priority = 20;
        camMainMenu.Priority = 10;
        camSettings.Priority = 10;

        // Mengatur UI yang tampil
        panelTitle.SetActive(true);
        panelMainMenu.SetActive(false);
        panelSettings.SetActive(false);
    }

    public void ShowMainMenu()
    {
        camTitle.Priority = 10;
        camMainMenu.Priority = 20;
        camSettings.Priority = 10;

        panelTitle.SetActive(false);
        panelMainMenu.SetActive(true);
        panelSettings.SetActive(false);
    }

    public void ShowSettings()
    {
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