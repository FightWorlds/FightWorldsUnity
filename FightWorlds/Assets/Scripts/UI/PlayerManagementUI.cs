using System;
using FightWorlds.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FightWorlds.UI
{
    public class PlayerManagementUI : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private TextMeshProUGUI textLevel;
        [SerializeField] private TextMeshProUGUI textExperience;
        [SerializeField] private TextMeshProUGUI textVip;
        [Header("Resources")]
        [SerializeField] private TextMeshProUGUI textOre;
        [SerializeField] private TextMeshProUGUI textGas;
        [SerializeField] private TextMeshProUGUI textMetal;
        [SerializeField] private TextMeshProUGUI textEnergy;
        [SerializeField] private TextMeshProUGUI textArtifacts;
        [SerializeField] private TextMeshProUGUI textCredits;
        [Header("PopUp")]
        [SerializeField] private TextMeshProUGUI textPopUp;
        [SerializeField] private GameObject popUp;
        [SerializeField] private Button agreeButton;
        [Header("LevelUp")]
        [SerializeField] private GameObject levelUpPopUp;
        [SerializeField] private Text textLevelPopUp;
        [SerializeField] private Text headerLevelPopUp;
        [SerializeField] private TextMeshProUGUI descriptionLevelPopUp;
        [SerializeField] private Button claimAgreeButton;

        public readonly int CreditsDiv = 1000;

        public void FillLevelUi(int level, int experience,
        int experienceNextLevel, bool isMaxLvl)
        {
            textLevel.text = $"LVL {level}";
            textExperience.text = isMaxLvl ? "100 / 100%" :
                $"{GetCurrentPercent(experience, experienceNextLevel)} / 100%";
        }

        public void FillResourcesUi(int ore, int gas,
        int metal, int energy, int credits, int artifacts)
        {
            textOre.text = ore.ToString();
            textGas.text = gas.ToString();
            textMetal.text = metal.ToString();
            textEnergy.text = energy.ToString();
            textCredits.text = credits.ToString();
            textArtifacts.text = artifacts.ToString();
        }

        public void FillVipUi(float mltpl) =>
            textVip.text = $"VIP {(mltpl - 1) * 10}";

        public void ShowLevelUp(int level, int credits)
        {
            textLevelPopUp.text = level.ToString();
            descriptionLevelPopUp.text =
            $"You reached LVL {level}\nCredits: {credits}";
            levelUpPopUp.SetActive(true);
        }

        public void ShowResourcePopUp(ResourceType type,
        int amount, Action action)
        {
            int require = Mathf.CeilToInt((float)amount / CreditsDiv);
            string text = "There is no enough ";
            if (type == ResourceType.Credits)
                text += "credits";
            else
                text += $"{type}\n" +
                "Would you like instant Build/Repair/Upgrade for\n" +
                $"<{require}> credits";
            textPopUp.text = text;
            agreeButton.onClick.RemoveAllListeners();
            if (action != null)
                agreeButton.onClick.AddListener(() =>
                {
                    popUp.SetActive(false);
                    action();
                });
            popUp.SetActive(true);
        }

        public void ShowBotsPopUp(int amount, Action action)
        {
            textPopUp.text = "There is no enough Bots" +
                "\nWould you like buy one for\n" +
                $"<{amount}> credits";
            agreeButton.onClick.RemoveAllListeners();
            agreeButton.onClick.AddListener(() =>
            {
                popUp.SetActive(false);
                action();
            });
            popUp.SetActive(true);
        }

        public void ShowLimitPopUp(string name)
        {
            textPopUp.text = "You reached limit of" +
                $"\n{name}s\n" +
                $"Level up to build more!";
            agreeButton.onClick.RemoveAllListeners();
            agreeButton.onClick.AddListener(() => popUp.SetActive(false));
            popUp.SetActive(true);
        }

        public void ShowMaxLvlPopUp()
        {
            textPopUp.text = "You reached max upgrade level" +
                $"\nfor selected building";
            agreeButton.onClick.RemoveAllListeners();
            agreeButton.onClick.AddListener(() => popUp.SetActive(false));
            popUp.SetActive(true);
        }

        public void FinishGamePopUp(int artifacts, UnityAction action)
        {
            levelUpPopUp.SetActive(true);
            textLevelPopUp.text = "";
            headerLevelPopUp.text = "FINISH";
            descriptionLevelPopUp.text =
                $"Session result:\n{artifacts} artifacts";
            claimAgreeButton.onClick.AddListener(action);
        }

        private int GetCurrentPercent(int xp, int nextXp) =>
            Mathf.CeilToInt(xp / (float)nextXp * 100);
    }
}