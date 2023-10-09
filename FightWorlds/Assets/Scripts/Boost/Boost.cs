using System;
using UnityEngine;
namespace FightWorlds.Boost
{
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
}