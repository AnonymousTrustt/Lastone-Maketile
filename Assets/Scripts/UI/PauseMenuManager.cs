using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuManager : MonoBehaviour
{
    private enum WarningAction
    {
        None,
        Restart,
        QuitToMenu
    }

    [Header("UI")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button audioToggleButton;
    [SerializeField] private TextMeshProUGUI audioToggleButtonText;
    [SerializeField] private Button saveGameButton;
    [SerializeField] private TextMeshProUGUI saveConfirmationText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitToMenuButton;

    [Header("Warning")]
    [SerializeField] private GameObject warningPanel;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;

    [Header("Game References")]
    [SerializeField] private SaveLoadManager saveLoadManager;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private BuildingPlacementManager buildingPlacementManager;
    [SerializeField] private BackgroundAudioManager backgroundAudioManager;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private WarningAction pendingWarningAction = WarningAction.None;
    private bool isPaused;
    private bool turnManagerWasRunning;
    private bool placementManagerWasEnabled;

    private void Awake()
    {
        if (IsMainMenuScene())
        {
            HidePauseMenuImmediate();

            if (pauseButton != null)
            {
                pauseButton.gameObject.SetActive(false);
            }

            enabled = false;
            return;
        }

        FindMissingReferences();
        HidePauseMenuImmediate();
        UpdateAudioButtonText();
    }

    private void OnEnable()
    {
        if (IsMainMenuScene())
        {
            return;
        }

        AddButtonListeners();
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
    }

    private void FindMissingReferences()
    {
        if (saveLoadManager == null)
        {
            saveLoadManager = FindFirstObjectByType<SaveLoadManager>();
        }

        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
        }

        if (buildingPlacementManager == null)
        {
            buildingPlacementManager = FindFirstObjectByType<BuildingPlacementManager>();
        }

        if (backgroundAudioManager == null)
        {
            backgroundAudioManager = FindFirstObjectByType<BackgroundAudioManager>();
        }
    }

    private void AddButtonListeners()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(OpenPauseMenu);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (audioToggleButton != null)
        {
            audioToggleButton.onClick.AddListener(ToggleAudio);
        }

        if (saveGameButton != null)
        {
            saveGameButton.onClick.AddListener(SaveGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(ShowRestartWarning);
        }

        if (quitToMenuButton != null)
        {
            quitToMenuButton.onClick.AddListener(ShowQuitToMenuWarning);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmWarningAction);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelWarning);
        }
    }

    private void RemoveButtonListeners()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(OpenPauseMenu);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }

        if (audioToggleButton != null)
        {
            audioToggleButton.onClick.RemoveListener(ToggleAudio);
        }

        if (saveGameButton != null)
        {
            saveGameButton.onClick.RemoveListener(SaveGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(ShowRestartWarning);
        }

        if (quitToMenuButton != null)
        {
            quitToMenuButton.onClick.RemoveListener(ShowQuitToMenuWarning);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmWarningAction);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(CancelWarning);
        }
    }

    public void OpenPauseMenu()
    {
        if (isPaused)
        {
            return;
        }

        FindMissingReferences();

        isPaused = true;
        turnManagerWasRunning = turnManager != null && turnManager.IsRunning;
        placementManagerWasEnabled = buildingPlacementManager != null && buildingPlacementManager.enabled;

        if (turnManagerWasRunning)
        {
            turnManager.StopTurns();
        }

        if (buildingPlacementManager != null)
        {
            buildingPlacementManager.CancelPlacement();
            buildingPlacementManager.enabled = false;
        }

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(false);
        }

        if (titleText != null)
        {
            titleText.text = "Paused";
        }

        ClearSaveConfirmation();
        HideWarning();
        UpdateAudioButtonText();
    }

    public void ResumeGame()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;
        pendingWarningAction = WarningAction.None;
        Time.timeScale = 1f;

        if (turnManager != null && turnManagerWasRunning)
        {
            turnManager.StartTurns();
        }

        if (buildingPlacementManager != null)
        {
            buildingPlacementManager.enabled = placementManagerWasEnabled;
        }

        HidePauseMenuImmediate();

        if (pauseButton != null)
        {
            pauseButton.gameObject.SetActive(true);
        }
    }

    public void ToggleAudio()
    {
        FindMissingReferences();

        if (backgroundAudioManager == null)
        {
            Debug.LogWarning("Pause menu could not find a BackgroundAudioManager.");
            return;
        }

        backgroundAudioManager.ToggleAudio();
        UpdateAudioButtonText();
    }

    public void SaveGame()
    {
        FindMissingReferences();

        if (saveLoadManager == null)
        {
            Debug.LogWarning("Pause menu could not find a SaveLoadManager.");
            ShowSaveConfirmation("Save manager missing.");
            return;
        }

        saveLoadManager.SaveGame();
        ShowSaveConfirmation("Game saved.");
    }

    public void ShowRestartWarning()
    {
        ShowWarning("Restarting will lose unsaved progress. Continue?", WarningAction.Restart);
    }

    public void ShowQuitToMenuWarning()
    {
        ShowWarning("Quit to main menu? Unsaved progress may be lost.", WarningAction.QuitToMenu);
    }

    public void ConfirmWarningAction()
    {
        Time.timeScale = 1f;

        if (pendingWarningAction == WarningAction.Restart)
        {
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
        }
        else if (pendingWarningAction == WarningAction.QuitToMenu)
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void CancelWarning()
    {
        pendingWarningAction = WarningAction.None;
        HideWarning();
    }

    private void ShowWarning(string message, WarningAction warningAction)
    {
        pendingWarningAction = warningAction;
        ClearSaveConfirmation();

        if (warningText != null)
        {
            warningText.text = message;
        }

        if (warningPanel != null)
        {
            warningPanel.SetActive(true);
        }
    }

    private void HideWarning()
    {
        pendingWarningAction = WarningAction.None;

        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }

    private void ShowSaveConfirmation(string message)
    {
        if (saveConfirmationText != null)
        {
            saveConfirmationText.text = message;
            saveConfirmationText.gameObject.SetActive(true);
        }
    }

    private void ClearSaveConfirmation()
    {
        if (saveConfirmationText != null)
        {
            saveConfirmationText.text = string.Empty;
            saveConfirmationText.gameObject.SetActive(false);
        }
    }

    private void UpdateAudioButtonText()
    {
        if (audioToggleButtonText == null)
        {
            return;
        }

        if (backgroundAudioManager == null)
        {
            backgroundAudioManager = FindFirstObjectByType<BackgroundAudioManager>();
        }

        bool audioOn = backgroundAudioManager == null || backgroundAudioManager.IsAudioOn;
        audioToggleButtonText.text = audioOn ? "Audio: ON" : "Audio: OFF";
    }

    private void HidePauseMenuImmediate()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (warningPanel != null)
        {
            warningPanel.SetActive(false);
        }
    }

    private bool IsMainMenuScene()
    {
        return SceneManager.GetActiveScene().name == mainMenuSceneName;
    }
}
