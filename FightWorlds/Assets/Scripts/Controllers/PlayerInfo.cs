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
        public PlayerInfo(int level, int experience,
        int credits, int artifacts, int record, int bots,
        int units, int unitsToHeal, int unitsLevel)
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
        }
    }
}