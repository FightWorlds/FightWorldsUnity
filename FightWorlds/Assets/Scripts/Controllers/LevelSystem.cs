using UnityEngine;

public class LevelSystem
{
    public int Level { get; private set; }
    public int Experience { get; private set; }

    private const int maxLevel = 30;

    private float mainLevelFormula => Mathf.Round(Mathf.Log10(5 * Level + 1));
    public int NextLevelExperience => (int)(((Level <= 15) ?
    mainLevelFormula :
    mainLevelFormula + (Level - 15)) *
    Mathf.Round(500 * Mathf.Log10(5 * Level)));

    public LevelSystem()
    {
        Level = 1;
        Experience = 0;
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
        return Level == maxLevel;
    }
}