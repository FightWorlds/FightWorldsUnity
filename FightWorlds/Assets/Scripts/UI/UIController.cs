using System;
using UnityEngine;
using UnityEngine.Events;
using FightWorlds.Placement;
using FightWorlds.Controllers;
using System.Collections.Generic;
using FightWorlds.Boost;

namespace FightWorlds.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private LeaderBoard leaderBoard;
        [SerializeField] private ActiveProcesses activeProcesses;
        [SerializeField] private BuildingsUnderAttack buildingsUnderAttack;
        [SerializeField] private EvacuationUI evacuationUI;
        [SerializeField] private BuildingMenuUI buildingMenu;
        [SerializeField] private UnitsMenu unitsMenu;
        [SerializeField] private PlayerManagementUI playerManagement;
        [SerializeField] private BoostsMap boosts;
        [SerializeField] private AttackManagementUI attackUI;
        [SerializeField] private GameObject buildingPanel;
        [SerializeField] private GameObject[] hideElements;

        private const int cloneLen = 7;

        public int CreditsDiv
        {
            get { return playerManagement.CreditsDiv; }
        }

        #region LeaderBoard
        public void UpdateLeaderBoard(int record) =>
            leaderBoard.UpdateBoard(record);
        #endregion

        #region ActiveProcesses
        public bool NewActiveProcess(GameObject obj, ProcessType type) =>
        activeProcesses.NewActiveProcess(obj, type);

        public void RemoveProcess(GameObject obj, bool defaultRemove) =>
            activeProcesses.RemoveProcess(obj, defaultRemove);

        public void AddRepairBot(Building bot) =>
            activeProcesses.AddRepairBot(bot);

        public void RemoveRepairBot(Building bot) =>
            activeProcesses.RemoveRepairBot(bot);
        #endregion

        #region BuildingsUnderAttack
        public void AddBuildUnderAttack(Building building) =>
            buildingsUnderAttack.AddBuildUnderAttack(building);

        public void RemoveFromUnderAttack(Building building) =>
            buildingsUnderAttack.RemoveFromUnderAttack(building);
        #endregion

        #region EvacuationUI

        public void SwitchButtonState(EvacuationState state, UnityAction act) =>
            evacuationUI.SwitchButtonState(state, act);

        public void ChangeTimeText(float time) =>
            evacuationUI.ChangeTimeText(time);

        public void UpdateBaseHpBar(float value, int spriteIndex) =>
            evacuationUI.UpdateBaseHpBar(value, spriteIndex);
        #endregion

        #region BuildingMenu
        public void ShowBuildingMenu(Building building) =>
                buildingMenu.ShowBuildingMenu(building);

        public void CloseBuildingMenu() => buildingMenu.CloseBuildingMenu();

        public void RotateBuilding() => buildingMenu.RotateBuilding();

        public void SwitchBuildingPanel(bool state)
        {
            buildingsUnderAttack.CollapseList();
            buildingPanel.SetActive(state);
        }
        public void SwitchBuildingPanel() =>
            buildingPanel.SetActive(!buildingPanel.activeSelf);
        #endregion

        #region PlayerManagementUI
        public void FillLevelUi(int level, int experience,
            int experienceNextLevel, bool isMaxLvl) =>
                playerManagement.FillLevelUi(level, experience,
                experienceNextLevel, isMaxLvl);

        public void FillResourcesUi(int ore, int gas,
        int metal, int energy, int credits, int artifacts) =>
            playerManagement.FillResourcesUi(ore, gas,
            metal, energy, credits, artifacts);

        public void FillVipUi(float mltpl) => playerManagement.FillVipUi(mltpl);

        public void ShowLevelUp(int level, int credits) =>
            playerManagement.ShowLevelUp(level, credits);

        public void ShowResourcePopUp(ResourceType type, int amount, Action action) => playerManagement.ShowResourcePopUp(type, amount, action);

        public void ShowLimitPopUp(string name) =>
            playerManagement.ShowLimitPopUp(name);

        public void ShowMaxLvlPopUp() =>
            playerManagement.ShowMaxLvlPopUp();

        public void FinishGamePopUp(int artifacts, UnityAction action) =>
            playerManagement.FinishGamePopUp(artifacts, action);
        #endregion

        #region UnitsMenu
        public void InitDockyard(Building building) =>
            unitsMenu.InitDockyard(building);

        public void RemoveDockyard() =>
            unitsMenu.RemoveDockyard();

        public void AddUnit() =>
            unitsMenu.AddUnit();

        public bool IsProducingUnits() =>
            unitsMenu.IsProducing;
        #endregion

        #region Boosts
        public bool LoadBoosts(BoostsSave save) => boosts.LoadBoosts(save);

        public BoostsSave SaveBoosts(bool isDefault) =>
            boosts.SaveBoosts(isDefault);

        public Dictionary<BoostType, int> GetActiveBoosts() =>
            boosts.ActiveBoosts;
        #endregion

        #region Attack
        public void ShowAttackCanvas()
        {
            gameObject.SetActive(false);
            attackUI.gameObject.SetActive(true);
        }

        public void PlaceHolder(Vector3 pos, bool isOnLand) =>
            attackUI.PlaceHolder(pos, isOnLand);

        #endregion

        public string CutClone(string name) =>
            name.Remove(name.Length - cloneLen);

        public void SwitchBoostsCanvas(bool turnOn)
        {
            SetDefaultLayout();
            boosts.transform.GetChild(0).gameObject.SetActive(turnOn);
        }

        public void SwitchUnitsCanvas(bool turnOn)
        {
            SetDefaultLayout();
            unitsMenu.transform.GetChild(0).gameObject.SetActive(turnOn);
        }

        public void HideMainCanvas()
        {
            foreach (Transform child in transform)
            {
                GameObject obj = child.gameObject;
                obj.SetActive(false);
            }
            return;
        }

        public void SetDefaultLayout()
        {
            // Show Processes List
            activeProcesses.transform.GetChild(1).gameObject.SetActive(true);
            foreach (var element in hideElements)
                element.SetActive(false);
        }
    }
}