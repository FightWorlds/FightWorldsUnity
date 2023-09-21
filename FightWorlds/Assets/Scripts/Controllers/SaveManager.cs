using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveManager
{
    private const string saveFile = "/info.dat";
    private const int startCredits = 10;
    private const int startLvl = 1;
    private const int startXp = 0;
    private const int startRecord = 0;

    public static void Save(PlayerInfo player)
    {
        string savePath =
            Path.Combine(Application.persistentDataPath, saveFile);
        BinaryFormatter formatter = new();
        using (FileStream stream = new(savePath, FileMode.Create))
            formatter.Serialize(stream, player);
    }

    public static PlayerInfo Load()
    {
        string savePath =
            Path.Combine(Application.persistentDataPath, saveFile);
        if (!File.Exists(savePath))
            return Reset();
        BinaryFormatter formatter = new();
        using (FileStream stream = new(savePath, FileMode.Open))
        {
            PlayerInfo info = formatter.Deserialize(stream) as PlayerInfo;
            return info;
        }
    }

    public static PlayerInfo Reset()
    {
        PlayerInfo startInfo =
                new(startLvl, startXp, startCredits, startRecord);
        Save(startInfo);
        return startInfo;
    }
}