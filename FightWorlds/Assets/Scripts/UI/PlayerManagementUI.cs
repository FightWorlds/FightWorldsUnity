using System;
using FightWorlds.Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FightWorlds.UI
{
    public class PlayerManagementUI : MonoBehaviour
    {
        [Header("Level")]
        [SerializeField] private TextMeshProUGUI textLevel;
        [SerializeField] private TextMeshProUGUI textExperience;
        [SerializeField] private Slider sliderLevel;
        [SerializeField] private GameObject levelUpPopUp;
        [SerializeField] private Text textLevelPopUp;
        [SerializeField] private Text headerLevelPopUp;
        [SerializeField] private Text descriptionLevelPopUp;
        [Header("Resources")]
        [SerializeField] private TextMeshProUGUI textResources;
        [SerializeField] private TextMeshProUGUI textEnergy;
        [SerializeField] private TextMeshProUGUI textArtifacts;
        [SerializeField] private TextMeshProUGUI textCredits;
        [Header("PopUp")]
        [SerializeField] private TextMeshProUGUI textPopUp;
        [SerializeField] private GameObject popUp;
        [SerializeField] private Button agreeButton;
        [Header("VIP")]
        //[SerializeField] private TextMeshProUGUI textPreferences;
        [SerializeField] private GameObject vip;

        public readonly int CreditsDiv = 1000;

        public void FillLevelUi(int level, int experience,
        int experienceNextLevel, bool isMaxLvl)
        {
            textLevel.text = level.ToString();
            textExperience.text = isMaxLvl ?
            $"{experienceNextLevel} / {experienceNextLevel}" :
            $"{experience} / {experienceNextLevel}";
            sliderLevel.value = isMaxLvl ? 1 :
                (float)experience / experienceNextLevel;
            // TODO switch slidervalue on image (filled)-horizontal-amount
        }

        public void FillResourcesUi(int ore, int gas,
        int metal, int energy, int credits, int artifacts)
        {
            textResources.text = $"Metal: {metal}\n Ore: {ore}";
            textEnergy.text = $"Energy: {energy}\n Gas: {gas}";
            textCredits.text = $"Credits:\n{credits}";
            textArtifacts.text = $"Artifacts:\n{artifacts}";
        }

        public void FillVipUi(float mltpl)
        {
            if (mltpl <= 1)
                vip.SetActive(false);
            else
            {
                vip.SetActive(true);
                //textPreferences.text = $"VIP\n{(mltpl - 1) * 10}";
            }
        }

        public void ShowLevelUp(int level, int credits)
        {
            textLevelPopUp.text = level.ToString();
            descriptionLevelPopUp.text =
            $"You reached LVL {level}\n\nCredits: {credits}";
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


        public void FinishGamePopUp(int artifacts)
        {
            levelUpPopUp.SetActive(true);
            textLevelPopUp.text = "";
            headerLevelPopUp.text = "FINISH";
            descriptionLevelPopUp.text =
                $"Session result:\n\n{artifacts} artifacts";
        }
    }
}