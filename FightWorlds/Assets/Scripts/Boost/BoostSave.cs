using System;
using System.Collections.Generic;

namespace FightWorlds.Boost
{
    [Serializable]
    public class BoostsSave
    {
        public List<Boost> Boosts;
        public BoostsSave(List<Boost> list) =>
            Boosts = list;
    }
}