namespace FightWorlds.Controllers
{
    [System.Serializable]
    public class PlayerInfo
    {
        public int Level;
        public int Experience;
        public int Credits;
        public int Record;
        public int Bots;
        public PlayerInfo(int level, int experience,
        int credits, int record, int bots)
        {
            Level = level;
            Experience = experience;
            Credits = credits;
            Record = record;
            Bots = bots;
        }
    }
}