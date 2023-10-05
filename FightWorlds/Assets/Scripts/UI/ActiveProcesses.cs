using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FightWorlds.Placement;
using FightWorlds.Controllers;
using System.Linq;
using System;
using System.Collections;

namespace FightWorlds.UI
{
    public class ActiveProcesses : MonoBehaviour
    {
        [SerializeField] private GameObject activeProcessPrefab;
        [SerializeField] private Transform activeProcessContainer;
        [SerializeField] private TextMeshProUGUI activeProcessCounter;
        [SerializeField] private PlacementSystem placement;
        [SerializeField] private PlayerManagementUI ui;
        [SerializeField] private Bot botPrefab;

        private const int maxUnitsAmount = 4;
        private const int newBotPrice = 60;
        private const string buildingText = "BUILDING";
        private const string repairingText = "REPAIRING";
        private const string productionText = "PRODUCTION";
        private const string upgradingText = "UPGRADING";

        private Dictionary<GameObject, GameObject> activeProcesses;
        private Dictionary<GameObject, Bot> botsProcesses;
        private Dictionary<Building, Bot> docks;
        private int docksAmount;

        private void Awake()
        {
            activeProcesses = new();
            botsProcesses = new();
            docks = new();
            StartCoroutine(StartUpdate());
        }

        public bool NewActiveProcess(GameObject gameObject, ProcessType type)
        {
            if (activeProcesses.ContainsKey(gameObject)) return false;
            if (IsProcessesFulled()) return false;
            var newProcessUI =
                Instantiate(activeProcessPrefab, activeProcessContainer);
            string text = "";
            switch (type)
            {
                case ProcessType.Building:
                    text = buildingText;
                    break;
                case ProcessType.Repairing:
                    text = repairingText;
                    break;
                case ProcessType.Production:
                    text = productionText;
                    break;
                case ProcessType.Upgrading:
                    text = upgradingText;
                    break;
                default: break;
            }
            newProcessUI.transform.GetChild(0)
                .GetComponent<TextMeshProUGUI>().text =
                text + ":\n" + gameObject.name;
            activeProcesses.Add(gameObject, newProcessUI);
            Bot freeBot =
            docks.First(pair =>
            pair.Value.gameObject.activeSelf && !pair.Value.IsBusy).Value;
            freeBot.StartOperation(gameObject.transform.position);
            botsProcesses.Add(gameObject, freeBot);
            UpdateProcessCounter();
            return true;
        }

        public void RemoveProcess(GameObject obj, bool defaultRemove)
        {
            if (!activeProcesses.Remove(obj, out GameObject uiObj))
                return;
            botsProcesses.Remove(obj, out Bot bot);
            if (!defaultRemove) bot.ReturnAtDock();
            Destroy(uiObj);
            UpdateProcessCounter();
        }

        public bool IsProcessesFulled()
        {
            if (activeProcesses.Count < Limit())
                return false;
            if (activeProcesses.Count >= placement.player.BotsAmount)
            {
                int price = placement.player.BotsAmount * newBotPrice;
                ui.ShowBotsPopUp(price, () =>
                {
                    if (placement.player.resourceSystem.UseResources(
                    price, ResourceType.Credits))
                    {
                        placement.player.AddBots();
                        if (placement.player.BotsAmount > docks.Count)
                            return;
                        docks.First(e => !e.Value.gameObject.activeSelf)
                        .Value.gameObject.SetActive(true);
                        UpdateProcessCounter();
                    }
                });
            }
            return true;
        }

        public void AddRepairBot(Building dock)
        {
            if (docksAmount >= maxUnitsAmount) return;
            docksAmount++;
            var bot = Instantiate(botPrefab,
            dock.transform.position, Quaternion.identity);
            if (docksAmount > placement.player.BotsAmount)
                bot.gameObject.SetActive(false);
            docks.Add(dock, bot);
            UpdateProcessCounter();
        }

        public void RemoveRepairBot(Building dock)
        {
            if (docksAmount == 0) return;
            docksAmount--;
            docks.Remove(dock, out Bot bot);
            Destroy(bot.gameObject);
            UpdateProcessCounter();
        }

        private IEnumerator StartUpdate()
        {
            yield return null;
            UpdateProcessCounter();
        }

        private int Limit() => docksAmount <= placement.player.BotsAmount ?
            docksAmount : placement.player.BotsAmount;

        private void UpdateProcessCounter() =>
            activeProcessCounter.text =
                $"{activeProcesses.Count}/{Limit()}";
    }
}