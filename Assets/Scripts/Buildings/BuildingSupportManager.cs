using System.Collections.Generic;
using UnityEngine;

public class BuildingSupportManager : MonoBehaviour
{
    private readonly List<PlacedBuilding> placedBuildings = new List<PlacedBuilding>();
    private readonly Dictionary<BuildingSupportType, int> supportProvided = new Dictionary<BuildingSupportType, int>();
    private readonly Dictionary<BuildingSupportType, int> supportRequired = new Dictionary<BuildingSupportType, int>();

    public void RegisterBuilding(PlacedBuilding building)
    {
        if (building == null || placedBuildings.Contains(building))
        {
            return;
        }

        placedBuildings.Add(building);
        RecalculateSupport();
    }

    public void UnregisterBuilding(PlacedBuilding building)
    {
        if (building == null)
        {
            return;
        }

        if (placedBuildings.Remove(building))
        {
            RecalculateSupport();
        }
    }

    public void RecalculateFromPlacedBuildings(List<PlacedBuilding> buildings)
    {
        placedBuildings.Clear();

        foreach (PlacedBuilding building in buildings)
        {
            if (building != null && building.Definition != null)
            {
                placedBuildings.Add(building);
            }
        }

        RecalculateSupport();
    }

    public int GetSupportProvided(BuildingSupportType supportType)
    {
        if (supportType == BuildingSupportType.None)
        {
            return 0;
        }

        return supportProvided.TryGetValue(supportType, out int amount) ? amount : 0;
    }

    public int GetSupportRequired(BuildingSupportType supportType)
    {
        if (supportType == BuildingSupportType.None)
        {
            return 0;
        }

        return supportRequired.TryGetValue(supportType, out int amount) ? amount : 0;
    }

    public int GetUnsupportedAmount(BuildingSupportType supportType)
    {
        return Mathf.Max(0, GetSupportRequired(supportType) - GetSupportProvided(supportType));
    }

    public bool HasEnoughSupport(BuildingSupportType supportType)
    {
        return GetUnsupportedAmount(supportType) == 0;
    }

    public void RecalculateSupport()
    {
        supportProvided.Clear();
        supportRequired.Clear();
        RemoveMissingBuildings();

        foreach (PlacedBuilding building in placedBuildings)
        {
            BuildingDefinition definition = building.Definition;
            if (definition == null)
            {
                continue;
            }

            if (definition.providesSupport && definition.providesSupportType != BuildingSupportType.None)
            {
                AddSupportValue(supportProvided, definition.providesSupportType, Mathf.Max(0, definition.supportCapacity));
            }

            if (definition.requiresSupport && definition.requiresSupportType != BuildingSupportType.None)
            {
                AddSupportValue(supportRequired, definition.requiresSupportType, Mathf.Max(0, definition.supportRequiredAmount));
            }
        }

        PrintSupportWarnings();
    }

    private void AddSupportValue(Dictionary<BuildingSupportType, int> values, BuildingSupportType supportType, int amount)
    {
        if (!values.ContainsKey(supportType))
        {
            values[supportType] = 0;
        }

        values[supportType] += amount;
    }

    private void RemoveMissingBuildings()
    {
        for (int i = placedBuildings.Count - 1; i >= 0; i--)
        {
            if (placedBuildings[i] == null || placedBuildings[i].Definition == null)
            {
                placedBuildings.RemoveAt(i);
            }
        }
    }

    private void PrintSupportWarnings()
    {
        foreach (KeyValuePair<BuildingSupportType, int> requirement in supportRequired)
        {
            BuildingSupportType supportType = requirement.Key;
            int required = requirement.Value;
            int provided = GetSupportProvided(supportType);
            int unsupported = Mathf.Max(0, required - provided);

            if (unsupported > 0)
            {
                Debug.LogWarning($"Support exceeded for {supportType}. Required: {required}, Provided: {provided}, Unsupported: {unsupported}.");
            }
        }
    }
}
