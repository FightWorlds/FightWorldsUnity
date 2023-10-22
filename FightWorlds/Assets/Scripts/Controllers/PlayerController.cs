using System;
using UnityEngine;
using FightWorlds.UI;
using System.Collections.Generic;
using FightWorlds.Boost;

namespace FightWorlds.Controllers
{
    public class PlayerController
    {
        private UIController ui;
        private const int creditsMultiplier = 10;
        private const int startResourcesAmount = 800;
        private const int defaultStorageSize = 2000;
        private const float vip = 1f;
        private LevelSystem levelSystem;

        public ResourceSystem resourceSystem { get; private set; }
        public int UnitsLevel { get; private set; }
        public int BotsAmount { get; private set; }
        public float VipMultiplier { get; private set; }
        public PlayerInfo Info { get; private set; }
        public event Action NewLevel;

        public PlayerController(UIController ui)
        {
            Info = SaveManager.Load();
            levelSystem = new(Info);
            resourceSystem =
                new(startResourcesAmount, defaultStorageSize, Info);
            BotsAmount = Info.Bots;
            UnitsLevel = Info.UnitsLevel;
            VipMultiplier = vip;
            this.ui = ui;
            if (!ui.LoadBoosts(JsonUtility.FromJson<BoostsSave>(Info.Boosts)))
                ui.SaveBoosts(true);
            FillLevelUi();
            FillResourcesUi();
            FillVipUi();
            FillLeaderBoardUi();
        }

        public int Level() => levelSystem.Level;

        public void TakeXp(int amount)
        {
            if (levelSystem.AddExperience((int)(amount * VipMultiplier)))
            {
                NewLevel?.Invoke();
                int credits = Level() * creditsMultiplier;
                ui.SetDefaultLayout();
                ui.ShowLevelUp(Level(), credits);
                resourceSystem.CollectResources(credits, ResourceType.Credits);
            }
            FillLevelUi();
        }

        public bool IsPossibleToConvert(int amount,
            ResourceType rawType, ResourceType type) =>
            resourceSystem.IsPossibleToConvert(amount, rawType, type);

        public bool CanUseResources(KeyValuePair<ResourceType, int>[] resources)
        => resourceSystem.CanUseResources(resources);

        public bool UseResources(int amount, ResourceType type,
        bool needPopUp, Action callback = null)
        {
            bool possible = resourceSystem.UseResources(amount, type);
            if (!possible)
            {
                if (needPopUp)
                    ui.ShowResourcePopUp(type, amount, callback == null ? null :
                    () =>
                    {
                        if (resourceSystem.UseResources(
                        Mathf.CeilToInt((float)amount / ui.CreditsDiv),
                        ResourceType.Credits))
                        {
                            callback();
                            FillResourcesUi();
                        }
                    });
                return possible;
            }
            FillResourcesUi();
            return possible;
        }

        public void TakeResources(int amount, ResourceType type)
        {
            resourceSystem.CollectResources(
                (int)(amount * VipMultiplier), type);
            FillResourcesUi();
        }

        public void NewStorage(ResourceType type) =>
            resourceSystem.UpdateStorageSpace(type, true);

        public void DestroyStorage(ResourceType type)
        {
            resourceSystem.UpdateStorageSpace(type, false);
            resourceSystem.UseResources(defaultStorageSize, type);
        }

        public void SavePlayerResult(int artifacts)
        {
            SaveManager.Save(
                new(levelSystem.Level, levelSystem.Experience,
                resourceSystem.Resources[ResourceType.Credits],
                resourceSystem.Resources[ResourceType.TotalArtifacts] +
                artifacts, Info.Record + artifacts, BotsAmount,
                resourceSystem.Resources[ResourceType.Units],
                resourceSystem.Resources[ResourceType.UnitsToHeal],
                UnitsLevel, JsonUtility.ToJson(ui.SaveBoosts(false))));
        }

        public void RegularSave()
        {
            SaveManager.Save(
                new(levelSystem.Level, levelSystem.Experience,
                resourceSystem.Resources[ResourceType.Credits],
                Info.Artifacts, Info.Record, BotsAmount,
                resourceSystem.Resources[ResourceType.Units],
                resourceSystem.Resources[ResourceType.UnitsToHeal],
                UnitsLevel, JsonUtility.ToJson(ui.SaveBoosts(false))));
        }

        public void AddBots() => BotsAmount++;
        public void UnitsNewLevel() => UnitsLevel++;

        private void FillLevelUi()
        {
            int level = levelSystem.Level;
            int experience = levelSystem.Experience;
            int experienceNextLevel = levelSystem.NextLevelExperience;
            ui.FillLevelUi(level, experience,
            experienceNextLevel, levelSystem.IsMaxLvl());
        }

        private void FillResourcesUi()
        {
            int ore = resourceSystem.Resources[ResourceType.Ore];
            int gas = resourceSystem.Resources[ResourceType.Gas];
            int metal = resourceSystem.Resources[ResourceType.Metal];
            int energy = resourceSystem.Resources[ResourceType.Energy];
            int credits = resourceSystem.Resources[ResourceType.Credits];
            int artifacts = resourceSystem.Resources[ResourceType.Artifacts];
            ui.FillResourcesUi(ore, gas, metal, energy, credits, artifacts);
        }

        private void FillVipUi() =>
            ui.FillVipUi(VipMultiplier);

        private void FillLeaderBoardUi() =>
            ui.UpdateLeaderBoard(Info.Record);
    }
}