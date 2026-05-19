using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TownStage
{
    Village,
    Town,
    City,
    EcoMetropolis
}

[Serializable]
public class TownStageData
{
    public TownStage stage = TownStage.Village;
    public string stageDisplayName = "Village";
    public int pollutionLimit = 100;
    public int happinessTarget = 100;
    public int requiredMoneyAmount = 500;
}

public class TownStageManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private TextMeshProUGUI stageText;

    [Header("Stages")]
    [SerializeField] private int currentStageIndex;
    [SerializeField] private List<TownStageData> stages = new List<TownStageData>
    {
        new TownStageData
        {
            stage = TownStage.Village,
            stageDisplayName = "Village",
            pollutionLimit = 100,
            happinessTarget = 100,
            requiredMoneyAmount = 500
        },
        new TownStageData
        {
            stage = TownStage.Town,
            stageDisplayName = "Town",
            pollutionLimit = 250,
            happinessTarget = 150,
            requiredMoneyAmount = 1500
        },
        new TownStageData
        {
            stage = TownStage.City,
            stageDisplayName = "City",
            pollutionLimit = 600,
            happinessTarget = 250,
            requiredMoneyAmount = 5000
        },
        new TownStageData
        {
            stage = TownStage.EcoMetropolis,
            stageDisplayName = "Eco Metropolis",
            pollutionLimit = 1000,
            happinessTarget = 400,
            requiredMoneyAmount = 15000
        }
    };

    public int CurrentLevel => currentStageIndex + 1;
    public TownStageData CurrentStage => GetStageByIndex(currentStageIndex);

    public event Action<TownStageData> StageChanged;
    public event Action<int> LevelChanged;

    private void Awake()
    {
        if (resourceManager == null)
        {
            resourceManager = FindFirstObjectByType<ResourceManager>();
        }
    }

    private void Start()
    {
        currentStageIndex = Mathf.Clamp(currentStageIndex, 0, Mathf.Max(0, stages.Count - 1));
        UpdateStageText();
        NotifyStageChanged();
    }

    public void AdvanceLevel()
    {
        AdvanceStage();
    }

    public void AdvanceStage()
    {
        if (currentStageIndex >= stages.Count - 1)
        {
            Debug.Log("Town is already at the final stage.");
            return;
        }

        currentStageIndex++;
        UpdateStageText();
        NotifyStageChanged();

        Debug.Log($"Town advanced to {CurrentStage.stageDisplayName}.");
    }

    public void SetLevel(int level)
    {
        currentStageIndex = Mathf.Clamp(level - 1, 0, Mathf.Max(0, stages.Count - 1));
        UpdateStageText();
        NotifyStageChanged();
    }

    public bool IsFinalStage()
    {
        return currentStageIndex >= stages.Count - 1;
    }

    public int GetPollutionCapForCurrentLevel()
    {
        return CurrentStage != null ? CurrentStage.pollutionLimit : 0;
    }

    public bool IsPollutionOverCurrentLimit()
    {
        if (resourceManager == null || CurrentStage == null)
        {
            return false;
        }

        return resourceManager.GetResource(ResourceType.Pollution) > CurrentStage.pollutionLimit;
    }

    public void CheckPollutionLimit()
    {
        if (!IsPollutionOverCurrentLimit())
        {
            return;
        }

        Debug.LogWarning($"Pollution is above the {CurrentStage.stageDisplayName} limit. Current: {resourceManager.GetResource(ResourceType.Pollution)}, Limit: {CurrentStage.pollutionLimit}.");
    }

    private TownStageData GetStageByIndex(int index)
    {
        if (stages == null || stages.Count == 0)
        {
            return null;
        }

        return stages[Mathf.Clamp(index, 0, stages.Count - 1)];
    }

    private void UpdateStageText()
    {
        if (stageText != null && CurrentStage != null)
        {
            stageText.text = $"Stage: {CurrentStage.stageDisplayName}";
        }
    }

    private void NotifyStageChanged()
    {
        StageChanged?.Invoke(CurrentStage);
        LevelChanged?.Invoke(CurrentLevel);
    }
}
