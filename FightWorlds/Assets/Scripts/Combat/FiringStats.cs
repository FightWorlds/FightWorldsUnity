namespace FightWorlds.Combat
{
    public struct FiringStats
    {
        public int Damage;
        public int Rate;
        public int Strength;
        public int Range;

        public FiringStats(int damage, int rate, int strength, int range)
        {
            Damage = damage;
            Rate = rate;
            Strength = strength;
            Range = range;
        }

        public static FiringStats operator *(FiringStats a, int b)
        =>
        new FiringStats(a.Damage * b, a.Rate * b, a.Strength * b, a.Range * b);

        public static FiringStats operator +(FiringStats stats,
        FiringStats stats2)
        =>
        new FiringStats(stats.Damage + stats2.Damage, stats.Rate + stats2.Rate,
            stats.Strength + stats2.Strength, stats.Range + stats2.Range);
    }
}