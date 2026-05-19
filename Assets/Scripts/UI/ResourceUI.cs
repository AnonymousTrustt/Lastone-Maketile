using TMPro;
using UnityEngine;

public class ResourceUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ResourceManager resourceManager;

    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI pollutionText;
    [SerializeField] private TextMeshProUGUI happinessText;

    private void Awake()
    {
        if (resourceManager == null)
        {
            resourceManager = FindFirstObjectByType<ResourceManager>();
        }
    }

    private void OnEnable()
    {
        if (resourceManager != null)
        {
            resourceManager.ResourceChanged += HandleResourceChanged;
            resourceManager.RefreshAllResourceEvents();
        }
    }

    private void OnDisable()
    {
        if (resourceManager != null)
        {
            resourceManager.ResourceChanged -= HandleResourceChanged;
        }
    }

    private void Start()
    {
        UpdateAllText();
    }

    private void HandleResourceChanged(ResourceType resourceType, int value, int cap, bool hasCap)
    {
        UpdateText(resourceType);
    }

    private void UpdateAllText()
    {
        UpdateText(ResourceType.Money);
        UpdateText(ResourceType.Energy);
        UpdateText(ResourceType.Pollution);
        UpdateText(ResourceType.Happiness);
    }

    private void UpdateText(ResourceType resourceType)
    {
        if (resourceManager == null)
        {
            return;
        }

        int value = resourceManager.GetResource(resourceType);
        int cap = resourceManager.GetCap(resourceType);
        bool hasCap = resourceManager.HasCap(resourceType);

        switch (resourceType)
        {
            case ResourceType.Money:
                SetText(moneyText, $"Money: ${value}");
                break;

            case ResourceType.Energy:
                SetText(energyText, hasCap ? $"Energy: {value} / {cap}" : $"Energy: {value}");
                break;

            case ResourceType.Pollution:
                SetText(pollutionText, hasCap ? $"Pollution: {value} / {cap}" : $"Pollution: {value}");
                break;

            case ResourceType.Happiness:
                SetText(happinessText, hasCap ? $"Happiness: {value} / {cap}" : $"Happiness: {value}");
                break;
        }
    }

    private void SetText(TextMeshProUGUI textField, string value)
    {
        if (textField != null)
        {
            textField.text = value;
        }
    }
}
