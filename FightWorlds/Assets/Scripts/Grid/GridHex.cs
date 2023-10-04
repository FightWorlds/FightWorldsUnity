using System;
using UnityEngine;

namespace FightWorlds.Grid
{
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
        public TGridObject[,] gridArray { get; private set; }

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
            return
                new Vector3(x, 0, 0) * cellSize +
                new Vector3(0, 0, x) * 1.7f + // flower
                new Vector3(z, 0, 0) + // flower
                new Vector3(0, 0, z) * cellSize +
                originPosition;
        }

        public void GetXZ(Vector3 worldPosition, out int x, out int z)
        {
            Vector3 offset = worldPosition - originPosition;
            x = Mathf.RoundToInt((offset.x * 50 - offset.z * 10) / 233); // formula
            z = Mathf.RoundToInt((offset.z * 50 - offset.x * 17) / 233);
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
}