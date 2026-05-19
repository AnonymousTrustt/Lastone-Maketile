using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class BuildingCategoryGroup
{
    public string categoryName = "Residential";
    public List<BuildingDefinition> buildings = new List<BuildingDefinition>();
}

public class CityMenuUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private BuildingPlacementManager buildingPlacementManager;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private TownStageManager townStageManager;
    [SerializeField] private ObjectiveManager objectiveManager;

    [Header("Main Controls")]
    [SerializeField] private Button cityMenuButton;
    [SerializeField] private RectTransform bottomPopupPanel;
    [SerializeField] private Image cityMenuButtonImage;
    [SerializeField] private Image cityMenuArrowImage;
    [SerializeField] private Sprite closedButtonSprite;
    [SerializeField] private Sprite openButtonSprite;
    [SerializeField] private float slideDuration = 0.2f;
    [SerializeField] private bool closePanelAfterBuildingSelected = true;

    [Header("Views")]
    [SerializeField] private GameObject mainMenuView;
    [SerializeField] private GameObject buildingsView;
    [SerializeField] private GameObject objectivesView;
    [SerializeField] private GameObject cityStatusView;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button buildingsButton;
    [SerializeField] private Button objectivesButton;
    [SerializeField] private Button cityStatusButton;

    [Header("Back Buttons")]
    [SerializeField] private Button buildingsBackButton;
    [SerializeField] private Button objectivesBackButton;
    [SerializeField] private Button cityStatusBackButton;

    [Header("Buildings View")]
    [SerializeField] private List<BuildingCategoryGroup> buildingCategories = new List<BuildingCategoryGroup>();
    [SerializeField] private Transform categoryButtonParent;
    [SerializeField] private Transform buildingButtonParent;
    [SerializeField] private Button categoryButtonPrefab;
    [SerializeField] private BuildingMenuButton buildingButtonPrefab;
    [SerializeField] private TextMeshProUGUI buildingsTitleText;

    [Header("Objectives View Text")]
    [SerializeField] private TextMeshProUGUI objectiveTitleText;
    [SerializeField] private TextMeshProUGUI objectiveRequirementsText;
    [SerializeField] private TextMeshProUGUI objectiveProgressText;
    [SerializeField] private TextMeshProUGUI objectiveStageText;
    [SerializeField] private TextMeshProUGUI objectivePollutionLimitText;
    [SerializeField] private TextMeshProUGUI objectiveDeadlineText;

    [Header("City Status View Text")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI pollutionText;
    [SerializeField] private TextMeshProUGUI happinessText;
    [SerializeField] private TextMeshProUGUI monthText;
    [SerializeField] private TextMeshProUGUI stageText;

    private readonly List<GameObject> spawnedCategoryButtons = new List<GameObject>();
    private readonly List<GameObject> spawnedBuildingButtons = new List<GameObject>();
    private Coroutine slideCoroutine;
    private RectTransform cityMenuButtonRect;
    private TextMeshProUGUI cityMenuButtonLabel;
    private Vector2 hiddenPosition;
    private Vector2 shownPosition;
    private Vector2 buttonHiddenPosition;
    private Vector2 buttonShownPosition;
    private bool isOpen;

    private void Awake()
    {
        FindMissingManagers();
        CachePanelPositions();
        HookButtons();
        ShowMainMenu();
        SetPanelVisibleInstant(false);
    }

    private void OnEnable()
    {
        if (resourceManager != null)
        {
            resourceManager.ResourceChanged += HandleResourceChanged;
        }

        if (turnManager != null)
        {
            turnManager.TurnEnded += HandleTurnEnded;
        }

        if (townStageManager != null)
        {
            townStageManager.StageChanged += HandleStageChanged;
        }
    }

    private void OnDisable()
    {
        if (resourceManager != null)
        {
            resourceManager.ResourceChanged -= HandleResourceChanged;
        }

        if (turnManager != null)
        {
            turnManager.TurnEnded -= HandleTurnEnded;
        }

        if (townStageManager != null)
        {
            townStageManager.StageChanged -= HandleStageChanged;
        }
    }

    private void Start()
    {
        BuildCategoryButtons();
        RefreshAllReadouts();
    }

    public void TogglePanel()
    {
        SetPanelOpen(!isOpen);
    }

    public void ShowMainMenu()
    {
        SetActiveView(mainMenuView);
    }

    public void ShowBuildingsView()
    {
        SetActiveView(buildingsView);
        BuildCategoryButtons();

        if (buildingCategories.Count > 0)
        {
            ShowBuildingCategory(0);
        }
        else
        {
            ClearSpawned(spawnedBuildingButtons);
            SetText(buildingsTitleText, "Buildings");
        }
    }

    public void ShowObjectivesView()
    {
        SetActiveView(objectivesView);
        RefreshObjectiveView();
    }

    public void ShowCityStatusView()
    {
        SetActiveView(cityStatusView);
        RefreshCityStatusView();
    }

    public void SelectBuilding(BuildingDefinition buildingDefinition)
    {
        if (buildingPlacementManager == null)
        {
            Debug.LogWarning("CityMenuUI cannot place buildings because BuildingPlacementManager is not assigned.");
            return;
        }

        buildingPlacementManager.BeginPlacement(buildingDefinition);

        if (closePanelAfterBuildingSelected)
        {
            SetPanelOpen(false);
        }
    }

    private void FindMissingManagers()
    {
        if (buildingPlacementManager == null)
        {
            buildingPlacementManager = FindFirstObjectByType<BuildingPlacementManager>();
        }

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

        if (objectiveManager == null)
        {
            objectiveManager = FindFirstObjectByType<ObjectiveManager>();
        }
    }

    private void CachePanelPositions()
    {
        if (bottomPopupPanel == null)
        {
            return;
        }

        if (cityMenuButton != null)
        {
            cityMenuButtonRect = cityMenuButton.GetComponent<RectTransform>();
            cityMenuButtonImage = cityMenuButton.GetComponent<Image>();
            if (cityMenuArrowImage == null)
            {
                Transform arrowTransform = cityMenuButton.transform.Find("ArrowIcon");
                if (arrowTransform != null)
                {
                    cityMenuArrowImage = arrowTransform.GetComponent<Image>();
                }
            }
            cityMenuButtonLabel = cityMenuButton.GetComponentInChildren<TextMeshProUGUI>();
            cityMenuButton.transform.SetAsLastSibling();
        }

        shownPosition = bottomPopupPanel.anchoredPosition;
        hiddenPosition = shownPosition - new Vector2(0f, bottomPopupPanel.rect.height + 24f);

        if (cityMenuButtonRect != null)
        {
            buttonHiddenPosition = cityMenuButtonRect.anchoredPosition;
            buttonShownPosition = buttonHiddenPosition + new Vector2(0f, bottomPopupPanel.rect.height + 12f);
        }
    }

    private void HookButtons()
    {
        AddClick(cityMenuButton, TogglePanel);
        AddClick(buildingsButton, ShowBuildingsView);
        AddClick(objectivesButton, ShowObjectivesView);
        AddClick(cityStatusButton, ShowCityStatusView);
        AddClick(buildingsBackButton, ShowMainMenu);
        AddClick(objectivesBackButton, ShowMainMenu);
        AddClick(cityStatusBackButton, ShowMainMenu);
    }

    private void AddClick(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
        }
    }

    private void SetPanelOpen(bool open)
    {
        if (bottomPopupPanel == null)
        {
            return;
        }

        isOpen = open;

        if (open)
        {
            bottomPopupPanel.gameObject.SetActive(true);
            RefreshAllReadouts();
        }

        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
        }

        SetCityMenuButtonLabel(open);
        slideCoroutine = StartCoroutine(SlidePanel(open ? shownPosition : hiddenPosition, open ? buttonShownPosition : buttonHiddenPosition, !open));
    }

    private IEnumerator SlidePanel(Vector2 targetPanelPosition, Vector2 targetButtonPosition, bool hideWhenDone)
    {
        Vector2 startPanelPosition = bottomPopupPanel.anchoredPosition;
        Vector2 startButtonPosition = cityMenuButtonRect != null ? cityMenuButtonRect.anchoredPosition : Vector2.zero;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(0.01f, slideDuration));
            t = Mathf.SmoothStep(0f, 1f, t);
            bottomPopupPanel.anchoredPosition = Vector2.Lerp(startPanelPosition, targetPanelPosition, t);

            if (cityMenuButtonRect != null)
            {
                cityMenuButtonRect.anchoredPosition = Vector2.Lerp(startButtonPosition, targetButtonPosition, t);
            }

            yield return null;
        }

        bottomPopupPanel.anchoredPosition = targetPanelPosition;

        if (cityMenuButtonRect != null)
        {
            cityMenuButtonRect.anchoredPosition = targetButtonPosition;
        }

        if (hideWhenDone)
        {
            bottomPopupPanel.gameObject.SetActive(false);
            ShowMainMenu();
            SetCityMenuButtonLabel(false);
        }

        slideCoroutine = null;
    }

    private void SetPanelVisibleInstant(bool visible)
    {
        if (bottomPopupPanel == null)
        {
            return;
        }

        isOpen = visible;
        bottomPopupPanel.anchoredPosition = visible ? shownPosition : hiddenPosition;
        if (cityMenuButtonRect != null)
        {
            cityMenuButtonRect.anchoredPosition = visible ? buttonShownPosition : buttonHiddenPosition;
        }

        SetCityMenuButtonLabel(visible);
        bottomPopupPanel.gameObject.SetActive(visible);
    }

    private void SetCityMenuButtonLabel(bool open)
    {
        if (cityMenuArrowImage != null)
        {
            Sprite sprite = open ? openButtonSprite : closedButtonSprite;
            if (sprite != null)
            {
                cityMenuArrowImage.sprite = sprite;
                cityMenuArrowImage.type = Image.Type.Simple;
                cityMenuArrowImage.preserveAspect = true;
            }
        }

        if (cityMenuButtonLabel != null)
        {
            cityMenuButtonLabel.text = string.Empty;
        }
    }

    private void SetActiveView(GameObject activeView)
    {
        SetViewActive(mainMenuView, activeView);
        SetViewActive(buildingsView, activeView);
        SetViewActive(objectivesView, activeView);
        SetViewActive(cityStatusView, activeView);
    }

    private void SetViewActive(GameObject view, GameObject activeView)
    {
        if (view != null)
        {
            view.SetActive(view == activeView);
        }
    }

    private void BuildCategoryButtons()
    {
        if (categoryButtonParent == null || categoryButtonPrefab == null)
        {
            return;
        }

        ClearSpawned(spawnedCategoryButtons);

        for (int i = 0; i < buildingCategories.Count; i++)
        {
            int categoryIndex = i;
            BuildingCategoryGroup category = buildingCategories[i];
            Button button = Instantiate(categoryButtonPrefab, categoryButtonParent);
            button.gameObject.SetActive(true);
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
            {
                label.text = category.categoryName;
            }

            TooltipTrigger tooltipTrigger = button.GetComponent<TooltipTrigger>();
            if (tooltipTrigger == null)
            {
                tooltipTrigger = button.gameObject.AddComponent<TooltipTrigger>();
            }

            tooltipTrigger.SetText(category.categoryName, $"Show {category.categoryName.ToLower()} buildings available for placement.");
            button.onClick.AddListener(() => ShowBuildingCategory(categoryIndex));
            spawnedCategoryButtons.Add(button.gameObject);
        }
    }

    private void ShowBuildingCategory(int categoryIndex)
    {
        if (categoryIndex < 0 || categoryIndex >= buildingCategories.Count)
        {
            return;
        }

        BuildingCategoryGroup category = buildingCategories[categoryIndex];
        SetText(buildingsTitleText, category.categoryName);
        ClearSpawned(spawnedBuildingButtons);

        if (buildingButtonParent == null || buildingButtonPrefab == null)
        {
            return;
        }

        foreach (BuildingDefinition buildingDefinition in category.buildings)
        {
            BuildingMenuButton button = Instantiate(buildingButtonPrefab, buildingButtonParent);
            button.gameObject.SetActive(true);
            button.Setup(buildingDefinition, this);
            spawnedBuildingButtons.Add(button.gameObject);
        }
    }

    private void RefreshAllReadouts()
    {
        RefreshObjectiveView();
        RefreshCityStatusView();
    }

    private void RefreshObjectiveView()
    {
        ObjectiveData objective = objectiveManager != null ? objectiveManager.CurrentObjective : null;
        TownStageData currentStage = townStageManager != null ? townStageManager.CurrentStage : null;

        SetText(objectiveTitleText, objective != null ? objective.objectiveName : "No Active Objective");
        SetText(objectiveRequirementsText, objectiveManager != null ? objectiveManager.GetObjectiveRequirementsText() : "No objective manager assigned.");
        SetText(objectiveProgressText, objectiveManager != null ? objectiveManager.GetObjectiveProgressText() : "No progress available.");
        SetText(objectiveStageText, $"Current Stage: {(currentStage != null ? currentStage.stageDisplayName : "Unknown")}");
        SetText(objectivePollutionLimitText, $"Pollution Limit: {(objective != null ? objective.maximumPollution : 0)}");
        SetText(objectiveDeadlineText, $"Deadline Month: {(objective != null ? objective.deadlineMonth.ToString() : "None")}");
    }

    private void RefreshCityStatusView()
    {
        TownStageData currentStage = townStageManager != null ? townStageManager.CurrentStage : null;

        SetText(moneyText, $"Money: {GetResourceValue(ResourceType.Money)}");
        SetText(energyText, $"Energy: {GetResourceValue(ResourceType.Energy)}");
        SetText(pollutionText, $"Pollution: {GetResourceValue(ResourceType.Pollution)}");
        SetText(happinessText, $"Happiness: {GetResourceValue(ResourceType.Happiness)}");
        SetText(monthText, $"Month: {(turnManager != null ? turnManager.CurrentTurn : 1)}");
        SetText(stageText, $"Stage: {(currentStage != null ? currentStage.stageDisplayName : "Unknown")}");
    }

    private int GetResourceValue(ResourceType resourceType)
    {
        return resourceManager != null ? resourceManager.GetResource(resourceType) : 0;
    }

    private void HandleResourceChanged(ResourceType resourceType, int value, int cap, bool hasCap)
    {
        RefreshAllReadouts();
    }

    private void HandleTurnEnded(int month)
    {
        RefreshAllReadouts();
    }

    private void HandleStageChanged(TownStageData stage)
    {
        RefreshAllReadouts();
    }

    private void SetText(TextMeshProUGUI text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private void ClearSpawned(List<GameObject> spawnedObjects)
    {
        foreach (GameObject spawnedObject in spawnedObjects)
        {
            if (spawnedObject != null)
            {
                Destroy(spawnedObject);
            }
        }

        spawnedObjects.Clear();
    }
}
