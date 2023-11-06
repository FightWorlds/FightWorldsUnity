using System;
using System.Collections.Generic;
using UnityEngine;

namespace FightWorlds.Placement
{
    [Serializable]
    public class BaseSave
    {
        public List<StartBuilding> Buildings;
        public List<Vector3> Platforms;
        public BaseSave(List<StartBuilding> buildings, List<Vector3> platforms)
        {
            Buildings = buildings;
            Platforms = platforms;
        }
    }
}