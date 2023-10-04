using UnityEngine;

namespace FightWorlds.Grid
{
    public class GridObject
    {
        public Transform Hex;
        public bool HasBuilding;
        public bool IsFilled;
        public int X { get; private set; }
        public int Z { get; private set; }

        public GridObject(int x, int z)
        {
            this.X = x;
            this.Z = z;
            this.IsFilled = false;
            this.HasBuilding = false;
        }

        public void FillHex()
        {
            if (IsFilled)
                return;
            IsFilled = true;
        }
    }
}