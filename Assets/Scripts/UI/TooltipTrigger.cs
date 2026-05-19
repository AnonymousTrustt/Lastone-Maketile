using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("Tooltip Text")]
    [SerializeField] private string title;
    [TextArea(2, 6)]
    [SerializeField] private string description;

    [Header("Optional Building")]
    [SerializeField] private BuildingDefinition buildingDefinition;

    [Header("Optional Reference")]
    [SerializeField] private TooltipUI tooltipUI;

    private bool isHovering;

    private void Awake()
    {
        if (tooltipUI == null)
        {
            tooltipUI = TooltipUI.Instance;
        }
    }

    private void OnDisable()
    {
        if (isHovering)
        {
            HideTooltip();
        }
    }

    public void SetText(string tooltipTitle, string tooltipDescription)
    {
        title = tooltipTitle;
        description = tooltipDescription;
        buildingDefinition = null;
    }

    public void SetBuildingDefinition(BuildingDefinition definition)
    {
        buildingDefinition = definition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        ShowTooltip(eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (isHovering)
        {
            ShowTooltip(eventData.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void ShowTooltip(Vector2 screenPosition)
    {
        if (tooltipUI == null)
        {
            tooltipUI = TooltipUI.Instance;
        }

        if (tooltipUI == null)
        {
            return;
        }

        if (buildingDefinition != null)
        {
            tooltipUI.Show(buildingDefinition.buildingName, BuildBuildingTooltipBody(buildingDefinition), screenPosition);
            return;
        }

        tooltipUI.Show(title, description, screenPosition);
    }

    private void HideTooltip()
    {
        isHovering = false;

        if (tooltipUI == null)
        {
            tooltipUI = TooltipUI.Instance;
        }

        if (tooltipUI != null)
        {
            tooltipUI.Hide();
        }
    }

    private string BuildBuildingTooltipBody(BuildingDefinition definition)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine($"Cost: {definition.cost}");
        builder.AppendLine($"Energy: {FormatChange(definition.energyChange)}");
        builder.AppendLine($"Pollution: {FormatChange(definition.pollutionChange)}");
        builder.AppendLine($"Happiness: {FormatChange(definition.happinessChange)}");
        builder.AppendLine($"Sector: {FormatSector(definition.sectorType)}");

        if (definition.providesSupport && definition.providesSupportType != BuildingSupportType.None)
        {
            builder.AppendLine($"Provides: {definition.supportCapacity} {definition.providesSupportType} support");
        }

        if (definition.requiresSupport && definition.requiresSupportType != BuildingSupportType.None)
        {
            builder.AppendLine($"Requires: {definition.supportRequiredAmount} {definition.requiresSupportType} support");
        }

        builder.Append($"Effect: {BuildEffectText(definition)}");
        return builder.ToString();
    }

    private string FormatChange(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private string FormatSector(BuildingSector sector)
    {
        switch (sector)
        {
            case BuildingSector.ResidentialCommercial:
                return "Residential / Commercial";
            case BuildingSector.EnvironmentalMitigation:
                return "Environment";
            default:
                return sector.ToString();
        }
    }

    private string BuildEffectText(BuildingDefinition definition)
    {
        if (definition.pollutionChange < 0)
        {
            return "Reduces pollution and helps keep the city below environmental thresholds.";
        }

        if (definition.happinessChange > 0 && definition.energyChange < 0)
        {
            return "Improves city life, but increases energy demand.";
        }

        if (definition.energyChange > 0 && definition.pollutionChange > 0)
        {
            return "Produces energy, but increases pollution.";
        }

        if (definition.energyChange > 0)
        {
            return "Adds cleaner energy capacity for city growth.";
        }

        if (definition.moneyChange > 0)
        {
            return "Improves the city economy.";
        }

        if (definition.requiresSupport)
        {
            return "Needs supporting infrastructure to work properly.";
        }

        return "Changes city resources when placed and during monthly simulation.";
    }
}
