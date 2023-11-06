using System;
using UnityEngine;
namespace FightWorlds.Placement
{
    [Serializable]
    public class StartBuilding
    {
        public Vector3 Position;
        public int YRotationAngle;
        public int ID;

        public StartBuilding(Vector3 position, int angle, int id)
        {
            Position = position;
            YRotationAngle = angle;
            ID = id;
        }

        public StartBuilding(Building building)
        {
            Position = building.transform.position;
            YRotationAngle = (int)building.transform.GetChild(0).rotation.y;
            ID = building.BuildingData.ID;
        }
    }
}