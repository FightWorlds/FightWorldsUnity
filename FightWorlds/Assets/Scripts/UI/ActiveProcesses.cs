using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FightWorlds.UI
{
    public class ActiveProcesses : MonoBehaviour
    {
        [SerializeField] private GameObject activeProcessPrefab;
        [SerializeField] private Transform activeProcessContainer;
        [SerializeField] private TextMeshProUGUI activeProcessCounter;

        private const int listSize = 5;
        private const string buildingText = "BUILDING";
        private const string repairingText = "REPAIRING";
        private const string productionText = "PRODUCTION";
        private const string upgradingText = "UPGRADING";

        private Dictionary<GameObject, GameObject> activeProcesses;
        private void Awake() =>
            activeProcesses = new Dictionary<GameObject, GameObject>();


        public void NewActiveProcess(GameObject gameObject, ProcessType type)
        {
            if (IsProcessesFulled() ||
                activeProcesses.ContainsKey(gameObject))
                return;
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
            UpdateProcessCounter();
        }

        public void RemoveProcess(GameObject obj)
        {
            if (!activeProcesses.Remove(obj, out GameObject uiObj))
                return;
            Destroy(uiObj);
            UpdateProcessCounter();
        }

        public bool IsProcessesFulled()
        {
            if (activeProcesses.Count < listSize)
                return false;
            return true;
        }

        private void UpdateProcessCounter() =>
            activeProcessCounter.text = $"{activeProcesses.Count}/{listSize}";
    }
}