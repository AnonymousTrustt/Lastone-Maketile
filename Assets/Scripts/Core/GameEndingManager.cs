using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameEndingManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private TownStageManager townStageManager;
    [SerializeField] private BuildingPlacementManager buildingPlacementManager;

    [Header("Ending UI")]
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private TextMeshProUGUI finalStatsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Warning UI")]
    [SerializeField] private TextMeshProUGUI pollutionWarningText;
    [SerializeField] private Color warningColor = new Color(1f, 0.82f, 0.25f);
    [SerializeField] private Color criticalColor = new Color(1f, 0.25f, 0.2f);

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Rules")]
    [SerializeField] private int pollutionCollapseMonths = 3;
    [SerializeField, Range(0f, 1f)] private float warningThresholdPercent = 0.8f;

    private int monthsAbovePollutionLimit;
    private bool endingTriggered;

    private void Awake()
    {
        FindMissingReferences();
    }

    private void OnEnable()
    {
        if (turnManager != null)
        {
            turnManager.TurnEnded += HandleTurnEnded;
        }

        if (resourceManager != null)
        {
            resourceManager.ResourceChanged += HandleResourceChanged;
        }

        if (townStageManager != null)
        {
            townStageManager.StageChanged += HandleStageChanged;
        }
    }

    private void OnDisable()
    {
        if (turnManager != null)
        {
            turnManager.TurnEnded -= HandleTurnEnded;
        }

        if (resourceManager != null)
        {
            resourceManager.ResourceChanged -= HandleResourceChanged;
        }

        if (townStageManager != null)
        {
            townStageManager.StageChanged -= HandleStageChanged;
        }
    }

    private void Start()
    {
        Time.timeScale = 1f;

        if (endingPanel != null)
        {
            endingPanel.SetActive(false);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(LoadMainMenu);
            mainMenuButton.onClick.AddListener(LoadMainMenu);
        }

        UpdatePollutionWarning();
        CheckImmediateEndings();
    }

    private void FindMissingReferences()
    {
        if (resourceManager == null)
        {
            resourceManager = FindFirstObjectByType<ResourceManager>();
        }

        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
        }

        if (townStageManager == null)
        {
            townStageManager = FindFirstObjectByType<TownStageManager>();
        }

        if (buildingPlacementManager == null)
        {
            buildingPlacementManager = FindFirstObjectByType<BuildingPlacementManager>();
        }
    }

    private void HandleTurnEnded(int month)
    {
        if (endingTriggered)
        {
            return;
        }

        UpdatePollutionDangerCounter();
        UpdatePollutionWarning();
        CheckImmediateEndings();
    }

    private void HandleResourceChanged(ResourceType resourceType, int value, int cap, bool hasCap)
    {
        if (endingTriggered)
        {
            return;
        }

        if (resourceType == ResourceType.Pollution)
        {
            UpdatePollutionWarning();
        }

        if (resourceType == ResourceType.Happiness || resourceType == ResourceType.Pollution)
        {
            CheckImmediateEndings();
        }
    }

    private void HandleStageChanged(TownStageData stageData)
    {
        if (endingTriggered)
        {
            return;
        }

        UpdatePollutionWarning();
        CheckImmediateEndings();
    }

    private void UpdatePollutionDangerCounter()
    {
        int pollutionLimit = GetCurrentPollutionLimit();
        int pollution = GetResource(ResourceType.Pollution);

        if (pollutionLimit <= 0 || pollution <= pollutionLimit)
        {
            monthsAbovePollutionLimit = 0;
            return;
        }

        monthsAbovePollutionLimit++;

        if (monthsAbovePollutionLimit >= pollutionCollapseMonths)
        {
            ShowEnding(
                "Threshold Breached",
                "The ERC has shut down Threshold City 17 after repeated environmental violations.");
        }
    }

    private void CheckImmediateEndings()
    {
        if (endingTriggered)
        {
            return;
        }

        if (GetResource(ResourceType.Happiness) <= 0)
        {
            ShowEnding(
                "Public Trust Lost",
                "Citizens have abandoned the city after months of poor living conditions.");
            return;
        }

        TownStageData currentStage = townStageManager != null ? townStageManager.CurrentStage : null;
        if (currentStage != null &&
            currentStage.stage == TownStage.EcoMetropolis &&
            GetResource(ResourceType.Pollution) < currentStage.pollutionLimit)
        {
            ShowEnding(
                "Sustainable Future Secured",
                "Threshold City 17 has become a model for sustainable urban development.");
        }
    }

    private void UpdatePollutionWarning()
    {
        if (pollutionWarningText == null)
        {
            return;
        }

        int pollutionLimit = GetCurrentPollutionLimit();
        int pollution = GetResource(ResourceType.Pollution);

        if (pollutionLimit <= 0)
        {
            pollutionWarningText.gameObject.SetActive(false);
            return;
        }

        if (pollution > pollutionLimit)
        {
            pollutionWarningText.gameObject.SetActive(true);
            pollutionWarningText.color = criticalColor;
            pollutionWarningText.text = $"CRITICAL: Pollution {pollution}/{pollutionLimit}. Reduce pollution now.";
            return;
        }

        if (pollution >= Mathf.CeilToInt(pollutionLimit * warningThresholdPercent))
        {
            pollutionWarningText.gameObject.SetActive(true);
            pollutionWarningText.color = warningColor;
            pollutionWarningText.text = $"Warning: Pollution nearing limit ({pollution}/{pollutionLimit}).";
            return;
        }

        pollutionWarningText.gameObject.SetActive(false);
    }

    private void ShowEnding(string title, string message)
    {
        if (endingTriggered)
        {
            return;
        }

        endingTriggered = true;

        if (turnManager != null)
        {
            turnManager.StopTurns();
        }

        if (buildingPlacementManager != null)
        {
            buildingPlacementManager.CancelPlacement();
            buildingPlacementManager.enabled = false;
        }

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (messageText != null)
        {
            messageText.text = message;
        }

        if (finalStatsText != null)
        {
            finalStatsText.text = BuildFinalStatsText();
        }

        if (endingPanel != null)
        {
            endingPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    private string BuildFinalStatsText()
    {
        string stageName = "Unknown";
        if (townStageManager != null && townStageManager.CurrentStage != null)
        {
            stageName = townStageManager.CurrentStage.stageDisplayName;
        }

        int month = turnManager != null ? turnManager.CurrentTurn : 1;

        return
            $"Month: {month}\n" +
            $"Money: {GetResource(ResourceType.Money)}\n" +
            $"Energy: {GetResource(ResourceType.Energy)}\n" +
            $"Pollution: {GetResource(ResourceType.Pollution)}\n" +
            $"Happiness: {GetResource(ResourceType.Happiness)}\n" +
            $"Stage: {stageName}";
    }

    private int GetCurrentPollutionLimit()
    {
        if (townStageManager == null || townStageManager.CurrentStage == null)
        {
            return 0;
        }

        return townStageManager.CurrentStage.pollutionLimit;
    }

    private int GetResource(ResourceType resourceType)
    {
        return resourceManager != null ? resourceManager.GetResource(resourceType) : 0;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
