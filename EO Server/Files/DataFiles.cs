using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EO_Server
{
    public static class DataFiles
    {
       
        public static string dataFileDirPath = Directory.GetCurrentDirectory() + "/data/";
        public static NpcDataFile npcDataFile;
        public static ItemDataFile itemDataFile;


        public static void LoadDataFiles()
        {
            Console.WriteLine("Loading in data files");
            npcDataFile = ReadNpcDataFile();
            itemDataFile = ReadItemDataFile();
            Console.WriteLine("Finished loading in data files");
        }

        public static void SaveExample()
        {
            NpcDataFile dataFile = new NpcDataFile();
            dataFile.EO_VERSION = "0.1";
            dataFile.Entries = new List<NpcDataEntry>();
            dataFile.Entries.Add(new NpcDataEntry { GfxId = 0, Name = "Sheep", NpcType = (int)NpcType.MOB_PASSIVE, MaxHealth = 10 });
            SaveDataFile(dataFile, true);

        }

        public static ItemDataEntry GetItemData(int itemId)
        {
            return itemDataFile.entries[itemId];
        }

        public static NpcDataFile ReadNpcDataFile()
        {
            string filePath = dataFileDirPath + "dat001.txt";
            NpcDataFile dataFile;

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                if (json != null && json.Length > 0)
                {
                    dataFile = JsonConvert.DeserializeObject<NpcDataFile>(json);
                    return dataFile;
                }
            }

            return null;
        }

        public static ItemDataFile ReadItemDataFile()
        {
            string filePath = dataFileDirPath + "dat002.txt";
            ItemDataFile dataFile;

            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);

                if (json != null && json.Length > 0)
                {
                    dataFile = JsonConvert.DeserializeObject<ItemDataFile>(json);
                    return dataFile;
                }
            }

            return null;
        }

        public static void SaveDataFile(NpcDataFile dataFile, bool overwrite)
        {
            string outputFilePath = dataFileDirPath + "dat001.txt";
            Directory.CreateDirectory(dataFileDirPath);

            if (File.Exists(outputFilePath) && !overwrite)
            {
                Console.WriteLine($"Failed to save Npc data file to {outputFilePath} because of existing data file.");

                return;
            }

            string json = JsonConvert.SerializeObject(dataFile);
            File.WriteAllText(outputFilePath, json);

            Console.WriteLine("Saved data file");
        }
    }
}
