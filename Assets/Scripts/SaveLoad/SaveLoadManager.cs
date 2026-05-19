using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ResourceSaveData
{
    public ResourceType resourceType;
    public int value;
}

[Serializable]
public class BuildingSaveData
{
    public string buildingId;
    public int gridX;
    public int gridY;
}

[Serializable]
public class GameSaveData
{
    public int currentLevel;
    public List<ResourceSaveData> resources = new List<ResourceSaveData>();
    public List<BuildingSaveData> placedBuildings = new List<BuildingSaveData>();
}

public class SaveLoadManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private TownStageManager townStageManager;
    [SerializeField] private IsometricGridManager gridManager;
    [SerializeField] private BuildingPlacementManager buildingPlacementManager;

    [Header("Building Catalog")]
    [SerializeField] private BuildingDefinition[] buildingCatalog;

    [Header("Save File")]
    [SerializeField] private string saveFileName = "threshold_city_17_save.json";

    public string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        if (resourceManager == null)
        {
            resourceManager = FindFirstObjectByType<ResourceManager>();
        }

        if (townStageManager == null)
        {
            townStageManager = FindFirstObjectByType<TownStageManager>();
        }

        if (gridManager == null)
        {
            gridManager = FindFirstObjectByType<IsometricGridManager>();
        }

        if (buildingPlacementManager == null)
        {
            buildingPlacementManager = FindFirstObjectByType<BuildingPlacementManager>();
        }
    }

    public void SaveGame()
    {
        GameSaveData saveData = new GameSaveData
        {
            currentLevel = townStageManager != null ? townStageManager.CurrentLevel : 1
        };

        SaveResource(saveData, ResourceType.Money);
        SaveResource(saveData, ResourceType.Energy);
        SaveResource(saveData, ResourceType.Pollution);
        SaveResource(saveData, ResourceType.Happiness);
        SavePlacedBuildings(saveData);

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game saved to: {SavePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"No save file found at: {SavePath}");
            return;
        }

        string json = File.ReadAllText(SavePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);

        if (saveData == null)
        {
            Debug.LogWarning("Save file could not be read.");
            return;
        }

        if (gridManager != null)
        {
            gridManager.ClearAllPlacedBuildings(true);
        }

        if (townStageManager != null)
        {
            townStageManager.SetLevel(saveData.currentLevel);
        }

        LoadResources(saveData);
        LoadPlacedBuildings(saveData);
        Debug.Log("Game loaded.");
    }

    public void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save file deleted.");
        }
    }

    private void SaveResource(GameSaveData saveData, ResourceType resourceType)
    {
        if (resourceManager == null)
        {
            return;
        }

        saveData.resources.Add(new ResourceSaveData
        {
            resourceType = resourceType,
            value = resourceManager.GetResource(resourceType)
        });
    }

    private void SavePlacedBuildings(GameSaveData saveData)
    {
        if (gridManager == null)
        {
            return;
        }

        foreach (PlacedBuilding building in gridManager.GetPlacedBuildings())
        {
            if (building == null || building.Definition == null)
            {
                continue;
            }

            saveData.placedBuildings.Add(new BuildingSaveData
            {
                buildingId = building.Definition.buildingId,
                gridX = building.GridPosition.x,
                gridY = building.GridPosition.y
            });
        }
    }

    private void LoadResources(GameSaveData saveData)
    {
        if (resourceManager == null)
        {
            return;
        }

        foreach (ResourceSaveData resourceData in saveData.resources)
        {
            resourceManager.SetResourceValue(resourceData.resourceType, resourceData.value);
        }
    }

    private void LoadPlacedBuildings(GameSaveData saveData)
    {
        if (buildingPlacementManager == null)
        {
            return;
        }

        foreach (BuildingSaveData buildingData in saveData.placedBuildings)
        {
            BuildingDefinition definition = FindBuildingDefinition(buildingData.buildingId);

            if (definition == null)
            {
                Debug.LogWarning($"Could not load building. Missing definition with id: {buildingData.buildingId}");
                continue;
            }

            buildingPlacementManager.PlaceLoadedBuilding(definition, new Vector2Int(buildingData.gridX, buildingData.gridY));
        }
    }

    private BuildingDefinition FindBuildingDefinition(string buildingId)
    {
        foreach (BuildingDefinition definition in buildingCatalog)
        {
            if (definition != null && definition.buildingId == buildingId)
            {
                return definition;
            }
        }

        return null;
    }
}
