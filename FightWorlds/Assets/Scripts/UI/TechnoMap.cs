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
        public double TimeLeft;

        public BoostCell(Vector3Int coordinates,
        BoostType type, double time)
        {
            GridCoords = coordinates;
            Type = type;
            TimeLeft = time;
        }
    }

    [Serializable]
    public struct BoostTimeExpire
    {
        public double Time;
        public Color Color;
    }

    [Serializable]
    public class Boost
    {
        public Vector3Int Coords;
        public double PassTime;
        public Boost(Vector3Int coords, double time)
        {
            Coords = coords;
            PassTime = time;
        }
    }

    [Serializable]
    public class BoostsSave
    {
        public List<Boost> Boosts;
        public BoostsSave(List<Boost> list) =>
            Boosts = list;
    }

    public class TechnoMap : MonoBehaviour
    {
        [SerializeField] private BoostTimeExpire[] ColorByTime;
        [SerializeField] private UnityEngine.Grid grid;
        [SerializeField] private Transform activePanel;
        [SerializeField] private TextMeshProUGUI textBoostsCounter;
        [SerializeField] private GameObject hexPrefab;
        [SerializeField] private Vector2Int size;
        [SerializeField] private int radius;

        private const float maxTime = 86400; // day in sec
        private const float addTime = 10800; // 3 hours

        public List<BoostCell> BoostsList;

        public Dictionary<BoostType, int> ActiveBoosts { get; private set; }

        public bool LoadBoosts(BoostsSave save)
        {
            try
            {
                int counter = 0;
                var currentSec = GetCurrentSec();
                foreach (var boost in BoostsList)
                {
                    boost.TimeLeft =
                        save.Boosts[counter].PassTime - currentSec;
                    if (boost.TimeLeft < 0) boost.TimeLeft = 0;
                    counter++;
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
                return false;
            }
        }

        public BoostsSave SaveBoosts(bool defaultBoosts)
        {
            List<Boost> list = new List<Boost>();
            foreach (var boost in BoostsList)
                list.Add(new(boost.GridCoords,
                    defaultBoosts ?
                    GetCurrentSec() :
                    GetCurrentSec() + boost.TimeLeft));
            BoostsSave save = new BoostsSave(list);
            if (defaultBoosts) LoadBoosts(save);
            return save;
        }

        private void Start()
        {
            //grid.cellSize = hexPrefab.GetComponent<RectTransform>().rect.size;
            foreach (Transform child in grid.transform)
            {
                string[] arr = child.name.Split(" ");
                Vector3Int coords =
                    new(Int32.Parse(arr[2]), Int32.Parse(arr[3]));
                AddCellListener(coords, child);
            }
        }

        private void Update()
        {
            UpdateEachCell();
            FillBoostsPanel();
        }

        private void UpdateEachCell()
        {
            int counter = -1;
            ActiveBoosts = new() {
                {BoostType.Damage, 0},
                {BoostType.Range, 0},
                {BoostType.Rate, 0},
                {BoostType.Health, 0},
            };
            foreach (var cell in BoostsList)
            {
                counter++;
                if (cell.TimeLeft <= 0)
                    continue;
                cell.TimeLeft -= Time.deltaTime;
                if (cell.TimeLeft < 0)
                    cell.TimeLeft = 0;
                else
                    ActiveBoosts[cell.Type]++;

                if (!grid.gameObject.activeSelf) continue;
                Transform canvasCell = grid.transform.GetChild(counter);
                ColorCell(canvasCell, cell.TimeLeft);
                TimeSpan time = TimeSpan.FromSeconds(cell.TimeLeft);
                canvasCell.GetChild(0).GetComponent<TextMeshProUGUI>().
                    text = $"X:{cell.GridCoords.x} Y:{cell.GridCoords.y}\n" +
                    $"{cell.Type}\n{time.ToString("hh':'mm':'ss")}";
            }
        }

        private void FillBoostsPanel()
        {
            int counter = 0;
            int boostsCounter = 0;
            foreach (var boost in ActiveBoosts) // change to events
            {
                counter++; // first==1 because in panel we have collapse button
                Transform panelElement = activePanel.GetChild(counter);
                if (boost.Value == 0)
                    panelElement.gameObject.SetActive(false);
                else
                {
                    panelElement.gameObject.SetActive(true);
                    panelElement.GetChild(1).GetComponent<TextMeshProUGUI>()
                    .text = $"+{boost.Value}%";
                    boostsCounter++;
                }
            }
            textBoostsCounter.text = boostsCounter.ToString();
        }

        private void ColorCell(Transform cell, double time)
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

        private void AddCellListener(Vector3Int coords, Transform cell)
        {
            if (coords == Vector3Int.zero)
                return;
            int index = BoostsList.FindIndex(b => b.GridCoords == coords);
            cell.AddComponent<Button>().onClick.AddListener(() =>
            {
                BoostsList[index].TimeLeft += addTime;
                if (BoostsList[index].TimeLeft > maxTime)
                    BoostsList[index].TimeLeft = maxTime;
            });
        }

        private double GetCurrentSec() =>
            DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
    }
}