using System.Collections.Generic;

public class ResourceSystem
{
    public Dictionary<ResourceType, int> Resources { get; private set; }
    public Dictionary<ResourceType, int> StorageSpace { get; private set; }
    private int defaultSpace;
    public ResourceSystem(int startResourcesAmount, int storageDefaultSpace,
    int credits)
    {
        defaultSpace = storageDefaultSpace;
        Resources = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Artifacts, 0},
            {ResourceType.Ore, 0},
            {ResourceType.Gas, 0},
            {ResourceType.Metal, startResourcesAmount},
            {ResourceType.Energy, startResourcesAmount},
            {ResourceType.Credits, credits},
        };
        StorageSpace = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Metal, defaultSpace},
            {ResourceType.Energy, defaultSpace},
        };
    }

    public bool UseResources(int amount, ResourceType type)
    {
        if (amount > Resources[type])
            return false;
        Resources[type] -= amount;
        return true;
    }

    public void UpdateStorageSpace(ResourceType type, bool increase) =>
        StorageSpace[type] += increase ? defaultSpace : -defaultSpace;

    public void CollectResources(int amount, ResourceType type)
    {
        if (type == ResourceType.Metal || type == ResourceType.Energy)
            if (Resources[type] >= StorageSpace[type])
                return;
        Resources[type] += amount;
    }
    public bool IsPossibleToConvert(int amount,
        ResourceType rawType, ResourceType type) =>
        Resources[type] < StorageSpace[type] && amount < Resources[rawType];
}
