using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace FightWorlds.UI
{
    public enum BoostType
    {
        None = 0,
        Damage = 1,
        Rate = 2,
        Range = 3,
        Health = 4
    }

    [Serializable]
    public class BoostCell
    {
        public Vector3Int GridCoords;
        public BoostType Type;
        public float TimeLeft;

        public BoostCell(Vector3Int coordinates,
        BoostType type, float time)
        {
            GridCoords = coordinates;
            Type = type;
            TimeLeft = time;
        }
    }

    [Serializable]
    public struct BoostTimeExpire
    {
        public float Time;
        public Color Color;
    }

    public class TechnoMap : MonoBehaviour
    {
        [SerializeField] private BoostTimeExpire[] ColorByTime;
        [SerializeField] private UnityEngine.Grid grid;
        [SerializeField] private GameObject hexPrefab;
        [SerializeField] private Vector2Int size;
        [SerializeField] private int radius;

        private const float maxTime = 86400; // day in sec
        private const float addTime = 10800; // 3 hours

        public List<BoostCell> BoostsList;

        private void Start()
        {
            grid.cellSize = hexPrefab.GetComponent<RectTransform>().rect.size;
            //BoostsList = new();
            //int width = size.x / 2 - 1; // offset due to 0,0 hex
            //int height = size.y / 2 - 1;
            //for (int y = -height; y <= height; y++)
            //{
            //    for (int x = -width; x <= width; x++)
            //Vector3 zeroCell = grid.GetCellCenterWorld(Vector3Int.zero);
            foreach (Transform child in grid.transform)
            {
                string[] arr = child.name.Split(" ");
                Vector3Int coords =
                    new(Int32.Parse(arr[2]), Int32.Parse(arr[3]));
                //var type = Enum.Parse<BoostType>(arr[0]);
                //var worldPos = grid.GetCellCenterWorld(coords);
                //var timeLeft = (worldPos == zeroCell ||
                //    Vector3.Distance(worldPos, zeroCell) > radius) ? 0 :
                //    UnityEngine.Random.Range(0, maxTime);
                //var cell = new BoostCell(coords, type, timeLeft);
                //BoostsList.Add(cell);
                //TimeSpan time = TimeSpan.FromSeconds(cell.TimeLeft);
                //child.GetChild(0).GetComponent<TextMeshProUGUI>().
                //    text = $"X:{coords.x} Y:{coords.y}\n{type}\n" +
                //    time.ToString("hh':'mm':'ss");
                //ColorCell(child, cell.TimeLeft);
                if (coords == Vector3Int.zero)
                    continue;
                int index = BoostsList.FindIndex(b => b.GridCoords == coords);
                child.AddComponent<Button>().onClick.AddListener(() =>
                {
                    BoostsList[index].TimeLeft += addTime;
                    if (BoostsList[index].TimeLeft > maxTime)
                        BoostsList[index].TimeLeft = maxTime;
                });
            }
            //}
        }

        private void Update()
        {
            int counter = -1;
            foreach (var cell in BoostsList)
            {
                counter++;
                if (cell.TimeLeft <= 0 || cell.TimeLeft < Time.deltaTime)
                    continue;
                cell.TimeLeft -= Time.deltaTime;
                if (!grid.gameObject.activeSelf) continue;
                Transform canvasCell = grid.transform.GetChild(counter);
                ColorCell(canvasCell, cell.TimeLeft);
                TimeSpan time = TimeSpan.FromSeconds(cell.TimeLeft);
                canvasCell.GetChild(0).GetComponent<TextMeshProUGUI>().
                    text = $"X:{cell.GridCoords.x} Y:{cell.GridCoords.y}\n" +
                    $"{cell.Type}\n{time.ToString("hh':'mm':'ss")}";
            }
        }

        private void ColorCell(Transform cell, float time)
        {
            Image image = cell.GetComponent<Image>();
            Color color = image.color;
            foreach (var pair in ColorByTime)
            {
                if (time > pair.Time) continue;
                color = pair.Color;
                break;
            }
            image.color = color;
        }
    }
}