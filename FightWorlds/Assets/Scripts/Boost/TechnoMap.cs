using System;
using System.Collections.Generic;
using FightWorlds.Placement;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FightWorlds.Boost
{
    public class TechnoMap : MonoBehaviour
    {
        [SerializeField] private BoostTimeExpire[] ColorByTime;
        [SerializeField] private UnityEngine.Grid grid;
        [SerializeField] private Transform activePanel;
        [SerializeField] private TextMeshProUGUI textBoostsCounter;
        [SerializeField] private GameObject hexPrefab;
        [SerializeField] private Vector2Int size;
        [SerializeField] private int radius;
        [SerializeField] private PlacementSystem placement;

        private const int maxTime = 86400; // day in sec
        private const int defaultAddTime = 10800; // 3 hours
        private const int boostPercentMltpl = 25; // 3 hours

        private static Vector3Int selectedCell;

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

        public void UpdateTime(int result)
        {
            if (selectedCell == Vector3Int.zero)
                return;
            int index = BoostsList.FindIndex(b => b.GridCoords == selectedCell);
            int addTime = 0;
            switch (result)
            {
                case 1:
                    addTime = defaultAddTime;
                    break;
                case 2:
                    addTime = defaultAddTime * 4;
                    break;
                case 3:
                    addTime = maxTime;
                    break;
            }
            BoostsList[index].TimeLeft += addTime;
            if (BoostsList[index].TimeLeft > maxTime)
                BoostsList[index].TimeLeft = maxTime;
            placement.player.RegularSave();
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

        private void Awake()
        {
            //grid.cellSize = hexPrefab.GetComponent<RectTransform>().rect.size;
            if (!PlacementSystem.AttackMode) selectedCell = Vector3Int.zero;
            foreach (Transform child in grid.transform)
            {
                string[] arr = child.name.Split(" ");
                Vector3Int coords =
                    new(Int32.Parse(arr[2]), Int32.Parse(arr[3]));
                AddCellListener(coords, child);
                UpdateEachCell();
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
                if (cell.TimeLeft <= 0)
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
                    .text = $"+{boost.Value * boostPercentMltpl}%";
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
                placement.player.RegularSave();
                PlacementSystem.AttackMode = true;
                selectedCell = coords;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            });
        }

        private double GetCurrentSec() =>
            DateTime.Now.Subtract(DateTime.MinValue).TotalSeconds;
    }
}