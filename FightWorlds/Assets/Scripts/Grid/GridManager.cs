using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using UnityEngine;

public class GridManager
{
    private readonly Vector3 unclick = new Vector3(-999999, 0, 0);
    private readonly float r = 7.5f;

    private GridHex<GridObject> grid;
    private List<GridObject> filledHexagons;
    private Vector2 prevClickHex;

    public GridManager(GridHex<GridObject> grid, List<Vector3> startPlatforms)
    {
        this.grid = grid;
        filledHexagons = new List<GridObject>();
        foreach (Vector3 coords in startPlatforms)
            FillHex(coords);
    }

    public void TapOnHex(Vector3 pos)
    {
        grid.GetXZ(pos, out int x, out int z);
        Vector2 clickHex = new Vector2(x, z);
        if (prevClickHex == clickHex)
        {
            GridObject obj = grid.GetGridObject(x, z);
            if (!obj.IsFilled)
                if (HaveFilledNeighbour(pos))
                    FillHex(obj);
            prevClickHex = unclick;
        }
        else
            prevClickHex = clickHex;
        Debug.Log($"x: {x} z: {z}");
    }

    private void FillHex(GridObject obj)
    {
        obj.FillHex();
        filledHexagons.Add(obj);
    }

    private void FillHex(Vector3 pos)
    {
        grid.GetXZ(pos, out int x, out int z);
        GridObject obj = grid.GetGridObject(x, z);
        obj.FillHex();
        filledHexagons.Add(obj);
    }

    private bool HaveFilledNeighbour(Vector3 pos)
    {
        return filledHexagons.FindAll(
            hex => Vector3.Distance(pos,
            grid.GetWorldPosition(hex.X, hex.Z)) <= r)
            .Find(h => h.IsFilled) != null;
    }
}