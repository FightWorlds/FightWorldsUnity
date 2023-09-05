using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject damageNotificationPrefab;
    [SerializeField] Transform damageNotificationContainer;
    [SerializeField] Text damageNotificationCounter;
    [SerializeField] GameObject activeProcessPrefab;
    [SerializeField] Transform activeProcessContainer;
    [SerializeField] Text activeProcessCounter;
    [SerializeField] GameObject buildingMenu;
    [SerializeField] Text simpleText;
    [SerializeField] Text instantText;
    [SerializeField] Vector2 bmMinBorders;
    [SerializeField] Vector2 bmMaxBorders;

    private Building selectedBuilding;
    private Dictionary<Building, GameObject> buildingsUnderAttack;
    private Dictionary<GameObject, GameObject> activeProcesses;
    private const int listSize = 5;
    private const int rotationAngle = 60;
    private const string underAttackText = "Buildings Under Attack: ";
    private const string buildingText = "BUILDING:\n";

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
        newDamagedObj.transform.GetChild(0).GetComponent<Text>().text =
            building.name;
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
        damageNotificationCounter.text = underAttackText +
        buildingsUnderAttack.Count;
    }

    internal void NewActiveProcess(GameObject gameObject)
    {
        if (IsProcessesFulled() ||
            activeProcesses.ContainsKey(gameObject))
            return;
        var newProcessUI = Instantiate(activeProcessPrefab, activeProcessContainer);
        newProcessUI.transform.GetChild(0).GetComponent<Text>().text =
            buildingText + gameObject.name;
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

    private void UpdateProcessCounter() =>
        activeProcessCounter.text =
        $"Active Process {activeProcesses.Count}/{listSize}";

    public bool IsProcessesFulled() => activeProcesses.Count == listSize;

    public void ShowBuildingMenu(Building building)
    {
        selectedBuilding = building;
        buildingMenu.SetActive(true);
        string text;
        if (building.IsCompleted)
            text = "REPAIR";
        else
            text = "BUILD";
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
            selectedBuilding.transform.Rotate(Vector3.up, rotationAngle);
    }
}
