using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Collections;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private List<Vector3> startPlatforms;
    [SerializeField] private List<StartBuilding> startBuildings;
    [SerializeField] private float radius;
    [SerializeField] private Vector3 heightOffset;
    [SerializeField] private BuildingsDatabase database;
    [SerializeField] private SoundFeedback soundFeedback;
    [SerializeField] private Button callButton;
    [SerializeField] private Button evacuationButton;
    [SerializeField] private Slider baseHpSlider;
    [SerializeField] private GameObject shuttlePrefab;

    public PlayerController player { get; private set; }
    public UIController ui;

    private const int shuttleOffset = 16;
    private const float evacuateMultiplier = 0.5f;
    private const float evacuateOperationTime = 1f;
    private const int artifactsPerOperation = 5;

    private int baseHp, baseMaxHp = 0;
    private bool isShuttleCalled;
    private bool isGameFinished;
    private int collectedArtifacts;
    private int id = -1;
    private List<GridObject> filledHexagons;
    private Dictionary<int, int> buildingCount;
    private GridInitializer initializer;
    private GridHex<GridObject> grid;
    private Shuttle shuttle;

    public void DamageBase(int damage) => baseHp -= damage;

    public void DestroyObj(Vector3 pos)
    {
        grid.GetXZ(pos - heightOffset, out int x, out int z);
        GridObject obj = grid.GetGridObject(x, z);
        obj.IsDestroyed = true;
    }

    public void StartPlacement(int ID)
    {
        //TODO add preview
        soundFeedback.PlaySound(SoundType.Click);
        id = ID;
    }

    public Collider[] GetBuildingsColliders()
    {
        return
            filledHexagons.FindAll(hex => hex.HasBuilding && !hex.IsDestroyed)
            .Select(hex => hex.Hex.GetChild(1).GetComponent<Collider>())
            .Where(build => build.GetComponent<Building>()
            .State != BuildingState.Building)
            .ToArray();
    }

    public void TapOnHex(Vector3 pos)
    {
        grid.GetXZ(pos, out int x, out int z);
        GridObject obj = grid.GetGridObject(x, z);
        Debug.Log($"x{x} z{z}");
        if (id < 0)
            if (pos.y == heightOffset.y)
                ui.ShowBuildingMenu(obj.Hex.GetChild(1)
                .GetComponent<Building>());
            else
                ui.CloseBuildingMenu();
        else if (id == 0)
            if (!obj.IsFilled && HaveFilledNeighbour(pos)
            && player.UseResources(database.objectsData[id].Cost,
            ResourceType.Metal, true) && LessThanLimit(database.objectsData[id]))
                FillHex(obj);
            else
                WrongPlace();
        else
            PlaceStructure(obj, true);
    }

    public void FinishGame()
    {
        if (isGameFinished) return;
        evacuationButton.gameObject.SetActive(false);
        isGameFinished = true;
        if (isShuttleCalled)
        {
            Debug.Log("You win");
            Debug.Log($"Player collect {collectedArtifacts} artifacts");
            shuttle.Evacuate();
        }
        else
        {
            Debug.Log("You lose");
        }
    }

    private void Awake()
    {
        player = new(ui);
        initializer = GetComponent<GridInitializer>();
        grid = initializer.GenerateHex();
        buildingCount = new Dictionary<int, int>() {
        { 0, 0 }, { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 },
        { 6, 0 }, { 7, 0 }, { 8, 0 }, { 9, 0 }, { 10, 0 }, {11, 0} };
        filledHexagons = new();
        foreach (Vector3 coords in startPlatforms)
            FillHex(coords);
        foreach (StartBuilding building in startBuildings)
        {
            id = building.ID;
            PlaceStructure(grid.GetGridObject(building.position), false);
        }
    }

    private void Update()
    {
        // TODO Move out base + shuttle logic into other classes
        baseHpSlider.value = (float)baseHp / baseMaxHp;
        if (baseHp < baseMaxHp * evacuateMultiplier && !isShuttleCalled)
        {
            callButton.gameObject.SetActive(true);
            callButton.onClick.AddListener(() =>
            { if (!isShuttleCalled) StartCoroutine(CollectArtifacts()); });
        }
    }

    private IEnumerator CollectArtifacts()
    {
        var obj = Instantiate(shuttlePrefab, startBuildings[0].position +
                Vector3.up * shuttleOffset, Quaternion.identity);
        isShuttleCalled = true;
        shuttle = obj.GetComponent<Shuttle>();
        callButton.gameObject.SetActive(false);
        int landingTime = 6;
        yield return new WaitForSeconds(landingTime);
        while (player.UseResources(artifactsPerOperation,
        ResourceType.Artifacts, false) && !isGameFinished)
        {
            collectedArtifacts += artifactsPerOperation;
            yield return new WaitForSeconds(evacuateOperationTime);
        }
        FinishGame();
    }

    private void PlaceStructure(GridObject gridObject, bool playerPlace)
    {
        BuildingData data = database.objectsData[id];
        if (gridObject.HasBuilding ||
        !gridObject.IsFilled || !LessThanLimit(data))
        {
            WrongPlace();
            return;
        }
        if (playerPlace)
        {
            if (!player.UseResources(data.Cost,
            ResourceType.Metal, true) || ui.IsProcessesFulled())
            {
                WrongPlace();
                return;
            }
            soundFeedback.PlaySound(SoundType.Place);
        }
        buildingCount[id]++;
        gridObject.HasBuilding = true;
        GameObject obj = Instantiate(data.Prefab,
        gridObject.Hex.position + heightOffset,
        Quaternion.identity, gridObject.Hex);
        Building building = obj.GetComponent<Building>();
        building.placement = this;
        building.BuildingData = data;
        if (!playerPlace) building.PermanentBuild(true, true);
        int newHp = building.Hp;
        baseHp += newHp;
        baseMaxHp += newHp;
        StopPlacement();
    }

    private void StopPlacement() => id = -1;

    private void WrongPlace()
    {
        soundFeedback.PlaySound(SoundType.WrongPlacement);
    }

    private void FillHex(GridObject obj)
    {
        obj.FillHex();
        filledHexagons.Add(obj);
        if (id == 0) buildingCount[id]++;
        StopPlacement();
    }

    private void FillHex(Vector3 pos)
    {
        grid.GetXZ(pos, out int x, out int z);
        GridObject obj = grid.GetGridObject(x, z);
        FillHex(obj);
    }

    private bool HaveFilledNeighbour(Vector3 pos)
    {
        return filledHexagons.FindAll(
            hex => Vector3.Distance(pos,
            grid.GetWorldPosition(hex.X, hex.Z)) <= radius)
            .Find(h => h.IsFilled) != null;
    }

    private bool LessThanLimit(BuildingData data) =>
        buildingCount[id] <
        data.MaxBuildingsPerLevel * player.Level() + data.MaxBuildingsAdd;
}