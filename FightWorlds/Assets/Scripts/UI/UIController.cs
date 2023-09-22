using System;
using UnityEngine;
using UnityEngine.Events;
using FightWorlds.Placement;
using FightWorlds.Controllers;

namespace FightWorlds.UI
{
    public class UIController : MonoBehaviour
    {
        [SerializeField] private LeaderBoard leaderBoard;
        [SerializeField] private ActiveProcesses activeProcesses;
        [SerializeField] private BuildingsUnderAttack buildingsUnderAttack;
        [SerializeField] private EvacuationUI evacuationUI;
        [SerializeField] private BuildingMenuUI buildingMenu;
        [SerializeField] private PlayerManagementUI playerManagement;

        private const int cloneLen = 7;
        // TODO single open panel (other should close)

        public int CreditsDiv
        {
            get { return playerManagement.CreditsDiv; }
        }

        #region LeaderBoard
        public void UpdateLeaderBoard(int record) =>
            leaderBoard.UpdateBoard(record);
        #endregion

        #region ActiveProcesses
        public void NewActiveProcess(GameObject obj, ProcessType type) =>
        activeProcesses.NewActiveProcess(obj, type);

        public void RemoveProcess(GameObject obj) =>
            activeProcesses.RemoveProcess(obj);

        public bool IsProcessesFulled() => activeProcesses.IsProcessesFulled();
        #endregion

        #region BuildingsUnderAttack
        public void AddBuildUnderAttack(Building building) =>
            buildingsUnderAttack.AddBuildUnderAttack(building);

        public void RemoveFromUnderAttack(Building building) =>
            buildingsUnderAttack.RemoveFromUnderAttack(building);
        #endregion

        #region EvacuationUI

        public void AddListenerOnRestart(UnityAction act) =>
            evacuationUI.AddListenerOnRestart(act);

        public void AddListenerOnCall(UnityAction act) =>
            evacuationUI.AddListenerOnCall(act);

        public void AddListenerOnUp(UnityAction act) =>
            evacuationUI.AddListenerOnUp(act);

        public void SwitchCallButtonState(bool value) =>
            evacuationUI.SwitchCallButtonState(value);

        public void SwitchEvacuationButtonState(bool value) =>
            evacuationUI.SwitchEvacuationButtonState(value);

        public void SwitchEvacuationTimerState(bool value) =>
            evacuationUI.SwitchEvacuationTimerState(value);

        public void ChangeTimeText(float time) =>
            evacuationUI.ChangeTimeText(time);

        public void UpdateBaseHpBar(float value) =>
            evacuationUI.UpdateBaseHpBar(value);
        #endregion

        #region BuildingMenu
        public void ShowBuildingMenu(Building building) =>
                buildingMenu.ShowBuildingMenu(building);

        public void CloseBuildingMenu() => buildingMenu.CloseBuildingMenu();

        public void RotateBuilding() => buildingMenu.RotateBuilding();
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

        public void FinishGamePopUp(int artifacts) =>
            playerManagement.FinishGamePopUp(artifacts);
        #endregion

        public string CutClone(string name) =>
            name.Remove(name.Length - cloneLen);
    }
}