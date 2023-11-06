using System;
using System.Collections.Generic;

namespace FightWorlds.Placement
{
    [Serializable]
    public class UpgradesSave
    {
        public List<int> Saves;
        public UpgradesSave(List<int> list) =>
            Saves = list;
    }
}