using System;
using System.Collections.Generic;
using UnityEngine;
namespace FightWorlds.Placement
{
    [CreateAssetMenu(fileName = "BuildingsDatabase",
    menuName = "ScriptableObjects/BuildingsDatabase")]
    public class BuildingsDatabase : ScriptableObject
    {
        public List<BuildingData> objectsData;
    }

    [Serializable]
    public class BuildingData
    {
        [field: SerializeField] public string Name { get; private set; }
        [field: SerializeField] public int ID { get; private set; }
        [field: SerializeField] public int Cost { get; private set; }
        [field: SerializeField] public GameObject Prefab { get; private set; }
        [field: SerializeField] public int MaxBuildingsAdd { get; private set; }
        [field: SerializeField]
        public int MaxBuildingsPerLevel { get; private set; }
    }
}