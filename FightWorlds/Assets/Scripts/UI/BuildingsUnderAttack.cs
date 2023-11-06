using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using FightWorlds.Placement;
using FightWorlds.Controllers;
using System.Text;

namespace FightWorlds.UI
{
    public class BuildingsUnderAttack : MonoBehaviour
    {
        [SerializeField] private CameraController cameraController;
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform container;
        [SerializeField] private TextMeshProUGUI counter;
        [SerializeField] private BuildingMenuUI buildingMenu;
        private Dictionary<Building, GameObject> buildingsUnderAttack;

        private const int listSize = 5;
        private const int fillLength = 15;
        private const string repeatChar = "I";

        private void Awake() =>
            buildingsUnderAttack = new Dictionary<Building, GameObject>();

        private void Update()
        {
            Building building;
            GameObject ui;
            foreach (var attack in buildingsUnderAttack)
            {
                building = attack.Key;
                ui = attack.Value;
                int hpBars =
                    (int)(building.Hp / (float)building.MaxHp * fillLength);
                FillProgressBar(ui, hpBars);
            }
        }

        public void AddBuildUnderAttack(Building building)
        {
            if (buildingsUnderAttack.Count >= listSize ||
                buildingsUnderAttack.ContainsKey(building))
                return;
            GameObject newDamagedObj = Instantiate(prefab, container);
            newDamagedObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>()
                .text = building.name;
            newDamagedObj.GetComponent<Button>()
                .onClick.AddListener(() => MoveToBuilding(building));
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

        public void CollapseList() => container.gameObject.SetActive(false);

        private void MoveToBuilding(Building building)
        {
            Vector3 buildingPos = building.transform.position;
            Vector3 cameraPos =
                new(buildingPos.x - cameraController.XOffset,
                cameraController.yOffset,
                buildingPos.z - cameraController.ZOffset);
            cameraController.MoveToNewPosition(cameraPos, true);
            buildingMenu.ShowBuildingMenu(building);

        }

        private void UpdateAttackCounter() =>
          counter.text = buildingsUnderAttack.Count.ToString();

        private void FillProgressBar(GameObject ui, int count) =>
            ui.transform.GetChild(1).GetChild(0)
                .GetComponent<TextMeshProUGUI>().text =
                    DuplicateCharacter(count);

        private static string DuplicateCharacter(int count)
        {
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < count; i++)
                result.Append(repeatChar);

            return result.ToString();
        }
    }
}