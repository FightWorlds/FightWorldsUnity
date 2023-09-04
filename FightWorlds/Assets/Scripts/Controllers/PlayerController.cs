using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Text textLevel;
    [SerializeField] private Text textExperience;
    [SerializeField] private Text textResources;
    [SerializeField] private Text textEnergy;
    [SerializeField] private Text textArtifacts;
    [SerializeField] private Slider sliderLevel;
    [SerializeField] private int startResourcesAmount;
    [SerializeField] private int defaultStorageSize;
    private LevelSystem levelSystem;
    private ResourceSystem resourceSystem;

    public event Action NewLevel;

    private void Awake()
    {
        levelSystem = new LevelSystem();
        resourceSystem =
            new ResourceSystem(startResourcesAmount, defaultStorageSize);
        FillLevelUi();
        FillResourcesUi();
        FillArtifactsUi();
    }

    public int GetArtifactsCount() => resourceSystem.Artifacts;

    public void TakeXp(int amount)
    {
        if (levelSystem.AddExperience(amount))
            NewLevel?.Invoke();
        FillLevelUi();
    }

    public bool IsPossibleToConvert(int amount,
        ResourceType rawType, ResourceType type) =>
        resourceSystem.IsPossibleToConvert(amount, rawType, type);


    public bool UseResources(int amount, ResourceType type)
    {
        bool possible = resourceSystem.UseResources(amount, type);
        if (!possible)
            return possible;
        FillResourcesUi();
        return possible;
    }

    public void TakeResources(int amount, ResourceType type)
    {
        resourceSystem.CollectResources(amount, type);
        FillResourcesUi();
    }

    public void TakeArtifacts(int amount)
    {
        resourceSystem.CollectArtifacts(amount);
        FillArtifactsUi();
    }

    public void NewStorage(ResourceType type) =>
        resourceSystem.UpdateStorageSpace(type, true);

    public void DestroyStorage(ResourceType type)
    {
        resourceSystem.UpdateStorageSpace(type, false);
        resourceSystem.UseResources(defaultStorageSize, type);
    }

    private void FillLevelUi()
    {
        int level = levelSystem.Level;
        int experience = levelSystem.Experience;
        int experienceNextLevel = levelSystem.NextLevelExperience;
        textLevel.text = $"Level: {level}";
        textExperience.text = levelSystem.IsMaxLvl() ?
        $"{experienceNextLevel} / {experienceNextLevel}" :
        $"{experience} / {experienceNextLevel}";
        sliderLevel.value = levelSystem.IsMaxLvl() ? 1 :
            (float)experience / experienceNextLevel;
    }

    private void FillResourcesUi()
    {
        int ore = resourceSystem.Resources[ResourceType.Ore];
        int gas = resourceSystem.Resources[ResourceType.Gas];
        int metal = resourceSystem.Resources[ResourceType.Metal];
        int energy = resourceSystem.Resources[ResourceType.Energy];
        textResources.text = $"Metal: {metal}\n Ore: {ore}";
        textEnergy.text = $"Energy: {energy}\n Gas: {gas}";
    }

    private void FillArtifactsUi() =>
        textArtifacts.text = $"Artifacts: {resourceSystem.Artifacts}";
}
