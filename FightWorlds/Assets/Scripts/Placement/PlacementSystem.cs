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
    [SerializeField] private GameObject shuttlePrefab;
    //public List<GameObject> objects;
    //public List<Vector3> pos;

    public PlayerController player { get; private set; }
    public UIController ui;
    public EvacuationSystem evacuation;

    private const int shuttleOffset = 16;
    private const float evacuateMultiplier = 0.9f;

    private int baseHp, baseMaxHp = 0;
    private int id = -1;
    private List<GridObject> filledHexagons;
    private Dictionary<int, int> buildingCount;
    private GridInitializer initializer;
    private GridHex<GridObject> grid;

    public float HpPercent => (float)baseHp / baseMaxHp;

    public void DamageBase(int damage)
    {
        baseHp -= damage;
        UpdateBaseHpSlider();
    }

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
            PlaceStructure(obj, 0, true);
    }

    private void Awake()
    {
        // foreach (var obj in objects)
        //     pos.Add(obj.transform.position);
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
            PlaceStructure(grid.GetGridObject(building.position),
            building.yRotationAngle, false);
        }
    }

    private void UpdateBaseHpSlider()
    {
        ui.UpdateBaseHpBar(HpPercent);
        if (HpPercent < evacuateMultiplier && evacuation == null)
        {
            evacuation = Instantiate(shuttlePrefab, startBuildings[0].position +
                Vector3.up * shuttleOffset, Quaternion.identity)
                .GetComponent<EvacuationSystem>();
            evacuation.placement = this;
        }
    }

    private void PlaceStructure(GridObject gridObject,
    int rotation, bool playerPlace)
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
        // TODO add order of building to name
        Building building = obj.GetComponent<Building>();
        building.placement = this;
        building.BuildingData = data;
        building.Rotate(rotation);
        if (!playerPlace) building.PermanentBuild(true, true);
        int newHp = GetTurretsFiringStats().Strength;
        baseHp += newHp;
        baseMaxHp += newHp;
        UpdateBaseHpSlider();
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

    // NUMBERS TAKEN FROM <<GDD: Damage model>>
    public int GetTurretsLimit() => 6 + Mathf.FloorToInt(player.Level() / 6);
    private int GetWallsLimit() => 24 + GetTurretsLimit() * 5;
    private bool LessThanLimit(BuildingData data)
    {
        if (id == 8) // turret
            return buildingCount[id] < GetTurretsLimit();
        else if (id == 11) // wall
            return buildingCount[id] < GetWallsLimit();
        else
            return buildingCount[id] <
            data.MaxBuildingsPerLevel * player.Level() + data.MaxBuildingsAdd;
    }
    public FiringStats GetTurretsFiringStats()
    {
        int turrets = GetTurretsLimit();
        int firingDamage = turrets - 5;
        int firingRate = turrets - 4;
        FiringStats npc = GetNPCFiringStats();
        int strength = turrets * firingDamage * firingRate - (npc.Damage * npc.Rate * npc.Strength * (10 + Mathf.CeilToInt(turrets / 10)));
        return new FiringStats()
        {
            Damage = firingDamage,
            Rate = firingRate,
            Strength = strength
        };
    }

    public FiringStats GetNPCFiringStats()
    {
        int turrets = GetTurretsLimit();
        int firingStat = turrets - 5;
        //  - NPC health * NPC Damage * NPC FireRate
        return new FiringStats()
        {
            Damage = firingStat,
            Rate = firingStat,
            Strength = firingStat
        };
    }

}