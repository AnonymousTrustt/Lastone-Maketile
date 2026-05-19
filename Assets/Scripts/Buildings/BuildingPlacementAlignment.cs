using UnityEngine;

public static class BuildingPlacementAlignment
{
    public static Vector2Int GetPlacementFootprint(BuildingDefinition definition, IsometricGridManager gridManager)
    {
        if (definition == null)
        {
            return Vector2Int.one;
        }

        Vector2Int explicitFootprint = ClampFootprint(definition.footprint);

        if (!definition.autoEstimateFootprint || explicitFootprint != Vector2Int.one || definition.prefab == null || gridManager == null)
        {
            return explicitFootprint;
        }

        Bounds visualBounds;
        if (!TryGetPrefabVisualBounds(definition.prefab, out visualBounds))
        {
            return explicitFootprint;
        }

        int widthCells = Mathf.Max(1, Mathf.CeilToInt(visualBounds.size.x / Mathf.Max(0.01f, gridManager.CellWidth)));

        if (widthCells <= 1)
        {
            return Vector2Int.one;
        }

        int depthCells = widthCells <= 2 ? 2 : Mathf.Max(1, Mathf.RoundToInt(widthCells * 0.5f));
        return new Vector2Int(widthCells, depthCells);
    }

    public static void ApplyPlacement(GameObject buildingObject, BuildingDefinition definition, Vector2Int gridPosition, IsometricGridManager gridManager, int sortingOrderOffset = 0)
    {
        if (buildingObject == null || gridManager == null)
        {
            return;
        }

        Vector2Int footprint = GetPlacementFootprint(definition, gridManager);
        buildingObject.transform.position = gridManager.GetBuildingAnchorWorldPosition(gridPosition, definition, footprint);
        AlignVisualsToAnchor(buildingObject, definition);

        Vector2Int sortingCell = gridManager.GetSortingCellForFootprint(gridPosition, footprint);
        int definitionSortingOffset = definition != null ? definition.sortingOrderOffset : 0;
        int sortingOrder = gridManager.GetSortingOrderForGridPosition(sortingCell, definitionSortingOffset + sortingOrderOffset);
        SpriteSortingUtility.ApplySortingOrder(buildingObject, sortingOrder);
    }

    public static void AlignVisualsToAnchor(GameObject buildingObject, BuildingDefinition definition)
    {
        if (buildingObject == null || definition == null || !definition.autoAlignSpriteToFootprint)
        {
            return;
        }

        SpriteRenderer[] spriteRenderers = buildingObject.GetComponentsInChildren<SpriteRenderer>();
        if (spriteRenderers.Length == 0)
        {
            return;
        }

        Bounds bounds = spriteRenderers[0].bounds;
        for (int i = 1; i < spriteRenderers.Length; i++)
        {
            bounds.Encapsulate(spriteRenderers[i].bounds);
        }

        Vector3 anchorPosition = buildingObject.transform.position;
        Vector3 visualBottomCenter = new Vector3(bounds.center.x, bounds.min.y, anchorPosition.z);
        Vector3 offset = anchorPosition - visualBottomCenter;
        offset.z = 0f;
        buildingObject.transform.position += offset;
    }

    private static Vector2Int ClampFootprint(Vector2Int footprint)
    {
        return new Vector2Int(Mathf.Max(1, footprint.x), Mathf.Max(1, footprint.y));
    }

    private static bool TryGetPrefabVisualBounds(GameObject prefab, out Bounds bounds)
    {
        bounds = new Bounds();

        if (prefab == null)
        {
            return false;
        }

        SpriteRenderer[] spriteRenderers = prefab.GetComponentsInChildren<SpriteRenderer>(true);
        if (spriteRenderers.Length == 0)
        {
            return false;
        }

        bounds = spriteRenderers[0].bounds;
        for (int i = 1; i < spriteRenderers.Length; i++)
        {
            bounds.Encapsulate(spriteRenderers[i].bounds);
        }

        return true;
    }
}
