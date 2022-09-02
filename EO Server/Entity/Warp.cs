using System;
using System.Collections.Generic;
using System.Text;

namespace EO_Server
{
    public class Warp : Entity
    {
        public uint warpMapId;
        public Vector2 warpPos;
        public uint warpDir;

        private Cell cell;

        public Warp(EOMap map, Vector2 pos, uint warpMapId, Vector2 warpPos, uint warpDir) : base(map, pos)
        {
            this.position = pos;
            this.warpMapId = warpMapId;
            this.warpPos = warpPos;
            this.warpDir = warpDir;
            this.cell = map.GetCell(this.position);
            this.entityType = EntityType.WARP;
        }

        public override void Update()
        {
            for(var node = cell.entities.First; node != null; node = node.Next)
            {
                if(node.Value is Character character)
                {
                    MapManager.WarpTo(character, map, MapManager.GetMap(warpMapId), warpPos, warpDir);
                    break;
                }
            }
        }
    }
}
