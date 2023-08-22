using System.Collections.Generic;
using UnityEngine;

public class GridInitializer : MonoBehaviour
{
    [SerializeField] private Transform pfHex;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Material hexMaterial;
    [SerializeField] private int width;
    [SerializeField] private int height;
    [SerializeField] private int cellSize;
    [SerializeField] private int radius;
    [SerializeField] private List<Vector3> startPlatforms;

    public GridManager gridManager { get; private set; }

    private GridHex<GridObject> gridHex;

    private void Awake()
    {
        gridHex =
            new GridHex<GridObject>(width, height, cellSize, offset, (GridHex<GridObject> g, int x, int y) => new GridObject(hexMaterial, x, y));

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 worldPos = gridHex.GetWorldPosition(x, z);
                float dist = Vector3.Distance(worldPos, Vector3.zero);
                if (Mathf.Abs(dist) < radius)
                {
                    Transform visualTransform = Instantiate(pfHex,
                        worldPos,
                        Quaternion.identity, transform);
                    gridHex.GetGridObject(x, z).VisualTransform =
                    visualTransform;
                }
            }
        }
        gridManager = new GridManager(gridHex, startPlatforms);
    }
}