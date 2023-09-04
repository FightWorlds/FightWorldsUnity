// CREDITS for: PetterSunnyVR

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private List<Vector3> startPlatforms;
    [SerializeField] private List<StartBuilding> startBuildings;
    [SerializeField] private float radius;
    [SerializeField] private Vector3 heightOffset;
    [SerializeField] private BuildingsDatabase database;
    [SerializeField] private SoundFeedback soundFeedback;
    [SerializeField] private GameObject evacuationButton;
    [SerializeField] private GameObject shuttlePrefab;

    public PlayerController player;
    public UIController ui;

    private const int shuttleOffset = 16;
    private const float evacuateMultiplier = 0.3f;

    private int baseHp, baseMaxHp = 0;
    private bool isShuttleCalled;
    private int id = -1;
    private List<GridObject> filledHexagons;
    private GridInitializer initializer;
    private GridHex<GridObject> grid;
    private Shuttle shuttle;

    private void Awake()
    {
        initializer = GetComponent<GridInitializer>();
        grid = initializer.GenerateHex();
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
        if (baseHp < baseMaxHp * evacuateMultiplier && !isShuttleCalled)
        {
            evacuationButton.SetActive(true);
            evacuationButton.GetComponent<Button>().onClick.AddListener(
            () =>
            {
                if (isShuttleCalled)
                    return;
                var obj = Instantiate(shuttlePrefab, startBuildings[0].position +
                Vector3.up * shuttleOffset, Quaternion.identity);
                isShuttleCalled = true;
                shuttle = obj.GetComponent<Shuttle>();
                evacuationButton.SetActive(false);
            });
        }
        if (baseHp <= 0)
            if (isShuttleCalled)
            {
                Debug.Log("You win");
                Debug.Log($"Player collect {player.GetArtifactsCount()} artifacts");
                shuttle.Evacuate();
            }
            else
            {
                Debug.Log("You lose");
                evacuationButton.SetActive(false);
            }

    }

    public void StartPlacement(int ID)
    {
        //TODO add preview
        soundFeedback.PlaySound(SoundType.Click);
        id = ID;
    }

    private void PlaceStructure(GridObject gridObject, bool withSound)
    {
        if (gridObject.HasBuilding || !gridObject.IsFilled)
        {
            WrongPlace();
            return;
        }
        if (withSound)
        {
            if (!player.UseResources(database.objectsData[id].Cost,
            ResourceType.Metal))
            {
                WrongPlace();
                return;
            }
            soundFeedback.PlaySound(SoundType.Place);
        }

        gridObject.HasBuilding = true;
        GameObject building = Instantiate(database.objectsData[id].Prefab,
        gridObject.Hex.position + heightOffset,
        Quaternion.identity, gridObject.Hex);
        Damageable damageable = building.GetComponent<Damageable>();
        damageable.placement = this;
        int newHp = damageable.Hp;
        baseHp += newHp;
        baseMaxHp += newHp;
        StopPlacement();
    }

    private void StopPlacement()
    {
        id = -1;
    }

    private void WrongPlace()
    {
        soundFeedback.PlaySound(SoundType.WrongPlacement);
    }

    public void TapOnHex(Vector3 pos)
    {
        if (id < 0)
            return;
        grid.GetXZ(pos, out int x, out int z);
        GridObject obj = grid.GetGridObject(x, z);
        Debug.Log($"x{x} z{z}");
        if (id == 0)
            if (!obj.IsFilled && HaveFilledNeighbour(pos)
            && player.UseResources(database.objectsData[id].Cost,
            ResourceType.Metal))
                FillHex(obj);
            else
                WrongPlace();
        else
            PlaceStructure(obj, true);
    }

    private void FillHex(GridObject obj)
    {
        obj.FillHex();
        filledHexagons.Add(obj);
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

    public void DamageBase(int damage, Vector3 pos, bool isDead)
    {
        baseHp -= damage;
        if (isDead)
        {
            grid.GetXZ(pos, out int x, out int z);
            GridObject obj = grid.GetGridObject(x, z);
            obj.IsDestroyed = true;
        }
    }

    public Collider[] GetBuildingsColliders()
    {
        return
            filledHexagons.FindAll(hex => hex.HasBuilding && !hex.IsDestroyed)
            .Select(hex => hex.Hex.GetChild(1).GetComponent<Collider>())
            .ToArray();
    }
}