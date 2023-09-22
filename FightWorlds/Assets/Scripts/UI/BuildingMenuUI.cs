using TMPro;
using UnityEngine;
using FightWorlds.Placement;

namespace FightWorlds.UI
{
    public class BuildingMenuUI : MonoBehaviour
    {
        [SerializeField] private GameObject buildingMenu;
        [SerializeField] private TextMeshProUGUI simpleText;
        [SerializeField] private TextMeshProUGUI instantText;
        [SerializeField] private TextMeshProUGUI infoText;

        private Vector2 bmMinBorders;
        private Vector2 bmMaxBorders;
        private Building selectedBuilding;
        private const float widthMinKoef = 5.76f;
        private const float widthMaxKoef = 2.93f;
        private const float heightMinKoef = 6.4f;
        private const float heightMaxKoef = 3.87f;

        private void Awake()
        {
            int width = Screen.width;
            int height = Screen.height;
            bmMinBorders =
                new Vector2(width / widthMinKoef, height / heightMinKoef);
            bmMaxBorders =
                new Vector2(width - (width / widthMaxKoef),
                height - (height / heightMaxKoef));
        }

        public void ShowBuildingMenu(Building building)
        {
            string text = "";
            selectedBuilding = building;
            buildingMenu.SetActive(true);
            switch (building.State)
            {
                case BuildingState.Building:
                    text = "BUILD";
                    break;
                case BuildingState.Damaged:
                    text = "REPAIR";
                    break;
                case BuildingState.Default:
                    text = "UPGRADE";
                    break;
            }
            simpleText.text = text;
            instantText.text = "INSTANT " + text;
            string[] parts = building.name.Split(" ");
            string namePart =
                parts.Length == 2 ? parts[0] : parts[0] + parts[1];
            string[] coords = building.transform.parent.name.Split(" ");
            infoText.text =
                namePart + "\nX:" + coords[1] + "\n\nY:" + coords[2] +
                "\n#" + parts[parts.Length - 1];
            infoText.gameObject.SetActive(false);
            Vector3 screenPos =
                Camera.main.WorldToScreenPoint(building.transform.position);
            float x = Mathf.Clamp(screenPos.x, bmMinBorders.x, bmMaxBorders.x);
            float y = Mathf.Clamp(screenPos.y, bmMinBorders.y, bmMaxBorders.y);
            buildingMenu.transform.position = new Vector3(x, y);
        }


        public void CloseBuildingMenu()
        {
            selectedBuilding = null;
            buildingMenu.SetActive(false);
        }

        public void RotateBuilding()
        {
            if (selectedBuilding != null)
                selectedBuilding.Rotate();
        }

        public void Action(bool instant)
        {
            if (selectedBuilding == null)
                return;
            switch (selectedBuilding.State)
            {
                case BuildingState.Building:
                    selectedBuilding.TryBuild(instant);
                    break;
                case BuildingState.Damaged:
                    selectedBuilding.TryRepair(instant);
                    break;
                case BuildingState.Default:
                    selectedBuilding.TryUpgrade(instant);
                    break;
            }
        }
    }
}