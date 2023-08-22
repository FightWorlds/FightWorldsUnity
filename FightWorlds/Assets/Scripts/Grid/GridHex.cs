using System;
using System.Collections.Generic;
using UnityEngine;

public class GridHex<TGridObject>
{
    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs
    {
        public int x;
        public int z;
    }

    private const float HEX_VERTICAL_OFFSET_MULTIPLIER = 0.75f;

    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    public GridHex(int width, int height,
        float cellSize, Vector3 originPosition,
        Func<GridHex<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        for (int x = 0; x < gridArray.GetLength(0); x++)
            for (int z = 0; z < gridArray.GetLength(1); z++)
                gridArray[x, z] = createGridObject(this, x, z);
    }

    public int GetWidth() => width;

    public int GetHeight() => height;

    public float GetCellSize() => cellSize;

    public TGridObject[,] GetGridArray() => gridArray;

    public Vector3 GetWorldPosition(int x, int z)
    {
        //return
        //    new Vector3(x, 0, 0) * cellSize +
        //    new Vector3(0, 0, x) * 1.75f + // flower
        //    new Vector3(z, 0, 0) + // flower
        //    new Vector3(0, 0, z) * cellSize +
        //    originPosition;
        return
        new Vector3(x, 0, 0) * cellSize +
        new Vector3(0, 0, z) * cellSize * HEX_VERTICAL_OFFSET_MULTIPLIER +
        ((Mathf.Abs(z) % 2) == 1 ? new Vector3(1, 0, 0) *
        cellSize * .5f : Vector3.zero) +
        originPosition;

    }

    public void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        int roughX = Mathf.RoundToInt((worldPosition - originPosition).x
        / cellSize);
        int roughZ = Mathf.RoundToInt((worldPosition - originPosition).z
        / cellSize / HEX_VERTICAL_OFFSET_MULTIPLIER);

        Vector3Int roughXZ = new Vector3Int(roughX, 0, roughZ);

        bool oddRow = roughZ % 2 == 1;

        List<Vector3Int> neighbourXZList = new List<Vector3Int> {
             roughXZ + new Vector3Int(-1, 0, 0),
             roughXZ + new Vector3Int(+1, 0, 0),

             roughXZ + new Vector3Int(oddRow ? +1 : -1, 0, +1),
             roughXZ + new Vector3Int(+0, 0, +1),

             roughXZ + new Vector3Int(oddRow ? +1 : -1, 0, -1),
             roughXZ + new Vector3Int(+0, 0, -1),
        };

        Vector3Int cXZ = roughXZ;

        foreach (Vector3Int nXZ in neighbourXZList)
            if (Vector3.Distance(worldPosition, GetWorldPosition(nXZ.x, nXZ.z))
                <
                Vector3.Distance(worldPosition, GetWorldPosition(cXZ.x, cXZ.z)))
                cXZ = nXZ; // Closer than closest

        x = cXZ.x;
        z = cXZ.z;
    }

    public void SetGridObject(int x, int z, TGridObject value)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
        {
            gridArray[x, z] = value;
            TriggerGridObjectChanged(x, z);
        }
    }

    public void TriggerGridObjectChanged(int x, int z)
    {
        OnGridObjectChanged?.Invoke(this,
            new OnGridObjectChangedEventArgs { x = x, z = z });
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value)
    {
        GetXZ(worldPosition, out int x, out int z);
        SetGridObject(x, z, value);
    }

    public TGridObject GetGridObject(int x, int z)
    {
        if (x >= 0 && z >= 0 && x < width && z < height)
            return gridArray[x, z];
        else
            return default(TGridObject);
    }

    public TGridObject GetGridObject(Vector3 worldPosition)
    {
        GetXZ(worldPosition, out int x, out int z);
        return GetGridObject(x, z);
    }
}
