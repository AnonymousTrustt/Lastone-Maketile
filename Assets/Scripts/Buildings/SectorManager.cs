using UnityEngine;

public class SectorManager : MonoBehaviour
{
    public bool CanAddBuildingToSector(BuildingSector sectorType)
    {
        // Future home for ratio rules, scene-specific sector limits, and SDG objective checks.
        // Example later: 1 Industry building can support 4 Residential/Commercial buildings.
        return true;
    }
}