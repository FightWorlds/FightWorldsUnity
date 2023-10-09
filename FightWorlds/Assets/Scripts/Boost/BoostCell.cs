using System;
using UnityEngine;

namespace FightWorlds.Boost
{
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
}