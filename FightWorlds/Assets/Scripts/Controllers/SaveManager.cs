using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace FightWorlds.Controllers
{
    public static class SaveManager
    {
        private const string saveFile = "info.dat";
        private const int startCredits = 10;
        private const int startLvl = 1;
        private const int startXp = 0;
        private const int startRecord = 0;
        private const int startBots = 1;
        private const int startUnitsAmount = 0;
        private const int startUnitsLevel = 1;

        public static void Save(PlayerInfo player)
        {
            string savePath =
                Path.Combine(Application.persistentDataPath, saveFile);
            BinaryFormatter formatter = new();
            using (FileStream stream = new(savePath, FileMode.OpenOrCreate))
                formatter.Serialize(stream, player);
        }

        public static PlayerInfo Load()
        {
            string savePath =
                Path.Combine(Application.persistentDataPath, saveFile);
            if (!File.Exists(savePath))
                return Reset();
            BinaryFormatter formatter = new();
            try
            {
                using (FileStream stream = new(savePath, FileMode.Open))
                {
                    PlayerInfo info =
                    formatter.Deserialize(stream) as PlayerInfo;
                    if (info.UnitsLevel == 0) // check for wrong load
                        return Reset();
                    return info;
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                File.Delete(savePath);
                return Reset();
            }
        }

        public static PlayerInfo Reset()
        {
            PlayerInfo startInfo =
                new(startLvl, startXp, startCredits, startRecord, startRecord,
                startBots, startUnitsAmount, startUnitsAmount, startUnitsLevel, null);
            Save(startInfo);
            return startInfo;
        }
    }
}