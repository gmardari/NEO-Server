using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    //TODO: change entries key type to uint
    public class NpcDataFile
    {
        public string EO_VERSION { get; set; }
        public List<NpcDataEntry> Entries { get; set; }

    }

    //TODO: Change ints to uint
    public class NpcDataEntry
    {
        public int GfxId { get; set; }
        public int NpcType { get; set; }
        public string Name { get; set; }
        public long MaxHealth { get; set; }
    }

    public class ItemDataFile
    {
        //The formatting of the data file
        public string dataFileVersion;

        public List<ItemDataEntry> entries;
    }

    public class ItemDataEntry
    {
        public string name;
        public uint displayGfx;
        public uint bodyGfx;
        public uint itemType;
        public uint sizeX;
        public uint sizeY;
    }


    public struct MapNpcSpawnInfo
    {
        public int npcId;
        public int respawnTimeMin;
        public int respawnTimeMax;
        public int fidgetTimeMin;
        public int fidgetTimeMax;
    }


    public struct MapWarpInfo
    {
        public int mapId;
        public int x;
        public int y;
        public int direction;
    }

    public struct MapChestInfo
    {
        public string name;
        public List<MapChestSpawnInfo> spawnList;
    }

    public struct MapChestSpawnInfo
    {
        public uint itemId;
        public IntRange quantityRange;
        public LongRange respawnRange; //Measured in milliseconds
    }

    public class MapSpecialLayerInfo
    {
        public int[] tiles;
        public List<MapNpcSpawnInfo> npcSpawnList;
        public List<MapWarpInfo> warpList;
        public List<MapChestInfo> chestList;

        public MapSpecialLayerInfo(int numTiles)
        {
            tiles = new int[numTiles];
        }
    }

    public class MapContainer
    {
        public int mapId;
        public string mapName;
        public string eo_version;
        //measured in the x axis
        public int width;
        //measured in the y axis
        public int height;
        public int minX;
        public int minY;
        public int[] groundLayer;
        public int[] objectsLayer;
        public int[] overlayLayer;
        public int[] wallsDownLayer;
        public int[] wallsRightLayer;
        public MapSpecialLayerInfo specialLayer;
    }

   

}
