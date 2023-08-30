using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Text textLevel;
    [SerializeField] private Text textExperience;
    [SerializeField] private Text textResources;
    [SerializeField] private Slider sliderLevel;
    [SerializeField] private int[] experiencePerLevel;
    [SerializeField] private int startResourcesAmount;
    private LevelSystem levelSystem;
    private ResourceSystem resourceSystem;

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
        levelSystem.AddExperience(amount);
        FillLevelUi();
        // maybe change to events for less call amount 
        // (for example if player have max level)
    }

    public bool UseResources(int amount)
    {
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
        textExperience.text = $"{experience} / {experienceNextLevel}";
        sliderLevel.value = (float)experience / experienceNextLevel;
    }

    private void FillResourcesUi()
    {
        int resource = resourceSystem.Resources;
        int artifacts = resourceSystem.Artifacts;
        textResources.text = $"Resource: {resource}\n Artifacts: {artifacts}";
    }
}
