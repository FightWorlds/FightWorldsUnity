using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TechnoMap : MonoBehaviour
{
    [SerializeField] private Grid grid;
    [SerializeField] private GameObject hexPrefab;
    [SerializeField] private Vector2Int size;
    [SerializeField] private int radius;
    private void Start()
    {
        grid.cellSize = hexPrefab.GetComponent<RectTransform>().rect.size;
        int width = size.x / 2 - 1; // offset due to 0,0 hex
        int height = size.y / 2 - 1;
        for (int y = -height; y <= height; y++)
        {
            for (int x = -width; x <= width; x++)
            {
                var worldPos = grid.GetCellCenterWorld(new(x, y));
                var obj = Instantiate(hexPrefab, worldPos,
                    Quaternion.identity, grid.transform);
                obj.name = $"Boost {x} {y}";
                obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text
                    = $"X: {x}\nY: {y}";
            }
        }
    }

    private void Update()
    {

    }
}
