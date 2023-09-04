using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    [SerializeField] GameObject damageNotificationPrefab;
    [SerializeField] Transform damageNotificationContainer;
    [SerializeField] Text damageNotificationCounter;

    private Dictionary<Building, GameObject> buildingsUnderAttack;
    private const int listSize = 5;
    private const string underAttackText = "Buildings Under Attack: ";

    private void Awake() =>
        buildingsUnderAttack = new Dictionary<Building, GameObject>();

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
        Debug.Log("pos " + uiObj.transform.GetChild(0).GetComponent<Text>().text);
        UpdateAttackCounter();
    }

    private void UpdateAttackCounter()
    {
        Debug.Log("COUNT " + buildingsUnderAttack.Count);
        damageNotificationCounter.text = underAttackText +
        buildingsUnderAttack.Count;
    }
}
