using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public static class MapManager
    {
        private static Dictionary<uint, EOMap> maps;

        public static void CreateMaps()
        {
            MapContainer[] containers = FileMap.LoadMaps();
            maps = new Dictionary<uint, EOMap>(containers.Length);

            foreach (MapContainer container in containers)
            {
                EOMap map = new EOMap(container);
                if (map.isLoaded)
                {
                    try
                    {
                        maps.Add(map.mapId, map);
                    }
                    catch(ArgumentException)
                    {
                        Console.WriteLine("Found map with a duplicate mapId. Ignoring.");
                    }
                }
                //Console.WriteLine($"Map id:{container.mapId}, width:{container.width}, height:{container.height}");
            }
            
            maps.TrimExcess();
        }

        public static void Update()
        {
            foreach(var map in maps.Values)
            {
                map.Update();
            }
        }

        public static void WarpTo(Character character, EOMap from, EOMap to, Vector2 pos, uint dir)
        {
            Console.WriteLine($"Warping character from mapId {from.mapId} to {to.mapId}");
            from.RemoveEntity(character);
            to.WarpPlayer(character, pos, dir);
        }

        public static EOMap GetMap(uint mapId)
        {
           try
            {
                EOMap map = maps[mapId];
                return map;
            }
           catch(KeyNotFoundException) { }

            return null;
        }

    }
}
