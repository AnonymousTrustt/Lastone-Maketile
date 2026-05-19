using UnityEngine;
using UnityEngine.EventSystems;

public class BuildingPlacementManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InputProvider inputProvider;
    [SerializeField] private IsometricGridManager gridManager;
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private BuildingSupportManager buildingSupportManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform buildingParent;

    [Header("2D Preview")]
    [SerializeField] private Color validPreviewColor = new Color(0f, 1f, 0.2f, 0.55f);
    [SerializeField] private Color invalidPreviewColor = new Color(1f, 0.15f, 0.1f, 0.55f);
    [SerializeField] private int previewSortingOrderOffset = 5000;

    private BuildingDefinition selectedBuilding;
    private GameObject previewInstance;
    private Vector2Int currentPreviewCell;
    private bool canPlaceAtPreviewCell;

    private void Awake()
    {
        if (inputProvider == null)
        {
            inputProvider = FindFirstObjectByType<InputProvider>();
        }

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

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void OnEnable()
    {
        if (inputProvider != null)
        {
            inputProvider.LeftClicked += HandleLeftClick;
            inputProvider.RightClicked += HandleRightClick;
        }
    }

    private void OnDisable()
    {
        if (inputProvider != null)
        {
            inputProvider.LeftClicked -= HandleLeftClick;
            inputProvider.RightClicked -= HandleRightClick;
        }
    }

    private void Update()
    {
        if (selectedBuilding == null || previewInstance == null)
        {
            return;
        }

        UpdatePreviewPosition();
    }

    public void BeginPlacement(BuildingDefinition buildingDefinition)
    {
        if (buildingDefinition == null || buildingDefinition.prefab == null)
        {
            Debug.LogWarning("Cannot begin placement. Building definition or prefab is missing.");
            return;
        }

        CancelPlacement();

        selectedBuilding = buildingDefinition;
        previewInstance = Instantiate(buildingDefinition.prefab, buildingParent);
        DisablePreviewColliders(previewInstance);
        UpdatePreviewPosition();
    }

    public void CancelPlacement()
    {
        selectedBuilding = null;
        canPlaceAtPreviewCell = false;

        if (previewInstance != null)
        {
            Destroy(previewInstance);
            previewInstance = null;
        }
    }

    private void UpdatePreviewPosition()
    {
        if (!TryGetMouseWorldPosition(inputProvider.MousePosition, out Vector2 mouseWorldPosition))
        {
            return;
        }

        currentPreviewCell = gridManager.WorldToGrid(mouseWorldPosition);

        canPlaceAtPreviewCell = CanPlaceSelectedBuilding(currentPreviewCell);
        BuildingPlacementAlignment.ApplyPlacement(previewInstance, selectedBuilding, currentPreviewCell, gridManager, previewSortingOrderOffset);
        SpriteSortingUtility.ApplyColor(previewInstance, canPlaceAtPreviewCell ? validPreviewColor : invalidPreviewColor);
    }

    private bool CanPlaceSelectedBuilding(Vector2Int gridPosition)
    {
        if (selectedBuilding == null)
        {
            return false;
        }

        if (!gridManager.CanPlaceBuilding(gridPosition, BuildingPlacementAlignment.GetPlacementFootprint(selectedBuilding, gridManager)))
        {
            return false;
        }

        return resourceManager == null || resourceManager.HasEnoughMoney(selectedBuilding.cost);
    }

    private void HandleLeftClick(Vector2 mousePosition)
    {
        if (selectedBuilding == null)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        TryPlaceSelectedBuilding();
    }

    private void HandleRightClick(Vector2 mousePosition)
    {
        if (selectedBuilding != null)
        {
            CancelPlacement();
        }
    }

    private void TryPlaceSelectedBuilding()
    {
        if (!canPlaceAtPreviewCell || selectedBuilding == null)
        {
            return;
        }

        if (resourceManager != null && !resourceManager.SpendMoney(selectedBuilding.cost))
        {
            return;
        }

        PlacedBuilding placedBuilding = CreatePlacedBuilding(selectedBuilding, currentPreviewCell);

        if (placedBuilding == null)
        {
            return;
        }

        gridManager.MarkCellsOccupied(currentPreviewCell, BuildingPlacementAlignment.GetPlacementFootprint(selectedBuilding, gridManager), placedBuilding);
        RegisterBuildingSupport(placedBuilding);
        ApplyBuildingResourceEffects(selectedBuilding);
    }

    public PlacedBuilding PlaceLoadedBuilding(BuildingDefinition definition, Vector2Int gridPosition)
    {
        if (definition == null || definition.prefab == null)
        {
            return null;
        }

        Vector2Int placementFootprint = BuildingPlacementAlignment.GetPlacementFootprint(definition, gridManager);

        if (!gridManager.CanPlaceBuilding(gridPosition, placementFootprint))
        {
            Debug.LogWarning($"Skipped loading building '{definition.buildingId}' because its cells are occupied.");
            return null;
        }

        PlacedBuilding placedBuilding = CreatePlacedBuilding(definition, gridPosition);

        if (placedBuilding != null)
        {
            gridManager.MarkCellsOccupied(gridPosition, placementFootprint, placedBuilding);
            RegisterBuildingSupport(placedBuilding);
        }

        return placedBuilding;
    }

    private PlacedBuilding CreatePlacedBuilding(BuildingDefinition definition, Vector2Int gridPosition)
    {
        Vector3 worldPosition = gridManager.GetBuildingAnchorWorldPosition(gridPosition, definition);
        GameObject buildingObject = Instantiate(definition.prefab, worldPosition, Quaternion.identity, buildingParent);
        PlacedBuilding placedBuilding = buildingObject.GetComponent<PlacedBuilding>();

        if (placedBuilding == null)
        {
            placedBuilding = buildingObject.AddComponent<PlacedBuilding>();
        }

        placedBuilding.Initialize(definition, gridPosition, gridManager);
        return placedBuilding;
    }

    private void RegisterBuildingSupport(PlacedBuilding placedBuilding)
    {
        if (buildingSupportManager != null)
        {
            buildingSupportManager.RegisterBuilding(placedBuilding);
        }
    }

    private void ApplyBuildingResourceEffects(BuildingDefinition definition)
    {
        if (resourceManager == null)
        {
            return;
        }

        if (definition.energyChange >= 0)
        {
            resourceManager.AddEnergy(definition.energyChange);
        }
        else
        {
            resourceManager.UseEnergy(Mathf.Abs(definition.energyChange));
        }

        if (definition.pollutionChange >= 0)
        {
            resourceManager.AddPollution(definition.pollutionChange);
        }
        else
        {
            resourceManager.ReducePollution(Mathf.Abs(definition.pollutionChange));
        }

        if (definition.happinessChange >= 0)
        {
            resourceManager.AddHappiness(definition.happinessChange);
        }
        else
        {
            resourceManager.ReduceHappiness(Mathf.Abs(definition.happinessChange));
        }
    }

    private bool TryGetMouseWorldPosition(Vector2 mousePosition, out Vector2 worldPosition)
    {
        worldPosition = Vector2.zero;

        if (mainCamera == null)
        {
            return false;
        }

        float cameraDepth = Mathf.Abs(mainCamera.transform.position.z - gridManager.BaseZ);
        Vector3 screenPosition = new Vector3(mousePosition.x, mousePosition.y, cameraDepth);
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition = new Vector2(worldPoint.x, worldPoint.y);
        return true;
    }

    private void DisablePreviewColliders(GameObject previewObject)
    {
        foreach (Collider collider in previewObject.GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }

        foreach (Collider2D collider in previewObject.GetComponentsInChildren<Collider2D>())
        {
            collider.enabled = false;
        }
    }
}
