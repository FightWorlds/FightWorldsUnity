using System.Collections;
using System.Collections.Generic;
using FightWorlds.Combat;
using FightWorlds.Controllers;
using FightWorlds.Placement;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FightWorlds.UI
{
    public class UnitsMenu : MonoBehaviour
    {
        [SerializeField] private PlacementSystem placement;
        [SerializeField] private TextMeshProUGUI numberOfUnits;
        [SerializeField] private TextMeshProUGUI currentUnitStats;
        [SerializeField] private TextMeshProUGUI nextUnitType;
        [SerializeField] private TextMeshProUGUI nextUnitStats;
        [SerializeField] private TextMeshProUGUI usageValues;
        [SerializeField] private TextMeshProUGUI minUnitsToProduce;
        [SerializeField] private TextMeshProUGUI maxUnitsToProduce;
        [SerializeField] private TextMeshProUGUI timeLeftText;
        [SerializeField] private TextMeshProUGUI instantFinishPriceText;
        [SerializeField] private TextMeshProUGUI totalArtifactsText;
        [SerializeField] private Slider selectUnitsAmountToProduce;
        [SerializeField] private Slider leftTimeSlider;
        [SerializeField] private List<GameObject> setupElements;
        [SerializeField] private List<GameObject> processElements;

        private const int defaultDamage = 4;
        private const int defaultRate = 1;
        private const int defaultStrength = 2;
        private const int defaultRange = 10;
        private const int maxUpgradeLevel = 3;
        private const int maxStoredUnits = 400;
        private const int instantUnitPrice = 1;
        private const int unitUpgradeCost = 100;
        private const int unitTimeInSecondsCost = 10;
        private const int unitEnergyCost = 50;
        private const int unitMetalCost = 50;
        private const int unitArtifactsCost = 10;
        private const int dockyardLevel = 1;

        public bool IsProducing { get; private set; }
        public static int MaxPossibleUnits => maxStoredUnits * dockyardLevel;
        public static readonly FiringStats DefaultFiringStats =
            new(defaultDamage, defaultRate, defaultStrength, defaultRange);

        private UnitTabs currentTab;
        private FiringStats UnitStats;
        private Building dockyard;
        private int timeCost;
        private int unitsLeftToProduce;
        private int lastPossibleValue;
        private Color defaultColor;
        private KeyValuePair<ResourceType, int>[] usage;

        private int storedUnits => (currentTab == UnitTabs.Produce) ?
        placement.player.resourceSystem.Resources[ResourceType.Units] :
        placement.player.resourceSystem.Resources[ResourceType.UnitsToHeal];
        private int maxUnits => (currentTab == UnitTabs.Produce) ?
        MaxPossibleUnits - storedUnits -
        placement.player.resourceSystem.Resources[ResourceType.UnitsToHeal] :
        storedUnits;
        private int upgradePrice =>
            unitUpgradeCost * placement.player.UnitsLevel;

        public void InitDockyard(Building building)
        {
            UnitStats = DefaultFiringStats * placement.player.UnitsLevel;
            dockyard = building;
            defaultColor = usageValues.color;
            UpdateMaxPossibleUnits();
            UpdateNumberOfUnits();
            UpdateStats();
            UpdateTotalArtifacts();
            OnValueChanged(0);
            selectUnitsAmountToProduce
                .onValueChanged.AddListener(OnValueChanged);
        }

        public void AddUnit()
        {
            if (storedUnits > maxUnits)
                return;
            if (currentTab == UnitTabs.Heal)
                placement.player
                    .UseResources(1, ResourceType.UnitsToHeal, false);
            placement.player.TakeResources(1, ResourceType.Units);
            unitsLeftToProduce--;
            UpdateInstantPrice();
            UpdateNumberOfUnits();
        }

        public void StartProcess()
        {
            if (ResetPossibleProduce()) return;

            foreach (var resource in usage)
                placement.player.UseResources(resource.Value, resource.Key, false);
            foreach (var ui in setupElements)
                ui.SetActive(false);
            foreach (var ui in processElements)
                ui.SetActive(true);
            usageValues.color = defaultColor;
            unitsLeftToProduce =
                (lastPossibleValue > selectUnitsAmountToProduce.value) ?
                lastPossibleValue : (int)selectUnitsAmountToProduce.value;
            UpdateInstantPrice();
            StartCoroutine(Process());
        }

        public void RemoveDockyard()
        {
            dockyard = null;
            IsProducing = false;
            StopAllCoroutines();
        }

        public void InstantFinish()
        {
            if (currentTab == UnitTabs.Upgrade)
            {
                if (placement.player.UnitsLevel < maxUpgradeLevel &&
                placement.player.UseResources(upgradePrice,
                ResourceType.Credits, false))
                    Upgrade();
                return;
            }
            if (placement.player.UseResources(instantUnitPrice *
                unitsLeftToProduce, ResourceType.Credits, false))
            {
                StopAllCoroutines();
                if (currentTab == UnitTabs.Produce)
                    placement.player.
                TakeResources(unitsLeftToProduce, ResourceType.Units);
                else
                {
                    placement.player.TakeResources(unitsLeftToProduce,
                        ResourceType.Units);
                    placement.player.UseResources(unitsLeftToProduce,
                        ResourceType.UnitsToHeal, false);
                }
                unitsLeftToProduce = 0;
                FinishProcess();
            }
        }

        public void SwitchToProduceTab() => SwitchTab(UnitTabs.Produce);
        public void SwitchToHealTab() => SwitchTab(UnitTabs.Heal);
        public void SwitchToUpgradeTab() => SwitchTab(UnitTabs.Upgrade);

        private void SwitchTab(UnitTabs tab)
        {
            if (IsProducing) return;
            currentTab = tab;
            if (tab == UnitTabs.Upgrade)
            {
                foreach (var ui in setupElements)
                    ui.SetActive(false);
                processElements[1].SetActive(true);
                UpdateInstantPrice();
            }
            else FinishProcess();
        }

        private IEnumerator Process()
        {
            int timeLeft = timeCost;
            IsProducing = true;
            while (timeLeft > 0)
            {
                timeLeftText.text = FormatTime(timeLeft);
                leftTimeSlider.value = 1 - (timeLeft / (float)timeCost);
                yield return new WaitForSeconds(1f);
                timeLeft--;
            }
            FinishProcess();
        }

        private void FinishProcess()
        {
            dockyard.StopProduce();
            IsProducing = false;
            foreach (var ui in setupElements)
                ui.SetActive(true);
            foreach (var ui in processElements)
                ui.SetActive(false);
            UpdateNumberOfUnits();
            UpdateMaxPossibleUnits();
            UpdateStats();
            UpdateTotalArtifacts();
            ResetPossibleProduce();
        }

        private void OnValueChanged(float value)
        {
            int val = (int)value;
            minUnitsToProduce.text = val.ToString();
            usage =
                new KeyValuePair<ResourceType, int>[] {
                new(ResourceType.TotalArtifacts,
                (currentTab == UnitTabs.Produce) ? unitArtifactsCost * val :
                unitArtifactsCost / 2 * val),
                new(ResourceType.Energy,
                (currentTab == UnitTabs.Produce) ? unitEnergyCost * val :
                unitEnergyCost/2 * val ),
                new(ResourceType.Metal,
                (currentTab == UnitTabs.Produce) ? unitMetalCost * val :
                unitMetalCost / 2 * val)
            };
            bool possible = placement.player.resourceSystem.CanUseResources(usage);
            if (possible)
            {
                lastPossibleValue = val;
                usageValues.color = defaultColor;
            }
            else
                usageValues.color = Color.red;

            timeCost = unitTimeInSecondsCost * val;
            string timeCostString = FormatTime(timeCost);
            usageValues.text =
                $"{usage[0].Value}\n{usage[1].Value}\n{usage[2].Value}\n{timeCostString}";
        }

        private void Upgrade()
        {
            placement.player.UnitsNewLevel();
            UnitStats += DefaultFiringStats;
            UpdateStats();
        }

        private void UpdateMaxPossibleUnits()
        {
            maxUnitsToProduce.text = maxUnits.ToString();
            selectUnitsAmountToProduce.maxValue = maxUnits;
        }

        private void UpdateNumberOfUnits() =>
            numberOfUnits.text = storedUnits.ToString();

        private void UpdateStats()
        {
            currentUnitStats.text =
            $"{UnitStats.Damage}\n{UnitStats.Rate}\n{UnitStats.Strength}";
            nextUnitStats.text =
            $"+{DefaultFiringStats.Damage}\n+{DefaultFiringStats.Rate}\n+{DefaultFiringStats.Strength}";
            string type = "TYPE ";
            switch (placement.player.UnitsLevel)
            {
                case 1:
                    type += "II";
                    break;
                case 2:
                    type += "III";
                    break;
                case 3:
                    type += "?";
                    break;
            }
            nextUnitType.text = type;
        }

        private void UpdateTotalArtifacts() =>
            totalArtifactsText.text =
            "ARTIFACTS: " + placement.player.resourceSystem.Resources[ResourceType.TotalArtifacts];

        private void UpdateInstantPrice() =>
            instantFinishPriceText.text = (currentTab == UnitTabs.Upgrade) ?
            $"$LT: {upgradePrice}" :
            $"$LT: {instantUnitPrice * unitsLeftToProduce}";

        private string FormatTime(int seconds)
        {
            int hours = 0;
            int min = seconds / 60;
            if (min >= 60)
            {
                hours = min / 60;
                min = min % 60;
            }
            int sec = seconds % 60;
            string minString = min.ToString();
            string secString = sec.ToString();
            minString = (minString.Length == 1) ? $"0{minString}" : minString;
            secString = (secString.Length == 1) ? $"0{secString}" : secString;
            string result = hours == 0 ? "" : $"{hours}:";
            return result += $"{minString}:{secString}";
        }

        private bool ResetPossibleProduce()
        {
            if (placement.player.resourceSystem.CanUseResources(usage))
                return false;
            selectUnitsAmountToProduce.value =
            (selectUnitsAmountToProduce.value == lastPossibleValue) ?
            0 : lastPossibleValue;
            return true;
        }
    }

    public enum UnitTabs
    {
        Produce,
        Heal,
        Upgrade
    }
}