// CREDITS for: PetterSunnyVR

using System.Collections.Generic;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField] private List<Vector3> startPlatforms;
    [SerializeField] private float radius;
    [SerializeField] private Vector3 heightOffset;
    [SerializeField] private BuildingsDatabase database;
    [SerializeField] private SoundFeedback soundFeedback;

    private int id = -1;
    private List<GridObject> filledHexagons;
    private List<GameObject> placedBuildings;
    private GridInitializer initializer;
    private GridHex<GridObject> grid;

    private void Awake()
    {
        initializer = GetComponent<GridInitializer>();
        grid = initializer.GenerateHex();
        filledHexagons = new();
        foreach (Vector3 coords in startPlatforms)
            FillHex(coords);
    }

    public void StartPlacement(int ID)
    {
        //TODO add preview
        soundFeedback.PlaySound(SoundType.Click);
        id = ID;
    }

    private void PlaceStructure(GridObject gridObject)
    {
        if (gridObject.HasBuilding || !gridObject.IsFilled)
        {
            WrongPlace();
            return;
        }
        soundFeedback.PlaySound(SoundType.Place);
        gridObject.HasBuilding = true;
        Instantiate(database.objectsData[id].Prefab,
        gridObject.Hex.position + heightOffset,
        Quaternion.identity, gridObject.Hex);

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
            if (!obj.IsFilled)
                if (HaveFilledNeighbour(pos))
                    FillHex(obj);
                else
                    WrongPlace();
            else
                WrongPlace();
        else
            PlaceStructure(obj);
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
}