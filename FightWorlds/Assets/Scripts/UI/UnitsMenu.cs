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
        [SerializeField] private Slider selectUnitsAmountToProduce;
        [SerializeField] private Slider leftTimeSlider;
        [SerializeField] private List<GameObject> setupElements;
        [SerializeField] private List<GameObject> processElements;
        private const int defaultDamage = 2;
        private const int defaultRate = 1;
        private const int defaultStrength = 2;
        private const int defaultLevel = 1;
        private const int maxUpgradeLevel = 3;
        private const int MaxStoredUnits = 200;
        private const int InstantUnitPrice = 1;
        private const int UnitTimeInSecondsCost = 20;
        private const int UnitEnergyCost = 200;
        private const int UnitMetalCost = 100;
        private const int UnitArtifactsCost = 10;
        private readonly FiringStats defaultFiringStats =
                new(defaultDamage, defaultRate, defaultStrength);

        public bool IsProducing { get; private set; }

        private int UnitsLevel;
        private int StoredUnits;
        private FiringStats UnitStats;
        private Building dockyard;
        private int timeCost;
        private int unitsLeftToProduce;
        private int lastPossibleValue;
        private Color defaultColor;
        private KeyValuePair<ResourceType, int>[] usage;

        private int maxUnits => MaxStoredUnits - StoredUnits;

        public void InitDockyard(Building building)
        {
            UnitStats = defaultFiringStats;
            UnitsLevel = defaultLevel;
            StoredUnits = 0;
            dockyard = building;
            defaultColor = usageValues.color;
            UpdateMaxPossibleUnits();
            UpdateNumberOfUnits();
            UpdateStats();
            OnValueChanged(0);
            selectUnitsAmountToProduce.onValueChanged.AddListener(OnValueChanged);
        }

        public void AddUnit()
        {
            if (StoredUnits < MaxStoredUnits)
            {
                StoredUnits++;
                unitsLeftToProduce--;
                UpdateInstantPrice();
                UpdateNumberOfUnits();
            }
        }

        public void Upgrade()
        {
            if (UnitsLevel >= maxUpgradeLevel)
                return;
            UnitsLevel++;
            UnitStats += defaultFiringStats;
        }

        public void StartProcess()
        {
            if (!placement.player.resourceSystem.CanUseResources(usage))
            {
                selectUnitsAmountToProduce.value = lastPossibleValue;
                return;
            }
            foreach (var resource in usage)
                placement.player.UseResources(resource.Value, resource.Key, false);
            foreach (var ui in setupElements)
                ui.SetActive(false);
            foreach (var ui in processElements)
                ui.SetActive(true);
            usageValues.color = defaultColor;
            unitsLeftToProduce = (lastPossibleValue > selectUnitsAmountToProduce.value) ? lastPossibleValue :
            (int)selectUnitsAmountToProduce.value;
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
            if (placement.player.UseResources(InstantUnitPrice * unitsLeftToProduce, ResourceType.Credits, false))
            {
                StopAllCoroutines();
                StoredUnits += unitsLeftToProduce;
                unitsLeftToProduce = 0;
                FinishProcess();
            }
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
        }

        private void OnValueChanged(float value)
        {
            int val = (int)value;
            minUnitsToProduce.text = val.ToString();
            usage =
            new KeyValuePair<ResourceType, int>[] {
            new(ResourceType.Artifacts, UnitArtifactsCost * val),
            new(ResourceType.Energy, UnitEnergyCost * val),
            new(ResourceType.Metal, UnitMetalCost * val)
            };
            bool possible = placement.player.resourceSystem.CanUseResources(usage);
            if (possible)
            {
                lastPossibleValue = val;
                usageValues.color = defaultColor;
            }
            else
                usageValues.color = Color.red;

            timeCost = UnitTimeInSecondsCost * val;
            string timeCostString = FormatTime(timeCost);
            usageValues.text =
                $"{usage[0].Value}\n{usage[1].Value}\n{usage[2].Value}\n{timeCostString}";
        }

        private void UpdateMaxPossibleUnits()
        {
            maxUnitsToProduce.text = maxUnits.ToString();
            selectUnitsAmountToProduce.maxValue = maxUnits;
        }

        private void UpdateNumberOfUnits() =>
            numberOfUnits.text = StoredUnits.ToString();

        private void UpdateStats()
        {
            currentUnitStats.text =
            $"{UnitStats.Damage}\n{UnitStats.Rate}\n{UnitStats.Strength}";
            nextUnitStats.text =
            $"+{defaultFiringStats.Damage}\n+{defaultFiringStats.Rate}\n+{defaultFiringStats.Strength}";
            string type = "TYPE ";
            switch (UnitsLevel)
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

        private void UpdateInstantPrice() =>
            instantFinishPriceText.text =
            $"$LT: {InstantUnitPrice * unitsLeftToProduce}";

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
    }
}