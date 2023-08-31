using UnityEngine;

public class LevelSystem
{
    public int Level { get; private set; }
    public int Experience { get; private set; }

    private int[] experiencePerLevel;

    public int NextLevelExperience =>
        experiencePerLevel[IsMaxLvl() ? Level - 1 : Level];

    public LevelSystem(int[] levelsXp)
    {
        Level = Experience = 0;
        experiencePerLevel = levelsXp;
    }

    public bool AddExperience(int xp)
    {
        if (IsMaxLvl())
            return false;
        Experience += xp;
        if (Experience < NextLevelExperience)
            return false;
        Level++;
        Experience = 0;
        return true;
    }

    public bool IsMaxLvl()
    {
        return Level == experiencePerLevel.Length;
    }
}