using UnityEngine;

public static class SaveManager
{
    private const string currentLvl = "PlayerCurrentLevel";
    private const string currentXp = "PlayerCurrentExperience";
    private const string currentCredits = "PlayerCurrentCredits";
    private const string currentRecord = "PlayerCurrentRecord";
    private const int startCredits = 10;
    private const int startLvl = 1;
    private const int startXp = 0;
    private const int startRecord = 0;

    // WARN for WebGL!!!
    // Unity stores up to 1MB of PlayerPrefs using the browser's IndexedDB API

    public static void Save(PlayerInfo player)
    {
        PlayerPrefs.SetInt(currentLvl, player.Level);
        PlayerPrefs.SetInt(currentXp, player.Experience);
        PlayerPrefs.SetInt(currentCredits, player.Credits);
        PlayerPrefs.SetInt(currentRecord, player.Record);
    }

    public static PlayerInfo Load() =>
    new PlayerInfo(
        PlayerPrefs.GetInt(currentLvl, startLvl),
        PlayerPrefs.GetInt(currentXp, startXp),
        PlayerPrefs.GetInt(currentCredits, startCredits),
        PlayerPrefs.GetInt(currentRecord, startRecord));

    public static PlayerInfo Reset()
    {
        PlayerInfo startInfo =
            new PlayerInfo(startLvl, startXp, startCredits, startRecord);
        Save(startInfo);
        return startInfo;
    }
}