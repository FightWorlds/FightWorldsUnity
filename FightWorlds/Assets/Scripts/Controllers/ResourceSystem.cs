public class ResourceSystem
{
    public int Artifacts { get; private set; }
    public int Resources { get; private set; }
    public ResourceSystem(int startResourcesAmount)
    {
        Artifacts = 0;
        Resources = startResourcesAmount;
    }

    public bool UseResources(int amount)
    {
        if (amount > Resources)
            return false;
        Resources -= amount;
        return true;
    }

    public void CollectResources(int amount) => Resources += amount;
    public void CollectArtifacts(int amount) => Artifacts += amount;
}
