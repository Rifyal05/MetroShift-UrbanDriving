using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    public GameObject canvasPauseOverlay;
    public GameObject panelPause;
    public GameObject panelSettings;
    public GameObject panelCarController;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (canvasPauseOverlay != null) canvasPauseOverlay.SetActive(false);
        if (panelPause != null) panelPause.SetActive(false);
        if (panelSettings != null) panelSettings.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (canvasPauseOverlay != null) canvasPauseOverlay.SetActive(true);
        if (panelPause != null) panelPause.SetActive(true);
        if (panelSettings != null) panelSettings.SetActive(false);
        
        if (panelCarController != null) panelCarController.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        if (canvasPauseOverlay != null) canvasPauseOverlay.SetActive(false);
        
        if (panelCarController != null)
        {
            if (GameManager.Instance != null)
            {
                panelCarController.SetActive(GameManager.Instance.gameSudahMulai);
            }
            else
            {
                panelCarController.SetActive(true);
            }
        }
    }

    public void OpenSettings()
    {
        if (panelPause != null) panelPause.SetActive(false);
        if (panelSettings != null) panelSettings.SetActive(true);
    }

    public void CloseSettings()
    {
        if (UnityEngine.EventSystems.EventSystem.current != null)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        }

        if (panelSettings != null) panelSettings.SetActive(false);
        if (panelPause != null) panelPause.SetActive(true);
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}