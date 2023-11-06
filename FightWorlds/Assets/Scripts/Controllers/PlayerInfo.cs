namespace FightWorlds.Controllers
{
    [System.Serializable]
    public class PlayerInfo
    {
        public int Level;
        public int Experience;
        public int Credits;
        public int Artifacts;
        public int Record;
        public int Bots;
        public int Units;
        public int UnitsToHeal;
        public int UnitsLevel;
        public string Boosts;
        public string Upgrades;
        public string Base;
        public PlayerInfo(int level, int experience, int credits,
        int artifacts, int record, int bots, int units, int unitsToHeal,
        int unitsLevel, string boosts, string upgrades, string baseStr)
        {
            Level = level;
            Experience = experience;
            Credits = credits;
            Artifacts = artifacts;
            Record = record;
            Bots = bots;
            Units = units;
            UnitsToHeal = unitsToHeal;
            UnitsLevel = unitsLevel;
            Boosts = boosts;
            Upgrades = upgrades;
            Base = baseStr;
        }
    }
}