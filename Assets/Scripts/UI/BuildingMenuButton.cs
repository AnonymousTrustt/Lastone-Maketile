using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class BuildingMenuButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI labelText;
    [SerializeField] private TooltipTrigger tooltipTrigger;

    private Button button;
    private BuildingDefinition buildingDefinition;
    private CityMenuUI cityMenuUI;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClicked);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClicked);
        }
    }

    public void Setup(BuildingDefinition definition, CityMenuUI owner)
    {
        buildingDefinition = definition;
        cityMenuUI = owner;

        if (labelText != null)
        {
            string buildingName = buildingDefinition != null ? buildingDefinition.buildingName : "Missing Building";
            int cost = buildingDefinition != null ? buildingDefinition.cost : 0;
            labelText.text = $"{buildingName}\n${cost}";
        }

        if (tooltipTrigger == null)
        {
            tooltipTrigger = GetComponent<TooltipTrigger>();
        }

        if (tooltipTrigger != null)
        {
            tooltipTrigger.SetBuildingDefinition(buildingDefinition);
        }
    }

    private void HandleClicked()
    {
        if (buildingDefinition == null || cityMenuUI == null)
        {
            Debug.LogWarning("Building menu button is missing its building definition or CityMenuUI reference.");
            return;
        }

        cityMenuUI.SelectBuilding(buildingDefinition);
    }
}
