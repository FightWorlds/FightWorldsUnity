using UnityEngine;

namespace FightWorlds.Grid
{
    public class GridInitializer : MonoBehaviour
    {
        [SerializeField] private Vector3 offset;
        [SerializeField] private Vector3 halfExtents;
        [SerializeField] private Material hexMaterial;
        [SerializeField] private LayerMask hexMask;
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private int cellSize;
        [SerializeField] private int radius;

        public GridHex<GridObject> GenerateHex()
        {
            GridHex<GridObject> gridHex =
                new GridHex<GridObject>(width, height, cellSize, offset,
                (GridHex<GridObject> g, int x, int y) =>
                new GridObject(hexMaterial, x, y));
            // TODO: redo gridhex array as list?
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    Vector3 worldPos = gridHex.GetWorldPosition(x, z);
                    float dist = Vector3.Distance(worldPos, Vector3.zero);
                    if (Mathf.Abs(dist) > radius)
                        continue;
                    Collider[] colliders = Physics.OverlapBox(worldPos + Vector3.up,
                    halfExtents, Quaternion.identity, hexMask);
                    if (colliders.Length == 0)
                        continue;
                    Transform visualTransform = colliders[0].transform.parent;
                    gridHex.GetGridObject(x, z).Hex =
                    visualTransform;
                    visualTransform.name = $"Hex {x} {z}";
                }
            }
            return gridHex;
        }
    }
}