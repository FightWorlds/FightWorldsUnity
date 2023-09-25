using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FightWorlds.Placement;
using FightWorlds.Controllers;

namespace FightWorlds.UI
{
    public class BuildingsUnderAttack : MonoBehaviour
    {
        [SerializeField] private CameraController cameraController;
        [SerializeField] private GameObject damageNotificationPrefab;
        [SerializeField] private Transform damageNotificationContainer;
        [SerializeField] private TextMeshProUGUI damageNotificationCounter;
        [SerializeField] private BuildingMenuUI buildingMenu;
        private Dictionary<Building, GameObject> buildingsUnderAttack;

        private const int listSize = 5;

        private void Awake() =>
            buildingsUnderAttack = new Dictionary<Building, GameObject>();


        public void AddBuildUnderAttack(Building building)
        {
            if (buildingsUnderAttack.Count >= listSize ||
                buildingsUnderAttack.ContainsKey(building))
                return;
            var newDamagedObj =
            Instantiate(damageNotificationPrefab, damageNotificationContainer);
            newDamagedObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>()
            .text = building.name;
            newDamagedObj.GetComponent<Button>().onClick.AddListener(() =>
            {
                Vector3 buildingPos = building.transform.position;
                Vector3 cameraPos =
                    new Vector3(buildingPos.x + cameraController.XOffset,
                    cameraController.yOffset,
                    buildingPos.z + cameraController.ZOffset);
                cameraController.MoveToNewPosition(cameraPos, true);
                buildingMenu.ShowBuildingMenu(building);
            });
            buildingsUnderAttack.Add(building, newDamagedObj);
            UpdateAttackCounter();
        }

        public void RemoveFromUnderAttack(Building building)
        {
            if (!buildingsUnderAttack.Remove(building, out GameObject uiObj))
                return;
            Destroy(uiObj);
            UpdateAttackCounter();
        }

        private void UpdateAttackCounter() =>
            damageNotificationCounter.text =
            buildingsUnderAttack.Count.ToString();
    }
}