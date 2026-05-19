using UnityEngine;

[CreateAssetMenu(fileName = "BuildingDefinition", menuName = "Threshold City 17/Building Definition")]
public class BuildingDefinition : ScriptableObject
{
    [Header("Identity")]
    public string buildingId = "building_id";
    public string buildingName = "New Building";
    public BuildingSector sectorType;

    [Header("Prefab")]
    public GameObject prefab;
    public Vector2Int footprint = Vector2Int.one;

    [Header("Placement Alignment")]
    [Tooltip("If enabled and Footprint is left at 1x1, placement can estimate a larger footprint from the prefab sprite width. For important buildings, set Footprint explicitly.")]
    public bool autoEstimateFootprint = true;

    [Tooltip("Most buildings should use OriginCellCenter so the visible base lands on the tile the player clicked. Footprint modes are for special large base decals.")]
    public BuildingAnchorMode anchorMode = BuildingAnchorMode.OriginCellCenter;

    [Tooltip("Optional final world-space nudge after automatic footprint alignment. Keep this at zero unless a specific asset has unusual artwork padding.")]
    public Vector2 placementOffset;

    [Tooltip("Automatically aligns the visible bottom-center of the prefab's sprites to the chosen grid anchor. This reduces problems from inconsistent sprite pivots.")]
    public bool autoAlignSpriteToFootprint = true;

    [Tooltip("Extra sorting offset for tall sprites that need to render above or below their base tile.")]
    public int sortingOrderOffset;

    [Header("Cost")]
    public int cost = 100;

    [Header("Resource Effects")]
    public int moneyChange;
    public int energyChange;
    public int pollutionChange;
    public int happinessChange;

    [Header("Support Provided")]
    public bool providesSupport;
    public BuildingSupportType providesSupportType = BuildingSupportType.None;
    public int supportCapacity;

    [Header("Support Required")]
    public bool requiresSupport;
    public BuildingSupportType requiresSupportType = BuildingSupportType.None;
    public int supportRequiredAmount = 1;
}
