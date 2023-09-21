using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private List<Vector3> startPlatforms;
    [SerializeField] private List<StartBuilding> startBuildings;
    [SerializeField] private float radius;
    [SerializeField] private Vector3 heightOffset;
    [SerializeField] private BuildingsDatabase database;
    [SerializeField] private SoundFeedback soundFeedback;
    [SerializeField] private GameObject shuttlePrefab;
    [SerializeField] private float saveDelay;
    //public List<GameObject> objects;
    //public List<Vector3> pos;

    public PlayerController player { get; private set; }
    public UIController ui;
    public EvacuationSystem evacuation;
    public Func<bool, GameObject> GetBoomExplosion;

    private const int shuttleOffset = 16;
    private const float evacuateMultiplier = 0.9f;

    private int baseHp, baseMaxHp = 0;
    private int id = -1;
    private int buildingsCounter = 0;
    private List<GridObject> filledHexagons;
    private Dictionary<int, List<Building>> buildingsList;
    private GridInitializer initializer;
    private GridHex<GridObject> grid;
    private Building selectedBuilding;

    public float HpPercent => (float)baseHp / baseMaxHp;

    public void DamageBase(int damage)
    {
        baseHp -= damage;
        UpdateBaseHpSlider();
    }

    public void DestroyObj(Vector3 pos, Building building)
    {
        if (building != null)
            buildingsList[building.BuildingData.ID].Remove(building);
        grid.GetXZ(pos - heightOffset, out int x, out int z);
        grid.GetGridObject(x, z).HasBuilding = false;
        var boom = GetBoomExplosion(false);
        boom.transform.position = pos;
    }

    public void StartPlacement(int ID)
    {
        //TODO add preview
        soundFeedback.PlaySound(SoundType.Click);
        id = ID;
    }

    public List<Collider> GetBuildingsColliders() =>
    GetAllBuildings().Select(build => build.GetComponent<Collider>()).ToList();


    public void TapOnHex(Vector3 pos)
    {
        grid.GetXZ(pos, out int x, out int z);
        GridObject obj = grid.GetGridObject(x, z);
        if (id < 0)
            if (pos.y == heightOffset.y)
            {
                var building = obj.Hex.GetChild(1)
                .GetComponent<Building>();
                ui.ShowBuildingMenu(building);
                selectedBuilding = (building.State == BuildingState.Building) ?
                building : null;
            }
            else
            {
                ui.CloseBuildingMenu();
                MoveStructure(obj);
            }
        else if (id == 0)
            if (!obj.IsFilled && HaveFilledNeighbour(pos) &&
            LessThanLimit(database.objectsData[id]) &&
            player.UseResources(database.objectsData[id].Cost,
            ResourceType.Metal, true, () => FillHex(obj)))
                FillHex(obj);
            else
                WrongPlace();
        else
            PlaceStructure(obj, 0, true);
    }

    public FiringStats GetTurretsFiringStats()
    {
        int turrets = GetTurretsLimit();
        int firingDamage = (int)((turrets - 5) * player.VipMultiplier);
        int firingRate = turrets - 4;
        FiringStats npc = GetNPCFiringStats();
        int strength = (turrets * firingDamage * firingRate - npc.Damage * npc.Rate * npc.Strength) * (10 + Mathf.CeilToInt(turrets / 10f));
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
        return new FiringStats()
        {
            Damage = firingStat,
            Rate = firingStat,
            Strength = firingStat
        };
    }

    private void Awake()
    {
        // foreach (var obj in objects)
        //     pos.Add(obj.transform.position);
        player = new(ui);
        initializer = GetComponent<GridInitializer>();
        grid = initializer.GenerateHex();
        buildingsList = new Dictionary<int, List<Building>>() {
        { 0, new () }, { 1, new() }, { 2, new() }, { 3, new() },
        { 4, new() }, { 5, new() },{ 6, new() }, { 7, new() },
        { 8, new() }, { 9, new() }, { 10, new() }, {11, new()} };
        filledHexagons = new();
        foreach (Vector3 coords in startPlatforms)
            FillHex(coords);
        foreach (StartBuilding building in startBuildings)
        {
            id = building.ID;
            PlaceStructure(grid.GetGridObject(building.position),
            building.yRotationAngle, false);
        }
        StartCoroutine(Saving());
    }

    private List<Building> GetAllBuildings() =>
        buildingsList.Values.SelectMany(x => x).ToList();

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

    private void MoveStructure(GridObject newHex)
    {
        if (selectedBuilding == null) return;
        if (!newHex.IsFilled || newHex.HasBuilding)
        {
            selectedBuilding = null;
            WrongPlace();
            return;
        }
        grid.GetXZ(selectedBuilding.transform.position, out int x, out int z);
        GridObject prevHex = grid.GetGridObject(x, z);
        prevHex.HasBuilding = false;
        newHex.HasBuilding = true;
        selectedBuilding.transform.position =
            newHex.Hex.position + heightOffset;
        selectedBuilding.transform.SetParent(newHex.Hex);

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
            ResourceType.Metal, true, () =>
            Place(gridObject, rotation, playerPlace, data)) ||
            ui.IsProcessesFulled())
            {
                WrongPlace();
                return;
            }
            soundFeedback.PlaySound(SoundType.Place);
        }
        Place(gridObject, rotation, playerPlace, data);
    }

    private void Place(GridObject gridObject, int rotation,
    bool playerPlace, BuildingData data)
    {
        buildingsCounter++;
        gridObject.HasBuilding = true;
        GameObject obj = Instantiate(data.Prefab,
        gridObject.Hex.position + heightOffset,
        Quaternion.identity, gridObject.Hex);
        obj.name = ui.CutClone(obj.name) + " " + buildingsCounter;
        Building building = obj.GetComponent<Building>();
        building.placement = this;
        building.BuildingData = data;
        building.Rotate(rotation);
        building.OnBuilded = OnPlaceFinish;
        if (!playerPlace) building.PermanentBuild();
        StopPlacement();
    }

    private void OnPlaceFinish(Building building)
    {
        buildingsList[building.BuildingData.ID].Add(building);
        int newHp = GetTurretsFiringStats().Strength;
        baseHp += newHp;
        baseMaxHp += newHp;
        UpdateBaseHpSlider();
    }

    public void StopPlacement() => id = -1;

    private void WrongPlace()
    {
        soundFeedback.PlaySound(SoundType.WrongPlacement);
    }

    private void FillHex(GridObject obj)
    {
        obj.FillHex();
        filledHexagons.Add(obj);
        if (id == 0)
            buildingsList[id].Add(obj.Hex.GetComponent<Building>());
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
        int count = buildingsList[id].Count;
        if (id == 8) // turret
            return count < GetTurretsLimit();
        else if (id == 11) // wall
            return count < GetWallsLimit();
        else
            return count <
            data.MaxBuildingsPerLevel * player.Level() + data.MaxBuildingsAdd;
    }

    private void OnEnable() => player.NewLevel += OnNewLevel;
    private void OnDisable() => player.NewLevel -= OnNewLevel;

    private void OnNewLevel()
    {
        foreach (var building in GetAllBuildings())
            building.UpdateStats(GetTurretsFiringStats());
    }

    private IEnumerator Saving()
    {
        while (evacuation == null || !evacuation.IsGameFinished)
        {
            yield return new WaitForSeconds(saveDelay);
            player.RegularSave();
        }

    }
}