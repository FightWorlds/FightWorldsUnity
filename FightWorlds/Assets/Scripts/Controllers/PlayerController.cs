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
    [SerializeField] private int[] experiencePerLevel;
    [SerializeField] private int startResourcesAmount;
    private LevelSystem levelSystem;
    private ResourceSystem resourceSystem;

    public event Action NewLevel;

    private void Awake()
    {
        levelSystem = new LevelSystem(experiencePerLevel);
        resourceSystem = new ResourceSystem(startResourcesAmount);
        FillLevelUi();
        FillResourcesUi();
    }

    public int GetArtifactsCount() => resourceSystem.Artifacts;

    public void TakeXp(int amount)
    {
        if (levelSystem.AddExperience(amount))
            NewLevel?.Invoke();
        FillLevelUi();
    }

    public bool UseResources(int amount)
    {
        // TODO enum for type of res
        bool possible = resourceSystem.UseResources(amount);
        if (!possible)
            return possible;
        FillResourcesUi();
        return possible;
    }

    public void TakeResources(int amount)
    {
        resourceSystem.CollectResources(amount);
        FillResourcesUi();
    }

    public void TakeArtifacts(int amount)
    {
        resourceSystem.CollectArtifacts(amount);
        FillResourcesUi();
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
        int resource = resourceSystem.Resources;
        int artifacts = resourceSystem.Artifacts;
        textArtifacts.text = $"Artifacts: {artifacts}";
        textResources.text = $"Metal: {resource}\n Ore: <unknown>";
    }
}
