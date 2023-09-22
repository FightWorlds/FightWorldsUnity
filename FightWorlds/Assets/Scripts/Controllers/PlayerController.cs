using System;
using UnityEngine;
using FightWorlds.UI;

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
        private ResourceSystem resourceSystem;
        public float VipMultiplier { get; private set; }
        public PlayerInfo Info { get; private set; }
        public event Action NewLevel;

        public PlayerController(UIController ui)
        {
            Info = SaveManager.Load();
            levelSystem = new LevelSystem(Info);
            resourceSystem = new ResourceSystem(startResourcesAmount,
            defaultStorageSize, Info);
            VipMultiplier = vip;
            this.ui = ui;
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
                ui.ShowLevelUp(Level(), credits);
                resourceSystem.CollectResources(credits, ResourceType.Credits);
            }
            FillLevelUi();
        }

        public bool IsPossibleToConvert(int amount,
            ResourceType rawType, ResourceType type) =>
            resourceSystem.IsPossibleToConvert(amount, rawType, type);


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
            resourceSystem.CollectResources((int)(amount * VipMultiplier), type);
            FillResourcesUi();
        }

        public void NewStorage(ResourceType type) =>
            resourceSystem.UpdateStorageSpace(type, true);

        public void DestroyStorage(ResourceType type)
        {
            resourceSystem.UpdateStorageSpace(type, false);
            resourceSystem.UseResources(defaultStorageSize, type);
        }

        public void SavePlayerResult(int record)
        {
            int newRecord = record > Info.Record ? record : Info.Record;
            SaveManager.Save(new(levelSystem.Level, levelSystem.Experience,
                resourceSystem.Resources[ResourceType.Credits], newRecord));
        }

        public void RegularSave()
        {
            SaveManager.Save(new(levelSystem.Level, levelSystem.Experience,
                resourceSystem.Resources[ResourceType.Credits], Info.Record));
        }

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