using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ResourceConfig
{
    public ResourceType resourceType;
    public int startingValue;
    public bool useCap;
    public int cap = 100;
}

public class ResourceManager : MonoBehaviour
{
    [SerializeField]
    private List<ResourceConfig> startingResources = new List<ResourceConfig>
    {
        new ResourceConfig { resourceType = ResourceType.Money, startingValue = 500, useCap = false },
        new ResourceConfig { resourceType = ResourceType.Energy, startingValue = 0, useCap = true, cap = 1000 },
        new ResourceConfig { resourceType = ResourceType.Pollution, startingValue = 0, useCap = true, cap = 1000 },
        new ResourceConfig { resourceType = ResourceType.Happiness, startingValue = 75, useCap = true, cap = 500 }
    };

    private readonly Dictionary<ResourceType, int> values = new Dictionary<ResourceType, int>();
    private readonly Dictionary<ResourceType, int> caps = new Dictionary<ResourceType, int>();
    private readonly HashSet<ResourceType> cappedResources = new HashSet<ResourceType>();

    public event Action<ResourceType, int, int, bool> ResourceChanged;

    private void Awake()
    {
        InitializeResources();
    }

    private void InitializeResources()
    {
        values.Clear();
        caps.Clear();
        cappedResources.Clear();

        foreach (ResourceConfig config in startingResources)
        {
            values[config.resourceType] = config.startingValue;
            caps[config.resourceType] = Mathf.Max(0, config.cap);

            if (config.useCap)
            {
                cappedResources.Add(config.resourceType);
            }

            values[config.resourceType] = ClampValue(config.resourceType, values[config.resourceType]);
        }
    }

    public int GetResource(ResourceType resourceType)
    {
        return values.TryGetValue(resourceType, out int value) ? value : 0;
    }

    public int GetCap(ResourceType resourceType)
    {
        return caps.TryGetValue(resourceType, out int cap) ? cap : 0;
    }

    public bool HasCap(ResourceType resourceType)
    {
        return cappedResources.Contains(resourceType);
    }

    public bool ModifyResource(ResourceType resourceType, int amount)
    {
        EnsureResourceExists(resourceType);

        int currentValue = values[resourceType];
        int newValue = ClampValue(resourceType, currentValue + amount);

        values[resourceType] = newValue;
        RaiseResourceChanged(resourceType);
        return true;
    }

    public bool TrySpendResource(ResourceType resourceType, int amount)
    {
        EnsureResourceExists(resourceType);

        if (amount < 0)
        {
            return ModifyResource(resourceType, -amount);
        }

        if (values[resourceType] < amount)
        {
            return false;
        }

        values[resourceType] -= amount;
        RaiseResourceChanged(resourceType);
        return true;
    }

    public void SetResourceValue(ResourceType resourceType, int value)
    {
        EnsureResourceExists(resourceType);
        values[resourceType] = ClampValue(resourceType, value);
        RaiseResourceChanged(resourceType);
    }

    public void SetResourceCap(ResourceType resourceType, int cap, bool useCap = true)
    {
        EnsureResourceExists(resourceType);

        caps[resourceType] = Mathf.Max(0, cap);

        if (useCap)
        {
            cappedResources.Add(resourceType);
        }
        else
        {
            cappedResources.Remove(resourceType);
        }

        values[resourceType] = ClampValue(resourceType, values[resourceType]);
        RaiseResourceChanged(resourceType);
    }

    public void AddMoney(int amount) => ModifyResource(ResourceType.Money, amount);
    public bool SpendMoney(int amount) => TrySpendResource(ResourceType.Money, amount);
    public bool HasEnoughMoney(int amount) => GetResource(ResourceType.Money) >= amount;

    public void AddEnergy(int amount) => ModifyResource(ResourceType.Energy, amount);
    public bool UseEnergy(int amount) => TrySpendResource(ResourceType.Energy, amount);

    public void AddPollution(int amount) => ModifyResource(ResourceType.Pollution, amount);
    public void ReducePollution(int amount) => ModifyResource(ResourceType.Pollution, -amount);

    public void AddHappiness(int amount) => ModifyResource(ResourceType.Happiness, amount);
    public void ReduceHappiness(int amount) => ModifyResource(ResourceType.Happiness, -amount);

    public void RefreshAllResourceEvents()
    {
        foreach (ResourceType resourceType in values.Keys)
        {
            RaiseResourceChanged(resourceType);
        }
    }

    private void EnsureResourceExists(ResourceType resourceType)
    {
        if (!values.ContainsKey(resourceType))
        {
            values[resourceType] = 0;
            caps[resourceType] = 0;
        }
    }

    private int ClampValue(ResourceType resourceType, int value)
    {
        value = Mathf.Max(0, value);

        if (cappedResources.Contains(resourceType))
        {
            value = Mathf.Min(value, GetCap(resourceType));
        }

        return value;
    }

    private void RaiseResourceChanged(ResourceType resourceType)
    {
        ResourceChanged?.Invoke(resourceType, GetResource(resourceType), GetCap(resourceType), HasCap(resourceType));
    }
}
