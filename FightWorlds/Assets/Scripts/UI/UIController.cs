using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [Header("Damage Notification")]
    [SerializeField] private GameObject damageNotificationPrefab;
    [SerializeField] private Transform damageNotificationContainer;
    [SerializeField] private TextMeshProUGUI damageNotificationCounter;
    [Header("Active Processes")]
    [SerializeField] private GameObject activeProcessPrefab;
    [SerializeField] private Transform activeProcessContainer;
    [SerializeField] private TextMeshProUGUI activeProcessCounter;
    [Header("Building Menu")]
    [SerializeField] private GameObject buildingMenu;
    [SerializeField] private TextMeshProUGUI simpleText;
    [SerializeField] private TextMeshProUGUI instantText;
    [SerializeField] private Vector2 bmMinBorders;
    [SerializeField] private Vector2 bmMaxBorders;
    [Header("Level")]
    [SerializeField] private TextMeshProUGUI textLevel;
    [SerializeField] private TextMeshProUGUI textExperience;
    [SerializeField] private Slider sliderLevel;
    [SerializeField] private GameObject levelUpPopUp;
    [SerializeField] private Text textLevelUpPopUp;
    [SerializeField] private Text textDescriptionPopUp;
    [Header("Resources")]
    [SerializeField] private TextMeshProUGUI textResources;
    [SerializeField] private TextMeshProUGUI textEnergy;
    [SerializeField] private TextMeshProUGUI textArtifacts;
    [SerializeField] private TextMeshProUGUI textCredits;
    [Header("VIP")]
    //[SerializeField] private TextMeshProUGUI textPreferences;
    [SerializeField] private GameObject vip;
    [Header("PopUp")]
    [SerializeField] private TextMeshProUGUI textPopUp;
    [SerializeField] private GameObject popUp;

    private Building selectedBuilding;
    private Dictionary<Building, GameObject> buildingsUnderAttack;
    private Dictionary<GameObject, GameObject> activeProcesses;
    private const int cloneLen = 7;
    private const int listSize = 5;
    private const string buildingText = "BUILDING";
    private const string repairingText = "REPAIRING";
    private const string productionText = "PRODUCTION";
    private const string upgradingText = "UPGRADING";
    // TODO single open panel (other should close)
    private void Awake()
    {
        buildingsUnderAttack = new Dictionary<Building, GameObject>();
        activeProcesses = new Dictionary<GameObject, GameObject>();
    }

    public void AddBuildUnderAttack(Building building)
    {
        if (buildingsUnderAttack.Count >= listSize ||
            buildingsUnderAttack.ContainsKey(building))
            return;
        var newDamagedObj = Instantiate(damageNotificationPrefab, damageNotificationContainer);
        string name = building.name;
        newDamagedObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>()
        .text = name.Remove(name.Length - cloneLen);
        buildingsUnderAttack.Add(building, newDamagedObj);
        UpdateAttackCounter();
    }

    public void RemoveFromUnderAttack(Building building)
    {
        buildingsUnderAttack.Remove(building, out GameObject uiObj);
        Destroy(uiObj);
        UpdateAttackCounter();
    }

    private void UpdateAttackCounter()
    {
        damageNotificationCounter.text = buildingsUnderAttack.Count.ToString();
    }

    public void NewActiveProcess(GameObject gameObject, ProcessType type)
    {
        if (IsProcessesFulled() ||
            activeProcesses.ContainsKey(gameObject))
            return;
        var newProcessUI = Instantiate(activeProcessPrefab, activeProcessContainer);
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
        string name = gameObject.name;
        newProcessUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text
        = text + ":\n" + name.Remove(name.Length - cloneLen);
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

    public void FillLevelUi(int level, int experience,
    int experienceNextLevel, bool isMaxLvl)
    {
        textLevel.text = level.ToString();
        textExperience.text = isMaxLvl ?
        $"{experienceNextLevel} / {experienceNextLevel}" :
        $"{experience} / {experienceNextLevel}";
        sliderLevel.value = isMaxLvl ? 1 :
            (float)experience / experienceNextLevel;
        // TODO slidervalue to image (filled) - horizontal - amount
    }

    public void FillResourcesUi(int ore, int gas,
    int metal, int energy, int credits, int artifacts)
    {
        textResources.text = $"Metal: {metal}\n Ore: {ore}";
        textEnergy.text = $"Energy: {energy}\n Gas: {gas}";
        textCredits.text = $"Credits:\n{credits}";
        textArtifacts.text = $"Artifacts:\n{artifacts}";
    }

    public bool IsProcessesFulled() => activeProcesses.Count == listSize;

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
                selectedBuilding.PermanentBuild(instant, false);
                break;
            case BuildingState.Damaged:
                selectedBuilding.TryRepair(instant);
                break;
            case BuildingState.Default:
                selectedBuilding.TryUpgrade(instant);
                break;
        }
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
        textLevelUpPopUp.text = level.ToString();
        textDescriptionPopUp.text =
        $"You reached LVL {level}\n\nCredits: {credits}";
        levelUpPopUp.SetActive(true);
    }

    public void ShowResourcePopUp(ResourceType type, int amount)
    {
        string text = "There is no enough ";
        if (type == ResourceType.Credits)
            text += "credits";
        else
            text += $"{type} resources\n" +
            "Would you like instant Build/Repair/Upgrade for\n" +
            $"<{amount}> credits";
        textPopUp.text = text;
        popUp.SetActive(true);
    }

    public void FinishGamePopUp(bool win, int artifacts)
    {
        levelUpPopUp.SetActive(true);
        levelUpPopUp.transform.GetChild(1).GetChild(0).GetComponent<Text>()
        .text = "";
        levelUpPopUp.transform.GetChild(2).GetChild(0).GetComponent<Text>()
        .text = win ? "SUCCESS" : "UNLUCKY";
        levelUpPopUp.transform.GetChild(3).GetChild(0).GetComponent<Text>()
        .text = $"Session result:\n\n{artifacts} artifacts";
    }

    private void UpdateProcessCounter() =>
        activeProcessCounter.text = $"{activeProcesses.Count}/{listSize}";
}
