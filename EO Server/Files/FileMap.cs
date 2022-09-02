using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace EO_Server
{
    public static class FileMap
    {
        public static string mapFolderPath = Directory.GetCurrentDirectory() + "/maps/";

        public static MapContainer[] LoadMaps()
        {
            string[] mapFiles = Directory.GetFiles(mapFolderPath);

            if (mapFiles.Length == 0)
                return null;

            MapContainer[] containers = new MapContainer[mapFiles.Length];
            int k = 0;

            for (int i = 0; i < mapFiles.Length; i++)
            {
                string filePath = mapFiles[i];

                string[] split = filePath.Split('.');
                if (split.Length > 1)
                {

                    if (split[split.Length - 1] == "txt")
                    {
                        //remove .txt ending
                        string noFileEnding = filePath.Remove(filePath.Length - 1 - 3, 4);

                        MapContainer container = ReadMapFromFile(noFileEnding, true);

                        if (container != null)
                        {
                            containers[k++] = container;
                            /*
                            GameObject map = Instantiate(mapPrefab, Vector3.zero, Quaternion.identity);
                            map.GetComponent<NetworkObject>().Spawn(true);
                            EOMap eoMap = map.GetComponent<EOMap>();
                            eoMap.LoadMap(container, (k == 0));


                            map.name = "Map" + container.mapId.ToString();
                            */
                            Console.WriteLine($"Successfully loaded in map file: {noFileEnding}");
                        }
                    }

                }

            }

            if (k > 0)
            {
                Array.Resize(ref containers, k);
                return containers;
            }

            return null;
        }

        public static MapContainer ReadMapFromFile(string fileName, bool isFullPath)
        {
            string filePath;

            if (isFullPath)
                filePath = fileName + ".txt";
            else
                filePath = mapFolderPath + fileName + ".txt";

            try
            {
                if (File.Exists(filePath))
                {
                    string text = File.ReadAllText(filePath);
                    //var span = new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes(text));
                    //var utf8Reader = new Utf8JsonReader(Encoding.ASCII.GetBytes(text));
                    //Console.WriteLine(text);


                    // MapContainer container = JsonSerializer.Deserialize<MapContainer>(ref utf8Reader);
                    MapContainer container = JsonConvert.DeserializeObject<MapContainer>(text);
                    //Debug.Log($"Successfully loaded map {container.mapName}");
                    if (VerifyMapContainer(container))
                    {
                        Console.WriteLine($"Map id:{container.mapId}, width:{container.width}, height:{container.height}");

                        return container;
                    }
                    else
                    {
                        Console.WriteLine($"Error found with loading in map");

                        return null;
                    }

                }
                else
                {
                    //Debug.LogWarning($"Couldn't find map file {filePath}", this);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return null;
        }

        public static bool VerifyMapContainer(MapContainer container)
        {
            if (container.mapId >= 0)
            {
                if (container.width > 0 && container.height > 0)
                {
                    if (container.mapName != null && container.mapName.Length > 0)
                    {
                        //Verify layers
                        int i = 0;
                        int maxX = container.minX + container.width;
                        int maxY = container.minY + container.height;

                        for (int x = container.minX; x < maxX; x++)
                        {
                            for (int y = container.minY; y < maxY; y++)
                            {
                                int tileId = container.groundLayer[i++];

                                //TODO: Fill in

                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
