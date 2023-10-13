using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using FightWorlds.UI;
using FightWorlds.Controllers;
using FightWorlds.Combat;
using FightWorlds.Grid;
using FightWorlds.Audio;
using FightWorlds.Boost;

namespace FightWorlds.Placement
{
    public class PlacementSystem : MonoBehaviour
    {
        public static bool AttackMode;

        [SerializeField] private List<Vector3> startPlatforms;
        [SerializeField] private List<Vector3> enemyStartPlatforms;
        [SerializeField] private List<StartBuilding> startBuildings;
        [SerializeField] private List<StartBuilding> enemyStartBuildings;
        [SerializeField] private float radius;
        [SerializeField] private Vector3 heightOffset;
        [SerializeField] private BuildingsDatabase database;
        [SerializeField] private GameObject shuttlePrefab;
        [SerializeField] private Material hexMaterial;
        [SerializeField] private float saveDelay;
        [SerializeField] protected int turretAttackRadius;
        [SerializeField] protected int npcAttackRadius;
        //public List<GameObject> objects;
        //public List<Vector3> pos;

        public SoundFeedback soundFeedback;
        public PlayerController player;
        public UIController ui;
        public EvacuationSystem evacuation;
        public Func<bool, GameObject> GetBoomExplosion;

        private const int randomDist = 10;
        private const int rotationAngle = 60;
        private const int shuttleOffset = 13;
        private const int artifactsPerBuilding = 15;
        private const float evacuateMultiplier = 0.9f;
        private const float boostMltpl = 0.125f;

        private int baseHp, baseMaxHp = 0;
        private int id = -1;
        private int buildingsCounter = 0;
        private List<GridObject> filledHexagons;
        private Dictionary<int, List<Building>> buildingsList;
        private GridInitializer initializer;
        private GridHex<GridObject> grid;
        private Building selectedBuilding;

        public float HpPercent => (float)baseHp / baseMaxHp;
        public int CollectedArtifacts { get; private set; }

        public void UpdateBaseHp(int damage)
        {
            baseHp -= damage;
            UpdateBaseHpSlider();
            if (baseHp <= 0) AttackManagementUI.GameShouldFinish = true;
        }

        public void DestroyObj(Vector3 pos, Building building)
        {
            if (building != null)
                buildingsList[building.BuildingData.ID].Remove(building);
            grid.GetXZ(pos - heightOffset, out int x, out int z);
            grid.GetGridObject(x, z).HasBuilding = false;
            var boom = GetBoomExplosion(false);
            boom.transform.position = pos;
            soundFeedback.PlaySound(SoundType.Destroy);
            if (AttackMode) CollectedArtifacts += artifactsPerBuilding;
        }

        public void StartPlacement(int ID)
        {
            soundFeedback.PlaySound(SoundType.Click);
            id = ID;
        }

        public List<Collider> GetBuildingsColliders() =>
            GetAllBuildings().Select(build =>
                build.GetComponent<Collider>()).ToList();

        public void TapOnLand(Vector3 pos, bool isOnLand)
        {
            Debug.Log(pos + " " + isOnLand);
            ui.PlaceHolder(pos, isOnLand);
            // TODO: place spawn point here
        }

        public void TapOnHex(Vector3 pos)
        {
            grid.GetXZ(pos, out int x, out int z);
            GridObject obj = grid.GetGridObject(x, z);
            if (id < 0)
                SelectBuilding(pos, obj);
            else if (id == 0)
                PlacePlatform(obj, pos);
            else
                PlaceStructure(obj, 0, true);
        }

        public bool DragOnHex(Vector3 pos)
        {
            grid.GetXZ(pos, out int x, out int z);
            GridObject obj = grid.GetGridObject(x, z);
            if (id < 0)
                return MoveStructure(obj);
            return false;
        }

        public FiringStats GetTurretsFiringStats()
        {
            Dictionary<BoostType, int> boosts = ui.GetActiveBoosts();
            int turrets = GetTurretsLimit();
            int firingDamage = turrets - 5;
            firingDamage +=
                (int)(firingDamage * boosts[BoostType.Damage] * boostMltpl);
            int firingRate = turrets - 4;
            firingRate +=
                (int)(firingRate * boosts[BoostType.Rate] * boostMltpl);
            FiringStats npc = GetNPCFiringStats();
            int strength = (turrets * firingDamage * firingRate - npc.Damage * npc.Rate * npc.Strength) * (10 + Mathf.CeilToInt(turrets / 10f));
            strength +=
                (int)(strength * boosts[BoostType.Health] * boostMltpl);
            int range = turretAttackRadius +
                (int)(turretAttackRadius * boosts[BoostType.Range] * boostMltpl);
            return new FiringStats()
            {
                Damage = firingDamage,
                Rate = firingRate,
                Strength = strength,
                Range = range
            };
        }

        public FiringStats GetNPCFiringStats()
        {
            int turrets = GetTurretsLimit();
            int firingStat = turrets - 5;
            int range = npcAttackRadius;
            return new FiringStats()
            {
                Damage = firingStat,
                Rate = firingStat,
                Strength = firingStat,
                Range = range
            };
        }

        public void ResetSelectedBuilding() => selectedBuilding = null;

        private void Awake()
        {
            //foreach (var obj in objects)
            //    pos.Add(obj.transform.position);
            player = new(ui);
            initializer = GetComponent<GridInitializer>();
            grid = initializer.GenerateHex();
            buildingsList = new Dictionary<int, List<Building>>() {
            { 1, new() }, { 2, new() }, { 3, new() },
            { 4, new() }, { 5, new() },{ 6, new() }, { 7, new() },
            { 8, new() }, { 9, new() }, { 10, new() }, {11, new()} };
            filledHexagons = new();
            List<Vector3> platforms =
                AttackMode ? enemyStartPlatforms : startPlatforms;
            foreach (Vector3 coords in platforms)
            {
                grid.GetXZ(coords, out int x, out int z);
                GridObject obj = grid.GetGridObject(x, z);
                FillHex(obj);
            }
            StartCoroutine(Saving());
            StartCoroutine(SecondFrameTask());
            if (AttackMode)
            {
                // TODO turn off extra features for that mode
                // UI elements, etc
                ui.SwitchMainCanvas(false);
                ui.ShowAttackCanvas();
            }
        }

        private IEnumerator SecondFrameTask()
        {
            yield return null;
            player.resourceSystem.Resources[ResourceType.Units] = 30; // REMOVE
            List<StartBuilding> buildings =
                AttackMode ? enemyStartBuildings : startBuildings;
            foreach (StartBuilding building in buildings)
            {
                id = building.ID;
                PlaceStructure(grid.GetGridObject(building.position),
                building.yRotationAngle, false);
            }
        }

        private List<Building> GetAllBuildings() =>
            buildingsList.Values.SelectMany(x => x)
            .Where(b => b.State != BuildingState.Building).ToList();

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

        private bool MoveStructure(GridObject newHex)
        {
            if (selectedBuilding == null) return false;
            if (!newHex.IsFilled || newHex.HasBuilding)
                return true;
            grid.GetXZ(selectedBuilding.transform.position,
                out int x, out int z);
            GridObject prevHex = grid.GetGridObject(x, z);
            prevHex.HasBuilding = false;
            newHex.HasBuilding = true;
            selectedBuilding.transform.position =
                newHex.Hex.position + heightOffset;
            selectedBuilding.transform.SetParent(newHex.Hex);
            ui.CloseBuildingMenu();
            return true;
        }

        private void SelectBuilding(Vector3 pos, GridObject obj)
        {
            if (selectedBuilding == null)
            {
                if (pos.y != heightOffset.y)
                    return;
                Building building = null;
                foreach (Transform child in obj.Hex)
                    if (child.TryGetComponent(out building))
                        ui.ShowBuildingMenu(building);
                selectedBuilding =
                (building.State == BuildingState.Building &&
                !building.IsProducing) ? building : null;

            }
            else
                ResetSelectedBuilding();
        }

        private void PlacePlatform(GridObject obj, Vector3 pos)
        {
            if (!obj.IsFilled && HaveFilledNeighbour(pos) &&
                LessThanLimit(database.objectsData[id]) &&
                player.UseResources(database.objectsData[id].Cost,
                ResourceType.Metal, true, () => FillHex(obj)))
                FillHex(obj);
            else
                WrongPlace();
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
                Place(gridObject, rotation, playerPlace, data)))
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
            obj.name = data.Name + " " + buildingsCounter;
            Building building = obj.GetComponent<Building>();
            building.placement = this;
            building.BuildingData = data;
            building.Rotate(rotation);
            building.OnBuilded = OnPlaceFinish;
            buildingsList[building.BuildingData.ID].Add(building);
            if (!playerPlace) building.PermanentBuild();
            StopPlacement();
        }

        private void OnPlaceFinish(Building building)
        {

            int newHp = GetTurretsFiringStats().Strength;
            // what if boost expired?
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
            obj.Hex.GetChild(0).GetComponent<MeshRenderer>()
            .material = hexMaterial;
            obj.Hex.GetChild(1).gameObject.SetActive(true);
            filledHexagons.Add(obj);
            StopPlacement();
        }

        private bool HaveFilledNeighbour(Vector3 pos)
        {
            return filledHexagons.FindAll(
                hex => Vector3.Distance(pos,
                grid.GetWorldPosition(hex.X, hex.Z)) <= radius)
                .Find(h => h.IsFilled) != null;
        }

        // NUMBERS TAKEN FROM <<GDD: Damage model>>
        public int GetTurretsLimit() =>
            6 + Mathf.FloorToInt(player.Level() / 6);

        private int GetWallsLimit() =>
            24 + GetTurretsLimit() * 5;

        private bool LessThanLimit(BuildingData data)
        {
            int count = id == 0 ?
            filledHexagons.Count : buildingsList[id].Count;
            if (id == 8) // turret
                return count < GetTurretsLimit();
            else if (id == 11) // wall
                return count < GetWallsLimit();
            else
                return count <
                data.MaxBuildingsPerLevel *
                player.Level() + data.MaxBuildingsAdd;
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
}