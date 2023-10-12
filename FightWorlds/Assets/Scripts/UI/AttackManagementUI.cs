using System;
using FightWorlds.Boost;
using FightWorlds.Combat;
using FightWorlds.Controllers;
using FightWorlds.Placement;
using FightWorlds.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AttackManagementUI : MonoBehaviour
{
    public static bool IsAttackStarted { get; private set; }
    public static bool GameShouldFinish;

    [SerializeField] private PlacementSystem placement;
    [SerializeField] private GameObject holder;
    [SerializeField] private GameObject FinishPopUp;
    [SerializeField] private Emitter emitter;
    [SerializeField] private TechnoMap map;
    [SerializeField] private Text timer;
    [SerializeField] private float timeLeft;

    private const float widthKoef = 7f;
    private const float heightKoef = 10f;
    private const int minSuccess = 60;
    private const int restoreLost = 5;
    private const int restoreSuccess = 2;

    private Vector2 hMinBorders;
    private Vector2 hMaxBorders;
    private Vector3 selectedWorldPosition;
    private Vector3 spawnPosition;
    private Vector3 destinationPosition;
    private bool isDestinationSelection;
    private bool isGameFinished;
    private int percentage => (int)((1 - placement.HpPercent) * 100);
    private int collectedArtifacts;

    public void PlaceHolder(Vector3 worldPos, bool isOnLand)
    {
        if (isOnLand == isDestinationSelection || IsAttackStarted)
            return;
        holder.SetActive(true);
        Vector3 screenPos =
                Camera.main.WorldToScreenPoint(worldPos);
        float x = Mathf.Clamp(screenPos.x, hMinBorders.x, hMaxBorders.x);
        float y = Mathf.Clamp(screenPos.y, hMinBorders.y, hMaxBorders.y);
        holder.transform.position = new Vector3(x, y);
        selectedWorldPosition = worldPos;
    }

    public void RestartGame()
    {
        placement.player.RegularSave();
        PlacementSystem.AttackMode = false;
        Time.timeScale = 1f;
        GameShouldFinish = false;
        IsAttackStarted = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void FinishGame()
    {
        isGameFinished = true;
        GameShouldFinish = false;
        FinishPopUp.SetActive(true);
        FinishPopUp.transform.GetChild(2)
        .GetComponentInChildren<Text>().text = percentage.ToString();
        int diviner = (percentage < minSuccess) ? restoreLost : restoreSuccess;
        int unitsLost = emitter.DestroyedCount;
        int toHeal = unitsLost / diviner;
        int maxToHeal = UnitsMenu.MaxPossibleUnits -
            placement.player.resourceSystem.Resources[ResourceType.UnitsToHeal]
            - placement.player.resourceSystem.Resources[ResourceType.Units];
        if (toHeal > maxToHeal) toHeal = maxToHeal;
        placement.player.TakeResources(toHeal, ResourceType.UnitsToHeal);
        var description = FinishPopUp.transform.GetChild(4);
        description.GetChild(0).GetComponent<Text>().text =
        $"Artifacts: {placement.CollectedArtifacts}\n\nUnits lost: {unitsLost - toHeal}";
        int stars = 0;
        if (percentage >= 100)
            stars = 3;
        else if (percentage >= 80)
            stars = 2;
        else if (percentage >= minSuccess)
            stars = 1;
        for (int i = 0; i < stars; i++)
            description.GetChild(1).GetChild(i).gameObject.SetActive(true);
        map.UpdateTime(stars);
        placement.player.SavePlayerResult(placement.CollectedArtifacts);
        Time.timeScale = 0f;
    }

    private void Awake()
    {
        int width = Screen.width;
        int height = Screen.height;
        float offsetX = width / widthKoef;
        float offsetY = height / heightKoef;
        hMinBorders =
                new Vector2(offsetX, offsetY);
        hMaxBorders =
            new Vector2(width - offsetX, height - offsetY);
        holder.transform.GetChild(0).GetComponent<Button>()
            .onClick.AddListener(OnSpawnClick);
    }

    private void Update()
    {
        if (isGameFinished) return;
        timer.text = TimeSpan.FromSeconds(timeLeft).ToString("hh':'mm':'ss");
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0 || GameShouldFinish) FinishGame();
    }

    private void OnSpawnClick()
    {
        var buttonT = holder.transform.GetChild(0);
        var button = buttonT.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnAttackClick);
        buttonT.GetChild(0).GetComponent<TextMeshProUGUI>().text = "ATTACK";
        holder.SetActive(false);
        spawnPosition = selectedWorldPosition;
        isDestinationSelection = true;
    }

    private void OnAttackClick()
    {
        holder.SetActive(false);
        destinationPosition = selectedWorldPosition;
        emitter.SetupForAttack(spawnPosition, destinationPosition);
        IsAttackStarted = true;
    }
}
