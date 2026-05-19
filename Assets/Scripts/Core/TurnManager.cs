using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private IsometricGridManager gridManager;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private BuildingSupportManager buildingSupportManager;
    [SerializeField] private TextMeshProUGUI monthText;

    [Header("Turn Settings")]
    [SerializeField] private float secondsPerTurn = 5f;
    [SerializeField] private bool autoStart = true;

    private Coroutine turnLoopCoroutine;

    public int CurrentTurn { get; private set; } = 1;
    public bool IsRunning => turnLoopCoroutine != null;

    public event Action<int> TurnStarted;
    public event Action<int> TurnEnded;

    private void Awake()
    {
        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<IsometricGridManager>();
        }

        if (resourceManager == null)
        {
            resourceManager = FindFirstObjectByType<ResourceManager>();
        }

        if (buildingSupportManager == null)
        {
            buildingSupportManager = FindFirstObjectByType<BuildingSupportManager>();
        }
    }

    private void Start()
    {
        UpdateMonthText();

        if (autoStart)
        {
            StartTurns();
        }
    }

    public void StartTurns()
    {
        if (turnLoopCoroutine != null)
        {
            return;
        }

        turnLoopCoroutine = StartCoroutine(TurnLoop());
    }

    public void StopTurns()
    {
        if (turnLoopCoroutine == null)
        {
            return;
        }

        StopCoroutine(turnLoopCoroutine);
        turnLoopCoroutine = null;
    }

    public void AdvanceTurnNow()
    {
        ProcessTurn();
    }

    private IEnumerator TurnLoop()
    {
        WaitForSeconds wait = new WaitForSeconds(Mathf.Max(0.1f, secondsPerTurn));

        while (true)
        {
            yield return wait;
            ProcessTurn();
        }
    }

    private void ProcessTurn()
    {
        Debug.Log($"Turn started: Month {CurrentTurn}");
        TurnStarted?.Invoke(CurrentTurn);

        TurnResourceTotals totals = CalculateTurnResourceTotals();
        ApplyTurnResourceTotals(totals);
        RecalculateSupportAndWarnings();

        Debug.Log($"Month {CurrentTurn} totals - Money: {totals.money}, Energy: {totals.energy}, Pollution: {totals.pollution}, Happiness: {totals.happiness}");

        TurnEnded?.Invoke(CurrentTurn);
        CurrentTurn++;
        UpdateMonthText();
    }

    private TurnResourceTotals CalculateTurnResourceTotals()
    {
        TurnResourceTotals totals = new TurnResourceTotals();

        if (gridManager == null)
        {
            return totals;
        }

        List<PlacedBuilding> placedBuildings = gridManager.GetPlacedBuildings();

        foreach (PlacedBuilding building in placedBuildings)
        {
            if (building == null || building.Definition == null)
            {
                continue;
            }

            BuildingDefinition definition = building.Definition;
            totals.money += definition.moneyChange;
            totals.energy += definition.energyChange;
            totals.pollution += definition.pollutionChange;
            totals.happiness += definition.happinessChange;
        }

        return totals;
    }

    private void ApplyTurnResourceTotals(TurnResourceTotals totals)
    {
        if (resourceManager == null)
        {
            return;
        }

        resourceManager.ModifyResource(ResourceType.Money, totals.money);
        resourceManager.ModifyResource(ResourceType.Energy, totals.energy);
        resourceManager.ModifyResource(ResourceType.Pollution, totals.pollution);
        resourceManager.ModifyResource(ResourceType.Happiness, totals.happiness);
    }

    private void RecalculateSupportAndWarnings()
    {
        if (buildingSupportManager == null || gridManager == null)
        {
            return;
        }

        buildingSupportManager.RecalculateFromPlacedBuildings(gridManager.GetPlacedBuildings());

        int unsupportedAgriculture = buildingSupportManager.GetUnsupportedAmount(BuildingSupportType.Agriculture);
        if (unsupportedAgriculture > 0)
        {
            Debug.LogWarning($"Unsupported Agriculture demand this month: {unsupportedAgriculture}. Add more Agricultural Warehouses to support Farm Tiles.");
        }
    }

    private void UpdateMonthText()
    {
        if (monthText != null)
        {
            monthText.text = $"Month {CurrentTurn}";
        }
    }

    private struct TurnResourceTotals
    {
        public int money;
        public int energy;
        public int pollution;
        public int happiness;
    }
}
