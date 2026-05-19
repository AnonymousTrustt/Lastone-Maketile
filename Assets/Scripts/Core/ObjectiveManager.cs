using System;
using TMPro;
using UnityEngine;

[Serializable]
public class ObjectiveData
{
    public string objectiveName = "Build a Happier Village";
    public int requiredHappiness = 200;
    public int maximumPollution = 100;
    public int deadlineMonth = 24;
}

public class ObjectiveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private TownStageManager townStageManager;
    [SerializeField] private TextMeshProUGUI objectiveText;

    [Header("Objective")]
    [SerializeField] private ObjectiveData currentObjective = new ObjectiveData();

    private bool objectiveCompleted;
    private bool objectiveFailed;

    public ObjectiveData CurrentObjective => currentObjective;
    public bool ObjectiveCompleted => objectiveCompleted;
    public bool ObjectiveFailed => objectiveFailed;

    private void Awake()
    {
        if (turnManager == null)
        {
            turnManager = FindFirstObjectByType<TurnManager>();
        }

        if (resourceManager == null)
        {
            resourceManager = FindFirstObjectByType<ResourceManager>();
        }

        if (townStageManager == null)
        {
            townStageManager = FindFirstObjectByType<TownStageManager>();
        }
    }

    private void OnEnable()
    {
        if (turnManager != null)
        {
            turnManager.TurnEnded += HandleTurnEnded;
        }
    }

    private void OnDisable()
    {
        if (turnManager != null)
        {
            turnManager.TurnEnded -= HandleTurnEnded;
        }
    }

    private void Start()
    {
        UpdateObjectiveText();
    }

    public void CheckObjective()
    {
        if (objectiveCompleted || objectiveFailed || resourceManager == null || townStageManager == null || currentObjective == null)
        {
            return;
        }

        int currentMonth = turnManager != null ? turnManager.CurrentTurn : 1;
        int happiness = resourceManager.GetResource(ResourceType.Happiness);
        int pollution = resourceManager.GetResource(ResourceType.Pollution);
        int money = resourceManager.GetResource(ResourceType.Money);

        townStageManager.CheckPollutionLimit();

        bool happinessReached = happiness >= currentObjective.requiredHappiness;
        bool pollutionUnderLimit = pollution < currentObjective.maximumPollution;
        bool beforeDeadline = currentMonth <= currentObjective.deadlineMonth;
        bool stageMoneyReached = townStageManager.CurrentStage == null || money >= townStageManager.CurrentStage.requiredMoneyAmount;

        if (happinessReached && pollutionUnderLimit && beforeDeadline && stageMoneyReached)
        {
            CompleteObjective();
            return;
        }

        if (currentMonth > currentObjective.deadlineMonth && !objectiveCompleted)
        {
            objectiveFailed = true;
            Debug.LogWarning($"Objective failed: {currentObjective.objectiveName}. Deadline was Month {currentObjective.deadlineMonth}.");
            UpdateObjectiveText();
        }
    }

    public string GetObjectiveRequirementsText()
    {
        if (currentObjective == null)
        {
            return "No active objective.";
        }

        return $"Reach {currentObjective.requiredHappiness} Happiness\nKeep Pollution below {currentObjective.maximumPollution}";
    }

    public string GetObjectiveProgressText()
    {
        if (currentObjective == null || resourceManager == null)
        {
            return "No progress available.";
        }

        int currentMonth = turnManager != null ? turnManager.CurrentTurn : 1;
        int happiness = resourceManager.GetResource(ResourceType.Happiness);
        int pollution = resourceManager.GetResource(ResourceType.Pollution);

        if (objectiveCompleted)
        {
            return "Completed";
        }

        if (objectiveFailed)
        {
            return "Failed";
        }

        return $"Happiness {happiness}/{currentObjective.requiredHappiness}\nPollution {pollution}/{currentObjective.maximumPollution}\nMonth {currentMonth}/{currentObjective.deadlineMonth}";
    }

    private void HandleTurnEnded(int month)
    {
        CheckObjective();
    }

    private void CompleteObjective()
    {
        objectiveCompleted = true;
        Debug.Log($"Objective completed: {currentObjective.objectiveName}. Advancing town stage.");

        townStageManager.AdvanceStage();
        UpdateObjectiveText();
    }

    private void UpdateObjectiveText()
    {
        if (objectiveText == null || currentObjective == null)
        {
            return;
        }

        if (objectiveCompleted)
        {
            objectiveText.text = $"Objective Complete: {currentObjective.objectiveName}";
            return;
        }

        if (objectiveFailed)
        {
            objectiveText.text = $"Objective Failed: {currentObjective.objectiveName}";
            return;
        }

        objectiveText.text = $"Objective: Reach {currentObjective.requiredHappiness} Happiness, keep Pollution below {currentObjective.maximumPollution}, before Month {currentObjective.deadlineMonth}";
    }
}
